using System.Globalization;
using System.Text;

namespace RayMmdNodeEditor.Services;

public static class FxFileWriter
{
    public const string AutoEncoding = "auto";
    public const string ShiftJisEncoding = "shift-jis";
    public const string GbkEncoding = "gbk";
    public const string Utf8Encoding = "utf-8";

    public static string NormalizeEncoding(string? encodingName)
    {
        return encodingName?.Trim().ToLowerInvariant() switch
        {
            AutoEncoding => AutoEncoding,
            GbkEncoding => GbkEncoding,
            Utf8Encoding => Utf8Encoding,
            _ => ShiftJisEncoding,
        };
    }

    public static Encoding ResolveEncoding(string? encodingName)
    {
        return NormalizeEncoding(encodingName) switch
        {
            AutoEncoding => ResolveBestEffortEncoding(string.Empty, ShiftJisEncoding),
            GbkEncoding => Encoding.GetEncoding(936),
            Utf8Encoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            _ => Encoding.GetEncoding(932),
        };
    }

    public static Encoding ResolveEncodingForContents(string contents, string? encodingName)
    {
        return ResolveBestEffortEncoding(contents, NormalizeEncoding(encodingName));
    }

    public static void WriteFx(string path, string contents, string? encodingName)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var encoding = ResolveEncodingForContents(contents, encodingName);
        var normalizedContents = NormalizeContentsForEncoding(contents, encoding);
        File.WriteAllText(path, normalizedContents, encoding);
    }

    public static bool TryWriteTemplateFx(string relativePath, string destinationPath, string? encodingName)
    {
        if (!AppResourceLoader.TryReadText(relativePath, out var contents))
        {
            return false;
        }

        WriteFx(destinationPath, contents, encodingName);
        return true;
    }

    private static Encoding ResolveBestEffortEncoding(string contents, string preferredEncoding)
    {
        if (preferredEncoding == Utf8Encoding)
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }

        var shiftJis = CreateStrictEncoding(932);
        var gbk = CreateStrictEncoding(936);
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        var localePreferredEncoding = ResolveLocalePreferredEncoding();
        var containsKana = ContainsJapaneseKana(contents);
        var containsHan = ContainsCjkIdeographs(contents);
        var canEncodeShiftJis = CanEncode(contents, shiftJis);
        var canEncodeGbk = CanEncode(contents, gbk);

        return preferredEncoding switch
        {
            GbkEncoding => canEncodeGbk ? Encoding.GetEncoding(936) :
                canEncodeShiftJis ? Encoding.GetEncoding(932) :
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            AutoEncoding => ResolveAutoEncoding(contents, containsKana, containsHan, canEncodeShiftJis, canEncodeGbk, localePreferredEncoding),
            _ => canEncodeShiftJis ? Encoding.GetEncoding(932) :
                canEncodeGbk ? Encoding.GetEncoding(936) :
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
        };
    }

    private static Encoding ResolveAutoEncoding(
        string contents,
        bool containsKana,
        bool containsHan,
        bool canEncodeShiftJis,
        bool canEncodeGbk,
        string localePreferredEncoding)
    {
        if (containsHan && canEncodeGbk)
        {
            return Encoding.GetEncoding(936);
        }

        if (containsKana && canEncodeShiftJis)
        {
            return Encoding.GetEncoding(932);
        }

        if (string.Equals(localePreferredEncoding, GbkEncoding, StringComparison.Ordinal))
        {
            if (canEncodeGbk)
            {
                return Encoding.GetEncoding(936);
            }

            if (canEncodeShiftJis)
            {
                return Encoding.GetEncoding(932);
            }
        }
        else
        {
            if (canEncodeShiftJis)
            {
                return Encoding.GetEncoding(932);
            }

            if (canEncodeGbk)
            {
                return Encoding.GetEncoding(936);
            }
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    }

    private static string NormalizeContentsForEncoding(string contents, Encoding encoding)
    {
        if (encoding.CodePage is not 932 and not 936)
        {
            return contents;
        }

        if (CanEncode(contents, CreateStrictEncoding(encoding.CodePage)))
        {
            return contents;
        }

        var builder = new StringBuilder(contents.Length);
        var segment = new StringBuilder();
        var strictEncoding = CreateStrictEncoding(encoding.CodePage);
        var inFallbackSegment = false;

        void FlushSegment()
        {
            if (segment.Length == 0)
            {
                return;
            }

            if (inFallbackSegment)
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(segment.ToString());
                builder.Append(encoding.GetString(utf8Bytes));
            }
            else
            {
                builder.Append(segment);
            }

            segment.Clear();
        }

        foreach (var ch in contents)
        {
            var canEncodeChar = CanEncode(ch.ToString(), strictEncoding);
            if (canEncodeChar == !inFallbackSegment)
            {
                segment.Append(ch);
                continue;
            }

            FlushSegment();
            inFallbackSegment = !canEncodeChar;
            segment.Append(ch);
        }

        FlushSegment();
        return builder.ToString();
    }

    private static string ResolveLocalePreferredEncoding()
    {
        var cultureName = CultureInfo.CurrentUICulture.Name;
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            cultureName = CultureInfo.CurrentCulture.Name;
        }

        if (cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return GbkEncoding;
        }

        if (cultureName.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
        {
            return ShiftJisEncoding;
        }

        return ShiftJisEncoding;
    }

    private static bool ContainsJapaneseKana(string contents)
    {
        foreach (var ch in contents)
        {
            if ((ch >= '\u3040' && ch <= '\u309F') ||
                (ch >= '\u30A0' && ch <= '\u30FF') ||
                (ch >= '\u31F0' && ch <= '\u31FF') ||
                (ch >= '\uFF66' && ch <= '\uFF9D'))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsCjkIdeographs(string contents)
    {
        foreach (var ch in contents)
        {
            if ((ch >= '\u3400' && ch <= '\u4DBF') ||
                (ch >= '\u4E00' && ch <= '\u9FFF') ||
                (ch >= '\uF900' && ch <= '\uFAFF'))
            {
                return true;
            }
        }

        return false;
    }

    private static Encoding CreateStrictEncoding(int codePage)
    {
        return Encoding.GetEncoding(
            codePage,
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback);
    }

    private static bool CanEncode(string contents, Encoding encoding)
    {
        try
        {
            encoding.GetBytes(contents);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

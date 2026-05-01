using System.Text.RegularExpressions;

namespace RayMmdNodeEditor.Services;

public static class RayConfigWriter
{
    public static string ApplyDefines(string originalText, IReadOnlyDictionary<string, string> values)
    {
        var result = originalText;
        foreach (var pair in values)
        {
            var pattern = $@"(?m)^(?<indent>\s*)#define\s+{Regex.Escape(pair.Key)}\s+.*$";
            var replacement = $"${{indent}}#define {pair.Key} {pair.Value}";
            if (Regex.IsMatch(result, pattern))
            {
                result = Regex.Replace(result, pattern, replacement);
            }
            else
            {
                result += $"{Environment.NewLine}#define {pair.Key} {pair.Value}";
            }
        }

        return result;
    }

    public static string ApplyStaticConstFloats(string originalText, IReadOnlyDictionary<string, string> values)
    {
        var result = originalText;
        foreach (var pair in values)
        {
            var pattern = $@"(?m)^(?<indent>\s*)static\s+const\s+float\s+{Regex.Escape(pair.Key)}(?<suffix>\s*(?:\[[^\]]+\])?)\s*=\s*[^;]+;";
            var replacement = $"${{indent}}static const float {pair.Key}${{suffix}} = {pair.Value};";
            if (Regex.IsMatch(result, pattern))
            {
                result = Regex.Replace(result, pattern, replacement);
            }
            else
            {
                result += $"{Environment.NewLine}static const float {pair.Key} = {pair.Value};";
            }
        }

        return result;
    }

    public static string ApplyMaterialCommonDefines(string originalText, IReadOnlyDictionary<string, string> values)
    {
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            normalized[pair.Key] = NormalizeMaterialCommonValue(pair.Key, pair.Value);
        }

        return ApplyDefines(originalText, normalized);
    }

    public static string NormalizeMaterialCommonValue(string key, string value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return key switch
        {
            "TEXTURE_FILTER" or "TEXTURE_MIP_FILTER" => NormalizeTextureFilter(trimmed),
            "TEXTURE_ANISOTROPY_LEVEL" => NormalizeAnisotropyLevel(trimmed),
            "ALPHA_THRESHOLD" => NormalizeAlphaThreshold(trimmed),
            _ => trimmed,
        };
    }

    private static string NormalizeTextureFilter(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "POINT" or "0" => "POINT",
            "LINEAR" or "1" => "LINEAR",
            "ANISOTROPIC" or "ANISO" or "2" => "ANISOTROPIC",
            _ => "ANISOTROPIC",
        };
    }

    private static string NormalizeAnisotropyLevel(string value)
    {
        if (!int.TryParse(value, out var parsed))
        {
            return "16";
        }

        return Math.Clamp(parsed, 1, 16).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string NormalizeAlphaThreshold(string value)
    {
        if (!double.TryParse(
                value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
        {
            return "0.999";
        }

        return Math.Clamp(parsed, 0.0, 1.0).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
    }
}

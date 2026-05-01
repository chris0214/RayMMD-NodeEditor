using System.Text.RegularExpressions;
using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor.Graph;

public static partial class NodeInlineValueCodec
{
    public static string GetStorageKey(string pinName)
    {
        var trimmed = string.IsNullOrWhiteSpace(pinName) ? "Value" : pinName.Trim();
        return $"Inline_{StorageKeySanitizer().Replace(trimmed, "_")}";
    }

    public static bool TryParse(string rawValue, GraphValueType valueType, out float[] values)
    {
        values = [];
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var tokens = rawValue
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return false;
        }

        var componentCount = GetComponentCount(valueType);
        var parsedValues = new float[Math.Max(componentCount, tokens.Length)];
        for (var index = 0; index < tokens.Length; index++)
        {
            if (!FloatParser.TryParse(tokens[index], out var parsed))
            {
                values = [];
                return false;
            }

            parsedValues[index] = parsed;
        }

        if (tokens.Length == 1 && componentCount > 1)
        {
            for (var index = 1; index < componentCount; index++)
            {
                parsedValues[index] = parsedValues[0];
            }
        }

        values = parsedValues[..componentCount];
        return true;
    }

    public static string Format(GraphValueType valueType, IReadOnlyList<float> values)
    {
        var componentCount = GetComponentCount(valueType);
        return string.Join(", ", Enumerable.Range(0, componentCount)
            .Select(index => FloatParser.Format(index < values.Count ? values[index] : 0f)));
    }

    public static string ToHlslLiteral(GraphValueType valueType, IReadOnlyList<float> values)
    {
        var componentCount = GetComponentCount(valueType);
        if (componentCount == 1)
        {
            return FloatParser.Format(values.Count > 0 ? values[0] : 0f);
        }

        var components = string.Join(", ", Enumerable.Range(0, componentCount)
            .Select(index => FloatParser.Format(index < values.Count ? values[index] : 0f)));
        return $"float{componentCount}({components})";
    }

    public static int GetComponentCount(GraphValueType valueType)
    {
        return valueType switch
        {
            GraphValueType.Float1 => 1,
            GraphValueType.Float2 => 2,
            GraphValueType.Float3 => 3,
            _ => 4,
        };
    }

    [GeneratedRegex(@"[^A-Za-z0-9_]+", RegexOptions.Compiled)]
    private static partial Regex StorageKeySanitizer();
}

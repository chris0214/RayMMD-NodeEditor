using System.Text.Json;

namespace RayMmdNodeEditor;

internal static class LocalizationService
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Strings = new(LoadDictionary);

    public static string CurrentLanguage { get; set; } = "zh-CN";

    public static string Get(string key, string fallback)
    {
        return Strings.Value.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    public static bool TryGet(string key, out string value)
    {
        if (Strings.Value.TryGetValue(key, out value!) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        value = string.Empty;
        return false;
    }

    public static string Format(string key, string fallback, params object[] args)
    {
        return string.Format(Get(key, fallback), args);
    }

    private static IReadOnlyDictionary<string, string> LoadDictionary()
    {
        try
        {
            if (!AppResourceLoader.TryReadText(Path.Combine("Localization", $"{CurrentLanguage}.json"), out var json))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

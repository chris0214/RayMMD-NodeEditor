using System.Text.Json;

namespace RayMmdNodeEditor.Services;

public static class RayDocumentPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void Save(RayDocument document, string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static RayDocument Load(string path)
    {
        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<RayDocument>(json, JsonOptions) ?? new RayDocument();
        document.Graph ??= RayGraphFactory.CreateDefault();
        if (string.IsNullOrWhiteSpace(document.MaterialMode))
        {
            document.MaterialMode = RayMaterialModes.Compatible;
        }

        document.RayConfValues ??= RayConfigDefaults.CreateRayConfDefaults();
        document.AdvancedConfValues ??= RayConfigDefaults.CreateAdvancedDefaults();
        document.MaterialCommonValues ??= RayConfigDefaults.CreateMaterialCommonDefaults();
        document.LightingPatchValues ??= RayConfigDefaults.CreateLightingPatchDefaults();
        document.FavoriteParameterKeys ??= [];
        document.FavoriteParameterKeys = document.FavoriteParameterKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToList();
        MergeMissing(document.RayConfValues, RayConfigDefaults.CreateRayConfDefaults());
        MergeMissing(document.AdvancedConfValues, RayConfigDefaults.CreateAdvancedDefaults());
        MergeMissing(document.MaterialCommonValues, RayConfigDefaults.CreateMaterialCommonDefaults());
        MergeMissing(document.LightingPatchValues, RayConfigDefaults.CreateLightingPatchDefaults());
        return document;
    }

    private static void MergeMissing(Dictionary<string, string> target, Dictionary<string, string> defaults)
    {
        foreach (var pair in defaults)
        {
            target.TryAdd(pair.Key, pair.Value);
        }
    }
}

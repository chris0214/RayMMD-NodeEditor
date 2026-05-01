using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public sealed class RayDocument
{
    public string FormatVersion { get; set; } = "1.0";

    public string RayRootPath { get; set; } = string.Empty;

    public string ExportDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "ExportedRayPreset");

    public string MaterialFileName { get; set; } = "material_2.0.fx";

    public NodeGraph Graph { get; set; } = RayGraphFactory.CreateDefault();

    public string QualityPreset { get; set; } = "Balanced";

    public string MaterialMode { get; set; } = RayMaterialModes.Compatible;

    public bool CopyTextureFiles { get; set; } = true;

    public bool ExportFullRayPackage { get; set; } = true;

    public bool AutoExportEnabled { get; set; }

    public List<string> FavoriteParameterKeys { get; set; } = [];

    public Dictionary<string, string> RayConfValues { get; set; } = RayConfigDefaults.CreateRayConfDefaults();

    public Dictionary<string, string> AdvancedConfValues { get; set; } = RayConfigDefaults.CreateAdvancedDefaults();

    public Dictionary<string, string> MaterialCommonValues { get; set; } = RayConfigDefaults.CreateMaterialCommonDefaults();

    public Dictionary<string, string> LightingPatchValues { get; set; } = RayConfigDefaults.CreateLightingPatchDefaults();
}

public static class RayMaterialModes
{
    public const string Compatible = "Compatible";
    public const string Advanced = "Advanced";
}

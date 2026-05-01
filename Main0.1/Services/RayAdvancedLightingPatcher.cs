using System.Globalization;
using System.Text.RegularExpressions;

namespace RayMmdNodeEditor.Services;

public sealed record RayLightingPatchResult(bool Enabled, IReadOnlyList<string> PatchedFiles, IReadOnlyList<string> Warnings);

public static class RayAdvancedLightingPatcher
{
    private const string BeginMarker = "// RAY_MMD_NODE_EDITOR_LIGHTING_BEGIN";
    private const string EndMarker = "// RAY_MMD_NODE_EDITOR_LIGHTING_END";
    private static readonly LightPatchTarget[] DirectionalTargets =
    [
        new("DirectionalLight", "directional_lighting.fxsub", "DirectionalLight.pmx"),
    ];
    private static readonly LightPatchTarget[] MultiLightTargets =
    [
        new("PointLight", "point_lighting.fxsub", "PointLight.pmx"),
        new("SpotLight", "spot_lighting.fxsub", "SpotLight.pmx"),
        new("SphereLight", "sphere_lighting.fxsub", "SphereLight.pmx"),
        new("RectangleLight", "rectangle_lighting.fxsub", "RectangleLight.pmx"),
        new("DiskLight", "disk_lighting.fxsub", "DiskLight.pmx"),
        new("TubeLight", "tube_lighting.fxsub", "TubeLight.pmx"),
        new("PointLightIES", "IES_lighting.fxsub", "PointLightIES.pmx"),
        new("SpotLightIES", "IES_lighting.fxsub", "SpotLightIES.pmx"),
    ];

    public static bool IsEnabled(RayDocument document)
    {
        return IsSwitchOn(document, "ENABLE_DIRECTIONAL_LIGHTING_PATCH") ||
               IsSwitchOn(document, "ENABLE_MULTI_LIGHTING_PATCH");
    }

    public static RayLightingPatchResult ExportDirectionalLightPatch(RayDocument document, string exportRoot)
    {
        if (!IsEnabled(document))
        {
            return new RayLightingPatchResult(false, [], []);
        }

        var warnings = new List<string>();
        var patched = new List<string>();
        var targets = GetTargets(document).ToList();
        if (targets.Count == 0)
        {
            return new RayLightingPatchResult(true, [], []);
        }

        foreach (var target in targets)
        {
            ExportLightTarget(document, exportRoot, target, patched, warnings);
        }

        return new RayLightingPatchResult(true, patched, warnings);
    }

    private static void ExportLightTarget(RayDocument document, string exportRoot, LightPatchTarget target, List<string> patched, List<string> warnings)
    {
        var sourceRoot = Path.Combine(document.RayRootPath, "Lighting", target.FolderName);
        var targetRoot = Path.Combine(exportRoot, "Lighting", target.FolderName);
        if (!Directory.Exists(sourceRoot))
        {
            warnings.Add($"缺失 Ray 灯光目录：{sourceRoot}");
            return;
        }

        CopyDirectory(sourceRoot, targetRoot);
        var sourcePmx = Path.Combine(document.RayRootPath, "Lighting", target.ControllerFileName);
        var targetPmx = Path.Combine(exportRoot, "Lighting", target.ControllerFileName);
        if (File.Exists(sourcePmx))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPmx)!);
            File.Copy(sourcePmx, targetPmx, overwrite: true);
            patched.Add(targetPmx);
        }
        else
        {
            warnings.Add($"缺失灯光控制器：{sourcePmx}");
        }

        var fxsubPath = Path.Combine(targetRoot, target.FxSubFileName);
        if (!File.Exists(fxsubPath))
        {
            warnings.Add($"缺失灯光 fxsub：{fxsubPath}");
            return;
        }

        File.WriteAllText(fxsubPath, Patch(File.ReadAllText(fxsubPath), document.LightingPatchValues));
        patched.Add(fxsubPath);
    }

    public static string Preview(RayDocument document)
    {
        var source = Path.Combine(document.RayRootPath, "Lighting", "DirectionalLight", "directional_lighting.fxsub");
        if (!File.Exists(source))
        {
            return $"Missing DirectionalLight source: {source}";
        }

        return IsSwitchOn(document, "ENABLE_DIRECTIONAL_LIGHTING_PATCH")
            ? Patch(File.ReadAllText(source), document.LightingPatchValues)
            : File.ReadAllText(source);
    }

    private static string Patch(string text, IReadOnlyDictionary<string, string> values)
    {
        values = ApplyPreset(values);
        var cleaned = RemoveExistingPatch(text);
        var match = Regex.Match(cleaned, @"\tdiffuse \*= (?<multiplier>[^;]+);");
        if (!match.Success)
        {
            return cleaned;
        }

        var multiplier = match.Groups["multiplier"].Value.Trim();
        var replacement =
            $"\tfloat3 rayNodeOriginalDiffuse = diffuse * ({multiplier});\r\n" +
            BeginMarker + "\r\n" +
            $"\tfloat rayNodeLightingBlend = saturate({ScalarSource(values, "LIGHTING_PATCH_BLEND", "0.35")});\r\n" +
            $"\tfloat3 rayNodeLightingTint = {ColorSource(values, "LIGHTING_PATCH_TINT", "{1.0, 1.0, 1.0}")};\r\n" +
            $"\tfloat rayNodeShadowAmount = saturate({ScalarSource(values, "LIGHTING_PATCH_SHADOW_AMOUNT", "1.0", "LIGHTING_PATCH_SHADOW_SOURCE")});\r\n" +
            $"\tfloat rayNodeSpecularKeep = saturate({ScalarSource(values, "LIGHTING_PATCH_SPECULAR_KEEP", "1.0", "LIGHTING_PATCH_SPECULAR_SOURCE")});\r\n" +
            "\tfloat rayNodeLambert = saturate(dot(material.normal, normalize(L)));\r\n" +
            "\tfloat rayNodeHalfLambert = saturate(rayNodeLambert * 0.5 + 0.5);\r\n" +
            $"\tfloat rayNodeLightingFactor = {ModeExpression(Read(values, "LIGHTING_PATCH_MODE", "HalfLambert"))};\r\n" +
            "\trayNodeLightingFactor *= lerp(1.0, shadow, rayNodeShadowAmount);\r\n" +
            $"\tfloat3 rayNodeLambertDiffuse = material.albedo * ({multiplier}) * rayNodeLightingTint * rayNodeLightingFactor;\r\n" +
            "\tdiffuse = lerp(rayNodeOriginalDiffuse, rayNodeLambertDiffuse, rayNodeLightingBlend);\r\n" +
            EndMarker;

        cleaned = cleaned.Remove(match.Index, match.Length).Insert(match.Index, replacement);
        return Regex.Replace(
            cleaned,
            @"\tspecular \*= (?<multiplier>[^;]+);",
            match => $"{match.Value}\r\n#if 1\r\n\tspecular *= lerp(1.0 - rayNodeLightingBlend, 1.0, rayNodeSpecularKeep);\r\n#endif",
            RegexOptions.None,
            TimeSpan.FromSeconds(1));
    }

    private static string RemoveExistingPatch(string text)
    {
        var result = Regex.Replace(
            text,
            $"{Regex.Escape(BeginMarker)}[\\s\\S]*?{Regex.Escape(EndMarker)}",
            "diffuse = rayNodeOriginalDiffuse;");
        result = Regex.Replace(
            result,
            @"\r?\n#if 1\r?\n\tspecular \*= lerp\(1\.0 - rayNodeLightingBlend, 1\.0, rayNodeSpecularKeep\);\r?\n#endif",
            string.Empty);
        return result;
    }

    private static string ModeExpression(string mode)
    {
        return mode.Trim().ToLowerInvariant() switch
        {
            "lambert" => "rayNodeLambert",
            "inverted" or "invert" => "(1.0 - rayNodeLambert)",
            _ => "rayNodeHalfLambert",
        };
    }

    private static string Read(IReadOnlyDictionary<string, string> values, string key, string fallback)
    {
        return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static IReadOnlyDictionary<string, string> ApplyPreset(IReadOnlyDictionary<string, string> values)
    {
        var merged = new Dictionary<string, string>(values, StringComparer.Ordinal);
        var preset = Read(values, "LIGHTING_PATCH_PRESET", "Custom").Trim().ToLowerInvariant();
        var presetValues = preset switch
        {
            "softtoon" => new Dictionary<string, string>
            {
                ["LIGHTING_PATCH_MODE"] = "HalfLambert",
                ["LIGHTING_PATCH_BLEND"] = "0.45",
                ["LIGHTING_PATCH_TINT"] = "{1.0, 0.96, 0.9}",
                ["LIGHTING_PATCH_SHADOW_AMOUNT"] = "0.8",
                ["LIGHTING_PATCH_SPECULAR_KEEP"] = "0.9",
            },
            "stronglambert" => new Dictionary<string, string>
            {
                ["LIGHTING_PATCH_MODE"] = "Lambert",
                ["LIGHTING_PATCH_BLEND"] = "0.65",
                ["LIGHTING_PATCH_TINT"] = "{1.0, 1.0, 1.0}",
                ["LIGHTING_PATCH_SHADOW_AMOUNT"] = "1.0",
                ["LIGHTING_PATCH_SPECULAR_KEEP"] = "0.75",
            },
            "keephighlight" => new Dictionary<string, string>
            {
                ["LIGHTING_PATCH_MODE"] = "HalfLambert",
                ["LIGHTING_PATCH_BLEND"] = "0.3",
                ["LIGHTING_PATCH_TINT"] = "{1.0, 1.0, 1.0}",
                ["LIGHTING_PATCH_SHADOW_AMOUNT"] = "1.0",
                ["LIGHTING_PATCH_SPECULAR_KEEP"] = "1.0",
            },
            "filllight" => new Dictionary<string, string>
            {
                ["LIGHTING_PATCH_MODE"] = "Inverted",
                ["LIGHTING_PATCH_BLEND"] = "0.25",
                ["LIGHTING_PATCH_TINT"] = "{0.75, 0.85, 1.0}",
                ["LIGHTING_PATCH_SHADOW_AMOUNT"] = "0.0",
                ["LIGHTING_PATCH_SPECULAR_KEEP"] = "1.0",
            },
            _ => [],
        };

        foreach (var pair in presetValues)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    private static string ScalarSource(IReadOnlyDictionary<string, string> values, string valueKey, string fallback, string? sourceKey = null)
    {
        var source = Read(values, sourceKey ?? valueKey + "_SOURCE", "Constant").Trim().ToLowerInvariant();
        return source switch
        {
            "customa" => "material.customDataA",
            "occlusion" => "material.visibility",
            "smoothness" => "material.smoothness",
            "metalness" => "material.metalness",
            "emissiveintensity" => "material.emissiveIntensity",
            _ => Read(values, valueKey, fallback),
        };
    }

    private static string ColorSource(IReadOnlyDictionary<string, string> values, string valueKey, string fallback)
    {
        var source = Read(values, valueKey + "_SOURCE", "Constant").Trim().ToLowerInvariant();
        return source switch
        {
            "customb" => "material.customDataB",
            "albedo" => "material.albedo",
            "specular" => "material.specular",
            "emissive" => "material.emissive",
            _ => ReadVector(values, valueKey, fallback),
        };
    }

    private static bool IsSwitchOn(RayDocument document, string key)
    {
        return document.LightingPatchValues.TryGetValue(key, out var value) &&
               (value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<LightPatchTarget> GetTargets(RayDocument document)
    {
        if (IsSwitchOn(document, "ENABLE_DIRECTIONAL_LIGHTING_PATCH"))
        {
            foreach (var target in DirectionalTargets)
            {
                yield return target;
            }
        }

        if (IsSwitchOn(document, "ENABLE_MULTI_LIGHTING_PATCH"))
        {
            foreach (var target in MultiLightTargets)
            {
                yield return target;
            }
        }
    }

    private static string ReadVector(IReadOnlyDictionary<string, string> values, string key, string fallback)
    {
        var text = Read(values, key, fallback).Trim();
        if (text.StartsWith("float3", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        var numbers = Regex.Matches(text, @"-?\d+(?:\.\d+)?")
            .Select(match => match.Value)
            .Take(3)
            .ToList();
        while (numbers.Count < 3)
        {
            numbers.Add("1.0");
        }

        return $"float3({string.Join(", ", numbers.Select(NormalizeNumber))})";
    }

    private static string NormalizeNumber(string value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed.ToString("0.######", CultureInfo.InvariantCulture)
            : value;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(directory.Replace(source, target));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, file.Replace(source, target), overwrite: true);
        }
    }

    private sealed record LightPatchTarget(string FolderName, string FxSubFileName, string ControllerFileName);
}

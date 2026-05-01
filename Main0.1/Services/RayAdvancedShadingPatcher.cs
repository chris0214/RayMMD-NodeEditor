using System.Text.RegularExpressions;

namespace RayMmdNodeEditor.Services;

public sealed record RayShadingPatchResult(bool Enabled, IReadOnlyList<string> PatchedFiles, IReadOnlyList<string> Warnings);

public static class RayAdvancedShadingPatcher
{
    private const string BeginMarker = "// RAY_MMD_NODE_EDITOR_SHADING_BEGIN";
    private const string EndMarker = "// RAY_MMD_NODE_EDITOR_SHADING_END";

    public static RayShadingPatchResult Export(RayDocument document, string exportRoot, RayAdvancedMaterialCompileResult? advanced)
    {
        if (advanced is null || string.IsNullOrWhiteSpace(advanced.ShadingPatchBlock))
        {
            return new RayShadingPatchResult(false, [], []);
        }

        var warnings = new List<string>();
        var patched = new List<string>();
        var sourceShaderRoot = Path.Combine(document.RayRootPath, "Shader");
        var targetShaderRoot = Path.Combine(exportRoot, "Shader");
        if (!Directory.Exists(sourceShaderRoot))
        {
            warnings.Add($"Missing Ray Shader directory: {sourceShaderRoot}");
            return new RayShadingPatchResult(true, patched, warnings);
        }

        CopyDirectory(sourceShaderRoot, targetShaderRoot);
        var sourceRayFx = Path.Combine(document.RayRootPath, "ray.fx");
        var targetRayFx = Path.Combine(exportRoot, "ray.fx");
        if (File.Exists(sourceRayFx))
        {
            Directory.CreateDirectory(exportRoot);
            File.Copy(sourceRayFx, targetRayFx, overwrite: true);
            patched.Add(targetRayFx);
        }
        else
        {
            warnings.Add($"Missing ray.fx: {sourceRayFx}");
        }

        var path = Path.Combine(targetShaderRoot, "ShadingMaterials.fxsub");
        if (!File.Exists(path))
        {
            warnings.Add($"Missing ShadingMaterials.fxsub: {path}");
            return new RayShadingPatchResult(true, patched, warnings);
        }

        File.WriteAllText(path, Patch(File.ReadAllText(path), advanced.ShadingPatchBlock));
        patched.Add(path);
        return new RayShadingPatchResult(true, patched, warnings);
    }

    public static string Preview(RayDocument document, RayAdvancedMaterialCompileResult? advanced)
    {
        var source = Path.Combine(document.RayRootPath, "Shader", "ShadingMaterials.fxsub");
        var block = advanced?.ShadingPatchBlock ?? string.Empty;
        if (!File.Exists(source))
        {
            return string.IsNullOrWhiteSpace(block)
                ? $"// Missing ShadingMaterials source: {source}"
                : $"// Missing ShadingMaterials source: {source}{Environment.NewLine}{Environment.NewLine}{block}";
        }

        return Patch(File.ReadAllText(source), block);
    }

    public static string Patch(string text, string patchBlock)
    {
        var result = RemoveExistingPatch(text);
        if (string.IsNullOrWhiteSpace(patchBlock))
        {
            return result;
        }

        result = InsertBefore(result, "#if SUN_LIGHT_ENABLE", patchBlock + Environment.NewLine);
        result = result.Replace(
            "\toColor0 = float4(diffuse * material.albedo + specular, material.linearDepth);",
            "\tfloat3 rayNodeOpaqueLighting = diffuse * material.albedo + specular;\r\n" +
            "\trayNodeOpaqueLighting = RayNode_ApplyShading(coord, material, rayNodeOpaqueLighting, diffuse, specular, V, L, screenPosition);\r\n" +
            "\toColor0 = float4(rayNodeOpaqueLighting, material.linearDepth);");
        result = result.Replace(
            "\treturn float4(lighting, material.linearDepth);",
            "\tlighting = RayNode_ApplyShading(coord, material, lighting, lighting, 0.0, V, L, screenPosition);\r\n" +
            "\treturn float4(lighting, material.linearDepth);");
        return result;
    }

    private static string RemoveExistingPatch(string text)
    {
        var result = Regex.Replace(
            text,
            $"{Regex.Escape(BeginMarker)}[\\s\\S]*?{Regex.Escape(EndMarker)}\\s*",
            string.Empty);
        result = Regex.Replace(
            result,
            @"\r?\n\tfloat3 rayNodeOpaqueLighting = diffuse \* material\.albedo \+ specular;\r?\n\trayNodeOpaqueLighting = RayNode_ApplyShading\(coord, material, rayNodeOpaqueLighting, diffuse, specular, V, L, screenPosition\);\r?\n\toColor0 = float4\(rayNodeOpaqueLighting, material\.linearDepth\);",
            "\r\n\toColor0 = float4(diffuse * material.albedo + specular, material.linearDepth);");
        result = Regex.Replace(
            result,
            @"\r?\n\tlighting = RayNode_ApplyShading\(coord, material, lighting, lighting, 0\.0, V, L, screenPosition\);\r?\n\treturn float4\(lighting, material\.linearDepth\);",
            "\r\n\treturn float4(lighting, material.linearDepth);");
        return result;
    }

    private static string InsertBefore(string text, string marker, string insert)
    {
        var index = text.IndexOf(marker, StringComparison.Ordinal);
        return index < 0 ? insert + text : text.Insert(index, insert);
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var directory in Directory.EnumerateDirectories(source))
        {
            CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
        }
    }
}

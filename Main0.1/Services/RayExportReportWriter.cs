using System.Text;

namespace RayMmdNodeEditor.Services;

public static class RayExportReportWriter
{
    public static void Write(
        string exportRoot,
        RayDocument document,
        RayMaterialCompileResult compileResult,
        RayTextureExportResult textureResult,
        IReadOnlyList<string> compatibilityIssues,
        RayAdvancedMaterialCompileResult? advancedResult = null,
        RayLightingPatchResult? lightingResult = null,
        RayShadingPatchResult? shadingResult = null,
        RayFeatureAnalysis? featureAnalysis = null,
        RayPackageExportResult? packageResult = null)
    {
        File.WriteAllText(
            Path.Combine(exportRoot, "RayMmdNodeEditor_Report.txt"),
            Build(document, compileResult, textureResult, compatibilityIssues, advancedResult, lightingResult, shadingResult, featureAnalysis, packageResult));
    }

    public static string Build(
        RayDocument document,
        RayMaterialCompileResult compileResult,
        RayTextureExportResult textureResult,
        IReadOnlyList<string> compatibilityIssues,
        RayAdvancedMaterialCompileResult? advancedResult = null,
        RayLightingPatchResult? lightingResult = null,
        RayShadingPatchResult? shadingResult = null,
        RayFeatureAnalysis? featureAnalysis = null,
        RayPackageExportResult? packageResult = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RayMmdNodeEditor Export Report");
        builder.AppendLine($"Export Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Ray Root: {document.RayRootPath}");
        builder.AppendLine($"Export Directory: {document.ExportDirectory}");
        builder.AppendLine($"Material File: {RayExportNaming.GetMaterialFileName(document)}");
        builder.AppendLine($"Quality Preset: {document.QualityPreset}");
        builder.AppendLine($"Material Mode: {document.MaterialMode}");
        builder.AppendLine($"Copy Textures: {document.CopyTextureFiles}");
        builder.AppendLine($"Export Full Ray Package: {document.ExportFullRayPackage}");
        builder.AppendLine();

        builder.AppendLine("Ray Package Copy");
        if (packageResult is null || !packageResult.Enabled)
        {
            builder.AppendLine("- Not enabled.");
        }
        else
        {
            builder.AppendLine($"- Copied Files: {packageResult.CopiedFiles}");
            foreach (var root in packageResult.CopiedRoots)
            {
                builder.AppendLine($"- Root: {root}");
            }

            foreach (var warning in packageResult.Warnings)
            {
                builder.AppendLine($"- Warning: {warning}");
            }
        }
        builder.AppendLine();

        AppendDictionary(builder, "ray.conf", document.RayConfValues);
        AppendDictionary(builder, "ray_advanced.conf", document.AdvancedConfValues);
        AppendDictionary(builder, "material_common_2.0.fxsub patch", document.MaterialCommonValues);
        AppendDictionary(builder, "Lighting / DirectionalLight patch", document.LightingPatchValues);

        featureAnalysis ??= RayFeatureAnalyzer.Analyze(document, applyAutoEnable: false);
        builder.AppendLine("Ray Feature Analysis");
        builder.AppendLine($"- Estimated Texture Samples: {featureAnalysis.EstimatedTextureSamples}");
        builder.AppendLine($"- Estimated Math Ops: {featureAnalysis.EstimatedMathOps}");
        builder.AppendLine($"- Required Defines: {(featureAnalysis.RequiredDefines.Count == 0 ? "None" : string.Join(", ", featureAnalysis.RequiredDefines.Select(pair => pair.Key + "=" + pair.Value)))}");
        builder.AppendLine($"- Auto Enabled: {(featureAnalysis.AutoEnabled.Count == 0 ? "None" : string.Join(", ", featureAnalysis.AutoEnabled))}");
        foreach (var note in featureAnalysis.Notes)
        {
            builder.AppendLine($"- Note: {note}");
        }
        builder.AppendLine();

        builder.AppendLine("Compatibility");
        if (compatibilityIssues.Count == 0)
        {
            builder.AppendLine("- No issues detected.");
        }
        else
        {
            foreach (var issue in compatibilityIssues)
            {
                builder.AppendLine($"- {issue}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("Advanced Node Mode");
        if (advancedResult is null)
        {
            builder.AppendLine("- Not enabled.");
        }
        else
        {
            builder.AppendLine($"- Success: {advancedResult.Success}");
            builder.AppendLine($"- Patched Slots: {(advancedResult.Slots.Count == 0 ? "None" : string.Join(", ", advancedResult.Slots))}");
            builder.AppendLine($"- Shading Slots: {(advancedResult.ShadingSlots is null || advancedResult.ShadingSlots.Count == 0 ? "None" : string.Join(", ", advancedResult.ShadingSlots))}");
            builder.AppendLine($"- Advanced Textures: {(advancedResult.TextureFiles.Count == 0 ? "None" : string.Join(", ", advancedResult.TextureFiles))}");
            foreach (var message in advancedResult.Messages)
            {
                builder.AppendLine($"- Message: {message}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("DirectionalLight Lighting Patch");
        if (lightingResult is null || !lightingResult.Enabled)
        {
            builder.AppendLine("- Not enabled.");
        }
        else
        {
            builder.AppendLine("- Enabled: True");
            foreach (var path in lightingResult.PatchedFiles)
            {
                builder.AppendLine($"- Patched: {path}");
            }

            foreach (var warning in lightingResult.Warnings)
            {
                builder.AppendLine($"- Warning: {warning}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("Ray Final Shading Patch");
        if (shadingResult is null || !shadingResult.Enabled)
        {
            builder.AppendLine("- Not enabled.");
        }
        else
        {
            builder.AppendLine("- Enabled: True");
            foreach (var path in shadingResult.PatchedFiles)
            {
                builder.AppendLine($"- Patched: {path}");
            }

            foreach (var warning in shadingResult.Warnings)
            {
                builder.AppendLine($"- Warning: {warning}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("Compiler Messages");
        if (compileResult.Messages.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var message in compileResult.Messages)
            {
                builder.AppendLine($"- {message}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("Texture Copy");
        if (textureResult.CopiedFiles.Count == 0)
        {
            builder.AppendLine("- No texture files copied.");
        }
        else
        {
            foreach (var path in textureResult.CopiedFiles)
            {
                builder.AppendLine($"- {path}");
            }
        }
        foreach (var warning in textureResult.Warnings)
        {
            builder.AppendLine($"- Warning: {warning}");
        }

        return builder.ToString();
    }

    private static void AppendDictionary(StringBuilder builder, string title, IReadOnlyDictionary<string, string> values)
    {
        builder.AppendLine(title);
        foreach (var pair in values)
        {
            builder.AppendLine($"- {pair.Key} = {pair.Value}");
        }
        builder.AppendLine();
    }
}

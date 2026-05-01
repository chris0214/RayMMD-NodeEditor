using System.Text.RegularExpressions;

namespace RayMmdNodeEditor.Services;

public static class RayAdvancedCommonPatcher
{
    private const string BeginMarker = "// RAY_MMD_NODE_EDITOR_ADVANCED_BEGIN";
    private const string EndMarker = "// RAY_MMD_NODE_EDITOR_ADVANCED_END";

    public static string Patch(string commonText, RayAdvancedMaterialCompileResult advanced)
    {
        var result = RemoveExistingPatch(commonText);
        if (!advanced.Success || string.IsNullOrWhiteSpace(advanced.CommonPatchBlock))
        {
            return result;
        }

        result = InsertBefore(result, "GbufferParam EncodeGbuffer", advanced.CommonPatchBlock + Environment.NewLine);
        result = PatchAlpha(result);
        result = PatchMaterialOverride(result);
        return result;
    }

    private static string RemoveExistingPatch(string text)
    {
        return Regex.Replace(
            text,
            $"{Regex.Escape(BeginMarker)}[\\s\\S]*?{Regex.Escape(EndMarker)}\\s*",
            string.Empty);
    }

    private static string InsertBefore(string text, string marker, string insert)
    {
        var index = text.IndexOf(marker, StringComparison.Ordinal);
        if (index >= 0)
        {
            return text.Insert(index, insert);
        }

        var materialPixelShaderIndex = text.IndexOf("GbufferParam MaterialPS", StringComparison.Ordinal);
        return materialPixelShaderIndex >= 0 ? text.Insert(materialPixelShaderIndex, insert) : text;
    }

    private static string PatchAlpha(string text)
    {
        const string original = "\tfloat alpha = GetAlpha(coord0);";
        const string replacement = "\tfloat alpha = GetAlpha(coord0);\r\n#if RAY_NODE_ADVANCED_ALPHA\r\n\talpha = RayNode_GetAlpha(coord0);\r\n#endif";
        return text.Replace(original, replacement);
    }

    private static string PatchMaterialOverride(string text)
    {
        const string original = "\tmaterial.lightModel = GetLightMode(material);";
        const string replacement =
            "#if RAY_NODE_ADVANCED_NORMAL\r\n\tmaterial.normal = RayNode_GetNormal(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_ALBEDO\r\n\tmaterial.albedo = RayNode_GetAlbedo(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_SMOOTHNESS\r\n\tmaterial.smoothness = RayNode_GetSmoothness(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_METALNESS\r\n\tmaterial.metalness = RayNode_GetMetalness(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_SPECULAR\r\n\tmaterial.specular = RayNode_GetSpecular(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_OCCLUSION\r\n\tmaterial.visibility = RayNode_GetOcclusion(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_EMISSIVE\r\n\tmaterial.emissive = RayNode_GetEmissive(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_CUSTOMA\r\n\tmaterial.customDataA = RayNode_GetCustomA(coord0, material);\r\n#endif\r\n" +
            "#if RAY_NODE_ADVANCED_CUSTOMB\r\n\tmaterial.customDataB = RayNode_GetCustomB(coord0, material);\r\n#endif\r\n" +
            "\tmaterial.lightModel = GetLightMode(material);";
        return text.Replace(original, replacement);
    }
}

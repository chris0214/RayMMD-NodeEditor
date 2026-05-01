using System.Globalization;
using System.Text.RegularExpressions;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public static class RayCompatibilityChecker
{
    public static IReadOnlyList<string> Check(RayDocument document, string materialText, RayAdvancedMaterialCompileResult? advancedResult = null)
    {
        var issues = new List<string>();
        var features = RayFeatureAnalyzer.Analyze(document, applyAutoEnable: false);
        foreach (var pair in features.RequiredDefines)
        {
            if (!document.RayConfValues.TryGetValue(pair.Key, out var current) ||
                ReadDefineLevel(current) < ReadDefineLevel(pair.Value))
            {
                issues.Add($"{pair.Key} should be at least {pair.Value} for the current Ray node graph. Export will auto-enable it.");
            }
        }
        foreach (var note in features.Notes)
        {
            issues.Add(note);
        }

        if (string.Equals(document.MaterialMode, RayMaterialModes.Advanced, StringComparison.OrdinalIgnoreCase))
        {
            if (advancedResult is null)
            {
                issues.Add("高级节点模式已启用，但高级编译结果为空。");
            }
            else
            {
                issues.AddRange(advancedResult.Messages);
                if (advancedResult.Success &&
                    advancedResult.Slots.Count == 0 &&
                    (advancedResult.ShadingSlots is null || advancedResult.ShadingSlots.Count == 0))
                {
                    issues.Add("高级节点模式已启用，但没有任何高级槽被连接；导出会保持兼容模式效果。");
                }
            }
        }
        else
        {
            foreach (var node in document.Graph.Nodes.Where(node => RayNodeSupport.RequiresAdvancedMode(node.Kind)))
            {
                issues.Add($"{NodeRegistry.Get(node.Kind).Title} 需要高级节点模式；兼容模式不会导出它的逐像素效果。");
            }
        }

        var customEnable = ReadDefineInt(materialText, "CUSTOM_ENABLE");
        var specularType = ReadDefineInt(materialText, "SPECULAR_MAP_TYPE");
        if (customEnable > 0 && specularType <= 1)
        {
            issues.Add("Custom 材质模式已启用，但 SPECULAR_MAP_TYPE 为 0/1。Ray 原版会报错；请把高光贴图类型改成灰度 UE4/Frostbite 或固定 0.4。");
        }

        var alphaThreshold = ReadFloat(document.MaterialCommonValues, "ALPHA_THRESHOLD", 0.999f);
        if (alphaThreshold < 0 || alphaThreshold > 1)
        {
            issues.Add("ALPHA_THRESHOLD 应在 0 到 1 之间。");
        }

        var anisotropy = ReadFloat(document.MaterialCommonValues, "TEXTURE_ANISOTROPY_LEVEL", 16);
        if (anisotropy is < 1 or > 16)
        {
            issues.Add("TEXTURE_ANISOTROPY_LEVEL 建议保持 1、2、4、8、16；过高可能不被显卡/FX 编译器接受。");
        }

        foreach (var node in document.Graph.Nodes.Where(node => node.Kind == NodeKind.RayTextureSlot))
        {
            var source = Read(node, "Source", "File");
            if (!IsFileSource(source))
            {
                continue;
            }

            var path = Read(node, "File", string.Empty);
            if (string.IsNullOrWhiteSpace(path))
            {
                issues.Add("有一个 Ray 贴图槽使用文件贴图，但 File 为空。");
                continue;
            }

            var absolute = ResolveTexturePath(document, path);
            if (absolute is null || !File.Exists(absolute))
            {
                issues.Add($"找不到贴图文件：{path}");
            }
        }

        if (ReadDefineInt(materialText, "PARALLAX_MAP_FROM") > 0 &&
            ReadDefineInt(materialText, "NORMAL_MAP_FROM") == 0)
        {
            issues.Add("启用了视差贴图但没有法线贴图。Ray 可以运行，但视差效果通常需要法线配合才自然。");
        }

        var output = document.Graph.Nodes.FirstOrDefault(node => node.Kind == NodeKind.RayMaterialOutput);
        var customMode = output is null ? "None" : Read(output, "CustomMode", "None");
        var customModeDisabled = customMode.Trim().Equals("None", StringComparison.OrdinalIgnoreCase) ||
                                 customMode.Trim().Equals("0", StringComparison.OrdinalIgnoreCase);
        if (customModeDisabled &&
            (document.Graph.FindInputConnection(output?.Id ?? Guid.Empty, "CustomA") is not null ||
             document.Graph.FindInputConnection(output?.Id ?? Guid.Empty, "CustomB") is not null ||
             advancedResult?.Slots.Contains("CustomA") == true ||
             advancedResult?.Slots.Contains("CustomB") == true))
        {
            issues.Add("CustomA/CustomB 已连接，但 Ray 材质输出的 Custom 材质模式仍为禁用。请在输出节点里设置 CustomMode，否则这些数据大多不会被 Ray 使用。");
        }

        return issues;
    }

    private static int ReadDefineInt(string text, string name)
    {
        var match = Regex.Match(text, $@"(?m)^\s*#define\s+{Regex.Escape(name)}\s+(-?\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static int ReadDefineLevel(string text)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : text.Equals("true", StringComparison.OrdinalIgnoreCase) || text.Equals("on", StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
    }

    private static float ReadFloat(IReadOnlyDictionary<string, string> values, string key, float fallback)
    {
        return values.TryGetValue(key, out var text) &&
               float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    internal static bool IsFileSource(string source)
    {
        return source.Trim().Equals("File", StringComparison.OrdinalIgnoreCase) ||
               source.Trim().Equals("1", StringComparison.OrdinalIgnoreCase);
    }

    internal static string? ResolveTexturePath(RayDocument document, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalized = path.Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalized))
        {
            return normalized;
        }

        var rayRelative = Path.Combine(document.RayRootPath, "Materials", normalized);
        if (File.Exists(rayRelative))
        {
            return rayRelative;
        }

        return Path.Combine(document.RayRootPath, normalized);
    }

    private static string Read(GraphNode node, string propertyName, string fallback)
    {
        return node.Properties.TryGetValue(propertyName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }
}

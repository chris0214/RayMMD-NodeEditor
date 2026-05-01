using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public sealed record RayFeatureAnalysis(
    IReadOnlyDictionary<string, string> RequiredDefines,
    IReadOnlyList<string> AutoEnabled,
    int EstimatedTextureSamples,
    int EstimatedMathOps,
    IReadOnlyList<string> Notes);

public static class RayFeatureAnalyzer
{
    public static RayFeatureAnalysis Analyze(RayDocument document, bool applyAutoEnable)
    {
        var required = new Dictionary<string, string>(StringComparer.Ordinal);
        var autoEnabled = new List<string>();
        var notes = new List<string>();
        var textureSamples = 0;
        var mathOps = 0;

        foreach (var node in document.Graph.Nodes)
        {
            switch (node.Kind)
            {
                case NodeKind.RaySsrReflection:
                    Require(required, "SSR_QUALITY", "1");
                    textureSamples += 4;
                    notes.Add("Ray SSR node samples screen-space reflection data in the final shading path.");
                    break;
                case NodeKind.RayOutlineChannel:
                    Require(required, "OUTLINE_QUALITY", "1");
                    textureSamples += 1;
                    break;
                case NodeKind.RayFogChannel:
                    Require(required, "FOG_ENABLE", "1");
                    textureSamples += 1;
                    break;
                case NodeKind.RaySsao:
                    Require(required, "SSDO_QUALITY", "2");
                    textureSamples += 1;
                    break;
                case NodeKind.RayIblReflection:
                    Require(required, "IBL_QUALITY", "1");
                    textureSamples += 5;
                    mathOps += 12;
                    break;
                case NodeKind.RayIblSplit:
                    Require(required, "IBL_QUALITY", "1");
                    textureSamples += 2;
                    mathOps += 8;
                    break;
                case NodeKind.RayChannelSplit:
                    Require(required, "SSDO_QUALITY", "2");
                    Require(required, "IBL_QUALITY", "1");
                    textureSamples += 8;
                    mathOps += 4;
                    notes.Add("Ray Channel Split exposes several internal Ray buffers. Only connect the channels you need to keep shader cost down.");
                    break;
                case NodeKind.RayMaterialDiagnostic:
                    AddDiagnosticRequirements(node, required, ref textureSamples);
                    mathOps += 4;
                    break;
                case NodeKind.RayFogDepthBlend:
                    Require(required, "FOG_ENABLE", "1");
                    textureSamples += 2;
                    mathOps += 8;
                    break;
                case NodeKind.RayShadowData:
                    Require(required, "SUN_LIGHT_ENABLE", "1");
                    Require(required, "SUN_SHADOW_QUALITY", "3");
                    textureSamples += 1;
                    break;
                case NodeKind.RayMultiLight:
                    Require(required, "MULTI_LIGHT_ENABLE", "1");
                    textureSamples += 2;
                    break;
                case NodeKind.RaySkinAdvanced or NodeKind.RaySkinSssBridge:
                    Require(required, "SSSS_QUALITY", "1");
                    mathOps += 8;
                    break;
                case NodeKind.RayDebugController:
                    AddDebugRequirements(document, node, required, ref textureSamples);
                    break;
                case NodeKind.RayTextureSlot or NodeKind.NormalMap or NodeKind.TriplanarBoxmap or NodeKind.GenericRampSample:
                    textureSamples += node.Kind == NodeKind.TriplanarBoxmap ? 3 : 1;
                    break;
                case NodeKind.NoiseTexture or NodeKind.FbmNoise or NodeKind.VoronoiTexture or NodeKind.CellEdgeTexture:
                    mathOps += node.Kind == NodeKind.FbmNoise ? 40 : 16;
                    break;
                case NodeKind.Wetness or NodeKind.RaySnowLayer or NodeKind.RayDustLayer or NodeKind.RayEdgeWear or NodeKind.RayMaterialLayer:
                    mathOps += 10;
                    break;
            }
        }

        if (applyAutoEnable)
        {
            foreach (var pair in required)
            {
                if (ShouldReplace(document.RayConfValues, pair.Key, pair.Value))
                {
                    document.RayConfValues[pair.Key] = pair.Value;
                    autoEnabled.Add($"{pair.Key}={pair.Value}");
                }
            }
        }

        if (textureSamples > 16)
        {
            notes.Add($"High texture sample estimate: {textureSamples}. Consider baking masks or simplifying procedural chains.");
        }

        if (mathOps > 80)
        {
            notes.Add($"High math estimate: {mathOps}. This graph may be expensive on older MME/DX9 setups.");
        }

        return new RayFeatureAnalysis(required, autoEnabled, textureSamples, mathOps, notes.Distinct().ToList());
    }

    private static void AddDebugRequirements(RayDocument document, GraphNode node, Dictionary<string, string> required, ref int textureSamples)
    {
        var channel = Read(node, "Channel", "SSAO").Trim().ToLowerInvariant();
        if (channel.Contains("ssr", StringComparison.Ordinal))
        {
            Require(required, "SSR_QUALITY", "1");
            textureSamples += 1;
        }
        else if (channel.Contains("outline", StringComparison.Ordinal))
        {
            Require(required, "OUTLINE_QUALITY", "1");
            textureSamples += 1;
        }
        else if (channel.Contains("pssm", StringComparison.Ordinal) || channel.Contains("shadow", StringComparison.Ordinal))
        {
            Require(required, "SUN_LIGHT_ENABLE", "1");
            Require(required, "SUN_SHADOW_QUALITY", "3");
            textureSamples += 4;
        }
        else if (channel.Contains("ssao", StringComparison.Ordinal) || channel.Contains("ssdo", StringComparison.Ordinal))
        {
            Require(required, "SSDO_QUALITY", "2");
            textureSamples += 1;
        }
    }

    private static void AddDiagnosticRequirements(GraphNode node, Dictionary<string, string> required, ref int textureSamples)
    {
        var channel = Read(node, "Channel", "Smoothness").Trim().ToLowerInvariant();
        if (channel.Contains("ssr", StringComparison.Ordinal))
        {
            Require(required, "SSR_QUALITY", "1");
            textureSamples += 1;
        }
        else if (channel.Contains("outline", StringComparison.Ordinal))
        {
            Require(required, "OUTLINE_QUALITY", "1");
            textureSamples += 1;
        }
        else if (channel.Contains("fog", StringComparison.Ordinal))
        {
            Require(required, "FOG_ENABLE", "1");
            textureSamples += 1;
        }
        else if (channel.Contains("ibl", StringComparison.Ordinal))
        {
            Require(required, "IBL_QUALITY", "1");
            textureSamples += 2;
        }
        else if (channel.Contains("shadow", StringComparison.Ordinal))
        {
            Require(required, "SUN_LIGHT_ENABLE", "1");
            Require(required, "SUN_SHADOW_QUALITY", "3");
            textureSamples += 1;
        }
        else if (channel.Contains("ssao", StringComparison.Ordinal) || channel.Contains("ssdo", StringComparison.Ordinal))
        {
            Require(required, "SSDO_QUALITY", "2");
            textureSamples += 1;
        }
        else if (channel.Contains("scene", StringComparison.Ordinal) || channel.Contains("depth", StringComparison.Ordinal))
        {
            textureSamples += 1;
        }
    }

    private static void Require(Dictionary<string, string> required, string key, string value)
    {
        if (!required.TryGetValue(key, out var existing) || ParseInt(value) > ParseInt(existing))
        {
            required[key] = value;
        }
    }

    private static bool ShouldReplace(IReadOnlyDictionary<string, string> values, string key, string required)
    {
        return !values.TryGetValue(key, out var existing) || ParseInt(existing) < ParseInt(required);
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static string Read(GraphNode node, string propertyName, string fallback)
    {
        return node.Properties.TryGetValue(propertyName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }
}

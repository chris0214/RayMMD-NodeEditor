using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor.Graph;

public static class OutputNodeProperties
{
    public const string Pipeline = "Pipeline";
    public const string TechniqueStyle = "TechniqueStyle";
    public const string ShadowTechniqueMode = "ShadowTechniqueMode";
    public const string ZplotTechniqueMode = "ZplotTechniqueMode";
    public const string EmitAuxiliaryDepthTechniques = "EmitAuxiliaryDepthTechniques";
    public const string RenderTarget0Name = "RenderTarget0Name";
    public const string RenderTarget1Name = "RenderTarget1Name";
    public const string RenderTarget0Scale = "RenderTarget0Scale";
    public const string RenderTarget1Scale = "RenderTarget1Scale";

    public static string GetRenderTargetNameProperty(int index) => index switch
    {
        0 => RenderTarget0Name,
        1 => RenderTarget1Name,
        _ => string.Empty,
    };

    public static string GetRenderTargetScaleProperty(int index) => index switch
    {
        0 => RenderTarget0Scale,
        1 => RenderTarget1Scale,
        _ => string.Empty,
    };

    public static bool TryGetRenderTargetName(GraphNode node, int index, out string resourceName)
    {
        resourceName = string.Empty;
        var propertyName = GetRenderTargetNameProperty(index);
        if (string.IsNullOrWhiteSpace(propertyName) ||
            !node.Properties.TryGetValue(propertyName, out var rawValue))
        {
            return false;
        }

        resourceName = rawValue?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(resourceName);
    }

    public static float ReadRenderTargetScale(GraphNode node, int index)
    {
        var propertyName = GetRenderTargetScaleProperty(index);
        if (string.IsNullOrWhiteSpace(propertyName) ||
            !node.Properties.TryGetValue(propertyName, out var rawValue) ||
            !FloatParser.TryParse(rawValue, out var value))
        {
            return 1.0f;
        }

        return Math.Clamp(value, 0.03125f, 1.0f);
    }

    public static bool UsesLegacyZBufferTechnique(GraphNode? outputNode)
    {
        return outputNode is not null &&
               outputNode.Properties.TryGetValue(TechniqueStyle, out var style) &&
               string.Equals(style, "LegacyZBuffer", StringComparison.OrdinalIgnoreCase);
    }

    public static string ReadAuxiliaryTechniqueMode(GraphNode? outputNode, string modePropertyName, string legacyPropertyName)
    {
        if (outputNode is not null &&
            outputNode.Properties.TryGetValue(modePropertyName, out var modeValue) &&
            !string.IsNullOrWhiteSpace(modeValue))
        {
            return modeValue.Trim();
        }

        if (outputNode is not null &&
            outputNode.Properties.TryGetValue(legacyPropertyName, out var legacyValue) &&
            bool.TryParse(legacyValue, out var legacyEnabled))
        {
            return legacyEnabled ? "Empty" : "Disabled";
        }

        return "Auto";
    }

    public static bool IsScenePostProcess(GraphNode outputNode)
    {
        return outputNode.Properties.TryGetValue(Pipeline, out var mode) &&
               string.Equals(mode, "ScenePostProcess", StringComparison.OrdinalIgnoreCase);
    }
}

public static class ResourceNodeProperties
{
    public const string ResourceName = "ResourceName";
    public const string SourcePath = "SourcePath";
    public const string TextureMode = "TextureMode";
    public const string MipLevels = "MipLevels";
    public const string BakedSourcePath = "BakedSourcePath";
    public const string BakedResourceName = "BakedResourceName";
    public const string BakedMipAtlasLevels = "BakedMipAtlasLevels";
    public const string BakedAtlasBaseWidth = "BakedAtlasBaseWidth";
    public const string BakedAtlasBaseHeight = "BakedAtlasBaseHeight";
    public const string HdriSourcePath = "HdriSourcePath";

    public static void SetPrimaryResource(GraphNode node, string preparedPath, string? resourceName = null)
    {
        node.Properties[SourcePath] = preparedPath;

        var effectiveResourceName = string.IsNullOrWhiteSpace(resourceName)
            ? Path.GetFileName(preparedPath)
            : resourceName.Trim();
        if (!string.IsNullOrWhiteSpace(effectiveResourceName))
        {
            node.Properties[ResourceName] = effectiveResourceName;
        }
    }

    public static void ClearBakedResource(GraphNode node)
    {
        node.Properties.Remove(BakedSourcePath);
        node.Properties.Remove(BakedResourceName);
        node.Properties.Remove(BakedMipAtlasLevels);
        node.Properties.Remove(BakedAtlasBaseWidth);
        node.Properties.Remove(BakedAtlasBaseHeight);
    }

    public static void SetBakedResource(GraphNode node, string outputPath, string resourceName, int levelCount, int? baseWidth = null, int? baseHeight = null)
    {
        node.Properties[BakedSourcePath] = outputPath;
        node.Properties[BakedResourceName] = resourceName;
        node.Properties[BakedMipAtlasLevels] = levelCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (baseWidth.HasValue)
        {
            node.Properties[BakedAtlasBaseWidth] = baseWidth.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (baseHeight.HasValue)
        {
            node.Properties[BakedAtlasBaseHeight] = baseHeight.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public static bool TryGetDisplayResourceName(GraphNode node, out string resourceName)
    {
        resourceName = string.Empty;
        if (node.Properties.TryGetValue(BakedResourceName, out var bakedName) &&
            !string.IsNullOrWhiteSpace(bakedName))
        {
            resourceName = bakedName.Trim();
            return true;
        }

        if (node.Properties.TryGetValue(ResourceName, out var rawName) &&
            !string.IsNullOrWhiteSpace(rawName))
        {
            resourceName = rawName.Trim();
            return true;
        }

        return false;
    }

    public static bool TryGetSourcePath(GraphNode node, out string sourcePath)
    {
        sourcePath = string.Empty;
        if (!node.Properties.TryGetValue(SourcePath, out var rawValue) ||
            string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        sourcePath = rawValue.Trim();
        return true;
    }

    public static bool HasBakedResource(GraphNode node)
    {
        return node.Properties.TryGetValue(BakedSourcePath, out var bakedSourcePath) &&
               !string.IsNullOrWhiteSpace(bakedSourcePath);
    }

    public static bool IsAnimated(GraphNode node)
    {
        return node.Properties.TryGetValue(TextureMode, out var textureMode) &&
               string.Equals(textureMode, "Animated", StringComparison.OrdinalIgnoreCase);
    }

    public static int ReadMipLevels(GraphNode node, int fallback = 1)
    {
        if (!node.Properties.TryGetValue(MipLevels, out var rawValue) ||
            string.IsNullOrWhiteSpace(rawValue) ||
            !int.TryParse(rawValue, out var parsed))
        {
            return fallback;
        }

        return Math.Max(0, parsed);
    }
}

public static class BufferNodeProperties
{
    public const string BufferName = "BufferName";
    public const string Description = "Description";
    public const string Format = "Format";
    public const string FilterMode = "FilterMode";
    public const string AntiAlias = "AntiAlias";
    public const string EffectBinding = "EffectBinding";
    public const string EffectFile = "EffectFile";

    public static string ReadBufferName(GraphNode node, string fallback)
    {
        return ReadText(node, BufferName, fallback);
    }

    public static string ReadDescription(GraphNode node, string fallback = "")
    {
        return ReadText(node, Description, fallback);
    }

    public static string ReadFormat(GraphNode node, string fallback)
    {
        return ReadText(node, Format, fallback);
    }

    public static string ReadFilterMode(GraphNode node, string fallback)
    {
        return ReadText(node, FilterMode, fallback);
    }

    public static bool ReadAntiAlias(GraphNode node, bool fallback = true)
    {
        var fallbackText = fallback ? "True" : "False";
        return string.Equals(ReadText(node, AntiAlias, fallbackText), "True", StringComparison.OrdinalIgnoreCase);
    }

    public static string ResolveDefaultEffect(GraphNode node, string fallbackWhenUnset = "")
    {
        var effectBinding = ReadText(node, EffectBinding, string.Empty);
        if (!string.IsNullOrWhiteSpace(effectBinding))
        {
            return effectBinding.Trim();
        }

        var effectFile = ReadText(node, EffectFile, string.Empty);
        if (string.IsNullOrWhiteSpace(effectFile))
        {
            return fallbackWhenUnset;
        }

        if (string.Equals(effectFile, "none", StringComparison.OrdinalIgnoreCase))
        {
            return "self = hide; * = none;";
        }

        return $"self = hide; * = {effectFile.Trim()};";
    }

    public static string ReadNamedBuffer(GraphNode node, string propertyName, string fallback)
    {
        return ReadText(node, propertyName, fallback);
    }

    private static string ReadText(GraphNode node, string key, string fallback)
    {
        if (!node.Properties.TryGetValue(key, out var value))
        {
            return fallback;
        }

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}

public static class VirtualDirectionalShadowNodeProperties
{
    public const string ControllerName = "ControllerName";
    public const string AnchorObjectName = "AnchorObjectName";
    public const string AnchorBoneItem = "AnchorBoneItem";
    public const string DirectionItem = "DirectionItem";
    public const string ShadowExtentItem = "ShadowExtentItem";
    public const string ShadowDepthItem = "ShadowDepthItem";
    public const string ShadowBiasItem = "ShadowBiasItem";
    public const string ShadowSoftnessItem = "ShadowSoftnessItem";
    public const string ShadowVarianceItem = "ShadowVarianceItem";
    public const string ShadowBleedItem = "ShadowBleedItem";
    public const string NearBufferName = "NearBufferName";
    public const string FarBufferName = "FarBufferName";
    public const string QualityPreset = "QualityPreset";
    public const string ProcessedShadowBufferName = "ProcessedShadowBufferName";
    public const string ProcessedShadowChannel = "ProcessedShadowChannel";
    public const string ProcessedShadowBlend = "ProcessedShadowBlend";

    public static string Read(GraphNode node, string propertyName, string fallback)
    {
        if (!node.Properties.TryGetValue(propertyName, out var value))
        {
            return fallback;
        }

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    public static string ReadDual(GraphNode node, string specificPropertyName, string fallbackPropertyName, string defaultValue)
    {
        if (node.Properties.TryGetValue(specificPropertyName, out var specificValue) &&
            !string.IsNullOrWhiteSpace(specificValue))
        {
            return specificValue.Trim();
        }

        if (node.Properties.TryGetValue(fallbackPropertyName, out var fallbackValue) &&
            !string.IsNullOrWhiteSpace(fallbackValue))
        {
            return fallbackValue.Trim();
        }

        return defaultValue;
    }

    public static Dictionary<string, string> CreateDualProxyProperties(GraphNode sourceNode, bool selfSystem)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ControllerName] = ReadDual(sourceNode, selfSystem ? "SelfControllerName" : "CastControllerName", ControllerName, selfSystem ? "virtual_directional_shadow_self.pmx" : "virtual_directional_shadow_cast.pmx"),
            [AnchorObjectName] = ReadDual(sourceNode, AnchorObjectName, AnchorObjectName, "(self)"),
            [AnchorBoneItem] = ReadDual(sourceNode, AnchorBoneItem, AnchorBoneItem, "センター"),
            [DirectionItem] = ReadDual(sourceNode, selfSystem ? "SelfDirectionItem" : "CastDirectionItem", DirectionItem, "Direction"),
            [ShadowExtentItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowExtentItem" : "CastShadowExtentItem", ShadowExtentItem, "ShadowExtent"),
            [ShadowDepthItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowDepthItem" : "CastShadowDepthItem", ShadowDepthItem, "ShadowDepth"),
            [ShadowBiasItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowBiasItem" : "CastShadowBiasItem", ShadowBiasItem, "ShadowBias"),
            [ShadowSoftnessItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowSoftnessItem" : "CastShadowSoftnessItem", ShadowSoftnessItem, "ShadowSoftness"),
            [ShadowVarianceItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowVarianceItem" : "CastShadowVarianceItem", ShadowVarianceItem, "ShadowVariance"),
            [ShadowBleedItem] = ReadDual(sourceNode, selfSystem ? "SelfShadowBleedItem" : "CastShadowBleedItem", ShadowBleedItem, "ShadowBleed"),
            [NearBufferName] = ReadDual(sourceNode, selfSystem ? "SelfNearBufferName" : "CastNearBufferName", NearBufferName, selfSystem ? "VirtualDirSelfLightShadowMap" : "VirtualDirCastLightShadowMap"),
            [FarBufferName] = ReadDual(sourceNode, selfSystem ? "SelfFarBufferName" : "CastFarBufferName", FarBufferName, selfSystem ? "VirtualDirSelfLightShadowMapFar" : "VirtualDirCastLightShadowMapFar"),
            [QualityPreset] = ReadDual(sourceNode, selfSystem ? "SelfQualityPreset" : "CastQualityPreset", QualityPreset, "VDS_QUALITY_HIGH"),
            ["NearExtentScale"] = ReadDual(sourceNode, selfSystem ? "SelfNearExtentScale" : "CastNearExtentScale", "NearExtentScale", selfSystem ? "0.55" : "0.90"),
            ["FarExtentScale"] = ReadDual(sourceNode, selfSystem ? "SelfFarExtentScale" : "CastFarExtentScale", "FarExtentScale", selfSystem ? "1.70" : "3.20"),
            ["NearDepthScale"] = ReadDual(sourceNode, selfSystem ? "SelfNearDepthScale" : "CastNearDepthScale", "NearDepthScale", selfSystem ? "0.75" : "0.90"),
            ["FarDepthScale"] = ReadDual(sourceNode, selfSystem ? "SelfFarDepthScale" : "CastFarDepthScale", "FarDepthScale", selfSystem ? "2.20" : "3.80"),
            ["BlendStart"] = ReadDual(sourceNode, selfSystem ? "SelfBlendStart" : "CastBlendStart", "BlendStart", selfSystem ? "0.45" : "0.55"),
            ["BlendEnd"] = ReadDual(sourceNode, selfSystem ? "SelfBlendEnd" : "CastBlendEnd", "BlendEnd", selfSystem ? "0.82" : "0.90"),
            ["Threshold"] = ReadDual(sourceNode, selfSystem ? "SelfThreshold" : "CastThreshold", "Threshold", "0.55"),
            ["Softness"] = ReadDual(sourceNode, selfSystem ? "SelfSoftness" : "CastSoftness", "Softness", "0.10"),
            ["ShadowAAMode"] = ReadDual(sourceNode, selfSystem ? "SelfShadowAAMode" : "CastShadowAAMode", "ShadowAAMode", "High"),
            ["ShadowStrength"] = ReadDual(sourceNode, selfSystem ? "SelfShadowStrength" : "CastShadowStrength", "ShadowStrength", "1.0"),
            [ProcessedShadowBufferName] = ReadDual(sourceNode, ProcessedShadowBufferName, ProcessedShadowBufferName, string.Empty),
            [ProcessedShadowChannel] = ReadDual(sourceNode, ProcessedShadowChannel, ProcessedShadowChannel, "R"),
            [ProcessedShadowBlend] = ReadDual(sourceNode, ProcessedShadowBlend, ProcessedShadowBlend, "0.0"),
            ["Invert"] = "False",
        };
    }
}

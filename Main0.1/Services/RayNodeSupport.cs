using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public static class RayNodeSupport
{
    private static readonly HashSet<NodeKind> ConstantNodes =
    [
        NodeKind.Scalar,
        NodeKind.Color,
        NodeKind.Float2Value,
        NodeKind.Float3Value,
        NodeKind.Float4Value,
    ];

    private static readonly HashSet<NodeKind> CompatibleMathNodes =
    [
        NodeKind.Add,
        NodeKind.Subtract,
        NodeKind.Multiply,
        NodeKind.Divide,
        NodeKind.Min,
        NodeKind.Max,
        NodeKind.Abs,
        NodeKind.Sign,
        NodeKind.Power,
        NodeKind.Floor,
        NodeKind.Frac,
        NodeKind.Ceil,
        NodeKind.Truncate,
        NodeKind.Round,
        NodeKind.Modulo,
        NodeKind.Sine,
        NodeKind.Cosine,
        NodeKind.Tangent,
        NodeKind.ArcSine,
        NodeKind.ArcCosine,
        NodeKind.ArcTangent,
        NodeKind.ToRadians,
        NodeKind.ToDegrees,
        NodeKind.SquareRoot,
        NodeKind.ReciprocalSquareRoot,
        NodeKind.Logarithm,
        NodeKind.Exponent,
        NodeKind.Clamp,
        NodeKind.Step,
        NodeKind.SmoothStep,
        NodeKind.LessThan,
        NodeKind.GreaterThan,
        NodeKind.LessEqual,
        NodeKind.GreaterEqual,
        NodeKind.Equal,
        NodeKind.NotEqual,
        NodeKind.Saturate,
        NodeKind.Lerp,
        NodeKind.OneMinus,
        NodeKind.Remap,
        NodeKind.SplitColor,
        NodeKind.SplitXY,
        NodeKind.SplitXYZ,
        NodeKind.SplitXYZW,
        NodeKind.ComposeColor,
        NodeKind.AppendFloat2,
        NodeKind.MergeXYZ,
        NodeKind.MergeXYZW,
        NodeKind.ComponentMask,
        NodeKind.ColorRamp,
        NodeKind.RgbCurve,
        NodeKind.ColorAdjust,
    ];

    private static readonly HashSet<NodeKind> AdvancedOnlyNodes =
    [
        NodeKind.Time,
        NodeKind.Normalize,
        NodeKind.Dot,
        NodeKind.Cross,
        NodeKind.Length,
        NodeKind.Distance,
        NodeKind.Project,
        NodeKind.UvTransform,
        NodeKind.Panner,
        NodeKind.UvRotate,
        NodeKind.LayerBlend,
        NodeKind.RayEmissivePulse,
        NodeKind.RayLightDirection,
        NodeKind.RayLightingMix,
        NodeKind.RayNormalStrength,
        NodeKind.RayCustomData,
        NodeKind.RayReflectionBridge,
        NodeKind.RayClearCoatBridge,
        NodeKind.RayAnisotropyBridge,
        NodeKind.RayClothBridge,
        NodeKind.RaySkinSssBridge,
        NodeKind.RayToonCelBridge,
        NodeKind.RayBrdfToRayBridge,
        NodeKind.RayShadingOutput,
        NodeKind.RaySceneColor,
        NodeKind.RaySceneDepth,
        NodeKind.RaySceneNormal,
        NodeKind.RaySsao,
        NodeKind.RayMultiLight,
        NodeKind.RayAccumulatedLighting,
        NodeKind.RaySunLightData,
        NodeKind.RayDebugView,
        NodeKind.RayIblReflection,
        NodeKind.RayShadowData,
        NodeKind.RaySkinAdvanced,
        NodeKind.RaySnowLayer,
        NodeKind.RayDustLayer,
        NodeKind.RayEdgeWear,
        NodeKind.RayMaterialLayer,
        NodeKind.RaySsrReflection,
        NodeKind.RayOutlineChannel,
        NodeKind.RayFogChannel,
        NodeKind.RayIblSplit,
        NodeKind.RayChannelSplit,
        NodeKind.RayMaterialDiagnostic,
        NodeKind.RayFogDepthBlend,
        NodeKind.RayControllerInput,
        NodeKind.RayPostParameter,
        NodeKind.RayDebugController,
        NodeKind.Wetness,
        NodeKind.TriplanarBoxmap,
        NodeKind.DepthFade,
        NodeKind.BoxMask,
        NodeKind.SphereMask,
        NodeKind.SlopeMask,
        NodeKind.Lambert,
        NodeKind.HalfLambert,
        NodeKind.Fresnel,
        NodeKind.FresnelSchlick,
        NodeKind.RimLight,
        NodeKind.ClearCoat,
        NodeKind.Bssrdf,
        NodeKind.SubsurfaceScattering,
        NodeKind.GGXSpecular,
        NodeKind.BurleyDiffuse,
        NodeKind.BRDFLighting,
        NodeKind.SmithJointGGX,
        NodeKind.CookTorranceSpecular,
        NodeKind.AnisotropicGGXSpecular,
        NodeKind.KelemenSzirmayKalosSpecular,
        NodeKind.SkinPreintegratedLut,
        NodeKind.DiffuseShadow,
        NodeKind.GenshinRamp,
        NodeKind.SnowBreakRamp,
        NodeKind.ShadowRampColor,
        NodeKind.MatCapBlendMode,
        NodeKind.ShadowThreshold,
        NodeKind.ShadowSoftness,
        NodeKind.ShadowColorMix,
        NodeKind.MatCapMix,
        NodeKind.ToonRampSample,
        NodeKind.GenericRampSample,
        NodeKind.TexCoord,
        NodeKind.TextureCoordinate,
        NodeKind.MatCapUv,
        NodeKind.LocalNormal,
        NodeKind.WorldNormal,
        NodeKind.ViewDirection,
        NodeKind.NoiseTexture,
        NodeKind.VoronoiTexture,
        NodeKind.FbmNoise,
        NodeKind.CellEdgeTexture,
        NodeKind.GradientTexture,
        NodeKind.CheckerTexture,
        NodeKind.BrickTexture,
        NodeKind.WaveTexture,
        NodeKind.ParallaxUv,
        NodeKind.NormalMap,
        NodeKind.DetailNormalBlend,
    ];

    public static string GetBadge(NodeKind kind, bool advancedMode)
    {
        if (kind == NodeKind.RayMaterialOutput)
        {
            return "Ray 输出";
        }

        if (kind == NodeKind.RayTextureSlot)
        {
            return advancedMode ? "贴图/逐像素" : "Ray 贴图";
        }

        if (kind is NodeKind.Reroute or NodeKind.Frame)
        {
            return "布局";
        }

        if (ConstantNodes.Contains(kind))
        {
            return "常量";
        }

        if (CompatibleMathNodes.Contains(kind))
        {
            return advancedMode ? "常量/逐像素" : "常量折算";
        }

        if (AdvancedOnlyNodes.Contains(kind))
        {
            return "仅高级";
        }

        return "未支持";
    }

    public static bool RequiresAdvancedMode(NodeKind kind) => AdvancedOnlyNodes.Contains(kind);

    public static string GetDescription(NodeKind kind, bool advancedMode)
    {
        if (kind == NodeKind.RayMaterialOutput)
        {
            return "Ray 材质输出节点。兼容模式导出 Ray 原生参数；高级模式可对部分槽写入逐像素覆盖。";
        }

        if (kind == NodeKind.RayTextureSlot)
        {
            return advancedMode
                ? "文件贴图可参与高级模式逐像素表达式；PMX 主贴图/Sphere/Toon 仍建议走兼容参数。"
                : "兼容模式会把贴图映射到 Ray 原生 MAP_FROM/MAP_TYPE/MAP_FILE 参数。";
        }

        if (ConstantNodes.Contains(kind))
        {
            return "常量输入。兼容模式会直接折算进 material_2.0.fx；高级模式也可参与逐像素表达式。";
        }

        if (CompatibleMathNodes.Contains(kind))
        {
            return advancedMode
                ? "高级模式可生成 HLSL 逐像素表达式；如果所有输入都是常量，兼容模式也会在导出时折算。"
                : "兼容模式只支持常量折算；如果接入文件贴图逐像素运算，请切到高级节点模式。";
        }

        if (AdvancedOnlyNodes.Contains(kind))
        {
            return advancedMode
                ? "该节点会写入 patched material_common_2.0.fxsub，只影响导出副本。"
                : "该节点需要高级节点模式；兼容模式不会生成 Ray 原生等价参数。";
        }

        if (kind is NodeKind.Reroute or NodeKind.Frame)
        {
            return "布局节点，不参与导出计算。";
        }

        return "该节点暂未纳入 Ray-MMD 专用导出集合。";
    }

    public static string GetSearchGroup(NodeKind kind, bool advancedMode)
    {
        if (kind == NodeKind.RayMaterialOutput)
        {
            return "Ray 输出";
        }

        if (kind == NodeKind.RayTextureSlot)
        {
            return "Ray 贴图";
        }

        if (kind is NodeKind.Reroute or NodeKind.Frame)
        {
            return "布局";
        }

        if (AdvancedOnlyNodes.Contains(kind))
        {
            return advancedMode ? "高级节点" : "仅高级";
        }

        if (ConstantNodes.Contains(kind))
        {
            return "输入";
        }

        if (kind is NodeKind.NoiseTexture or NodeKind.VoronoiTexture or NodeKind.FbmNoise or NodeKind.CellEdgeTexture or NodeKind.GradientTexture or NodeKind.CheckerTexture or NodeKind.BrickTexture or NodeKind.WaveTexture)
        {
            return "程序纹理";
        }

        return "数学";
    }
}

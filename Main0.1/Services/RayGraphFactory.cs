using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public static class RayGraphFactory
{
    public static NodeGraph CreateDefault()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        graph.AddNode(NodeKind.Color, 80, 80);
        graph.AddNode(NodeKind.Scalar, 80, 240);
        graph.AddNode(NodeKind.RayMaterialOutput, 520, 150);
        return graph;
    }

    public static bool IsRayNodeAllowed(NodeKind kind)
    {
        return kind is
            NodeKind.Scalar or
            NodeKind.Color or
            NodeKind.Float2Value or
            NodeKind.Float3Value or
            NodeKind.Float4Value or
            NodeKind.Add or
            NodeKind.Subtract or
            NodeKind.Multiply or
            NodeKind.Divide or
            NodeKind.Min or
            NodeKind.Max or
            NodeKind.Abs or
            NodeKind.Sign or
            NodeKind.Power or
            NodeKind.Floor or
            NodeKind.Frac or
            NodeKind.Ceil or
            NodeKind.Truncate or
            NodeKind.Round or
            NodeKind.Modulo or
            NodeKind.Sine or
            NodeKind.Cosine or
            NodeKind.Tangent or
            NodeKind.ArcSine or
            NodeKind.ArcCosine or
            NodeKind.ArcTangent or
            NodeKind.ToRadians or
            NodeKind.ToDegrees or
            NodeKind.SquareRoot or
            NodeKind.ReciprocalSquareRoot or
            NodeKind.Logarithm or
            NodeKind.Exponent or
            NodeKind.Normalize or
            NodeKind.Dot or
            NodeKind.Cross or
            NodeKind.Length or
            NodeKind.Distance or
            NodeKind.Project or
            NodeKind.Clamp or
            NodeKind.Step or
            NodeKind.SmoothStep or
            NodeKind.LessThan or
            NodeKind.GreaterThan or
            NodeKind.LessEqual or
            NodeKind.GreaterEqual or
            NodeKind.Equal or
            NodeKind.NotEqual or
            NodeKind.Saturate or
            NodeKind.Lerp or
            NodeKind.OneMinus or
            NodeKind.Remap or
            NodeKind.SplitColor or
            NodeKind.SplitXY or
            NodeKind.SplitXYZ or
            NodeKind.SplitXYZW or
            NodeKind.ComposeColor or
            NodeKind.AppendFloat2 or
            NodeKind.MergeXYZ or
            NodeKind.MergeXYZW or
            NodeKind.ComponentMask or
            NodeKind.ColorRamp or
            NodeKind.RgbCurve or
            NodeKind.ColorAdjust or
            NodeKind.LayerBlend or
            NodeKind.RayLightDirection or
            NodeKind.RayLightingMix or
            NodeKind.RayNormalStrength or
            NodeKind.RayCustomData or
            NodeKind.RayReflectionBridge or
            NodeKind.RayClearCoatBridge or
            NodeKind.RayAnisotropyBridge or
            NodeKind.RayClothBridge or
            NodeKind.RaySkinSssBridge or
            NodeKind.RayToonCelBridge or
            NodeKind.RayBrdfToRayBridge or
            NodeKind.Lambert or
            NodeKind.HalfLambert or
            NodeKind.Fresnel or
            NodeKind.FresnelSchlick or
            NodeKind.RimLight or
            NodeKind.ClearCoat or
            NodeKind.Bssrdf or
            NodeKind.SubsurfaceScattering or
            NodeKind.GGXSpecular or
            NodeKind.BurleyDiffuse or
            NodeKind.BRDFLighting or
            NodeKind.SmithJointGGX or
            NodeKind.CookTorranceSpecular or
            NodeKind.AnisotropicGGXSpecular or
            NodeKind.KelemenSzirmayKalosSpecular or
            NodeKind.SkinPreintegratedLut or
            NodeKind.DiffuseShadow or
            NodeKind.GenshinRamp or
            NodeKind.SnowBreakRamp or
            NodeKind.ShadowRampColor or
            NodeKind.MatCapBlendMode or
            NodeKind.ShadowThreshold or
            NodeKind.ShadowSoftness or
            NodeKind.ShadowColorMix or
            NodeKind.MatCapMix or
            NodeKind.ToonRampSample or
            NodeKind.GenericRampSample or
            NodeKind.TexCoord or
            NodeKind.TextureCoordinate or
            NodeKind.MatCapUv or
            NodeKind.LocalNormal or
            NodeKind.WorldNormal or
            NodeKind.ViewDirection or
            NodeKind.NoiseTexture or
            NodeKind.VoronoiTexture or
            NodeKind.FbmNoise or
            NodeKind.CellEdgeTexture or
            NodeKind.GradientTexture or
            NodeKind.CheckerTexture or
            NodeKind.BrickTexture or
            NodeKind.WaveTexture or
            NodeKind.UvTransform or
            NodeKind.Panner or
            NodeKind.UvRotate or
            NodeKind.ParallaxUv or
            NodeKind.NormalMap or
            NodeKind.DetailNormalBlend or
            NodeKind.Time or
            NodeKind.RayEmissivePulse or
            NodeKind.RayTextureSlot or
            NodeKind.RayMaterialOutput or
            NodeKind.RayShadingOutput or
            NodeKind.RaySceneColor or
            NodeKind.RaySceneDepth or
            NodeKind.RaySceneNormal or
            NodeKind.RaySsao or
            NodeKind.RayMultiLight or
            NodeKind.RayAccumulatedLighting or
            NodeKind.RaySunLightData or
            NodeKind.RayDebugView or
            NodeKind.RayIblReflection or
            NodeKind.RayShadowData or
            NodeKind.RaySkinAdvanced or
            NodeKind.RaySnowLayer or
            NodeKind.RayDustLayer or
            NodeKind.RayEdgeWear or
            NodeKind.RayMaterialLayer or
            NodeKind.RaySsrReflection or
            NodeKind.RayOutlineChannel or
            NodeKind.RayFogChannel or
            NodeKind.RayIblSplit or
            NodeKind.RayChannelSplit or
            NodeKind.RayMaterialDiagnostic or
            NodeKind.RayFogDepthBlend or
            NodeKind.RayControllerInput or
            NodeKind.RayPostParameter or
            NodeKind.RayDebugController or
            NodeKind.Wetness or
            NodeKind.TriplanarBoxmap or
            NodeKind.DepthFade or
            NodeKind.BoxMask or
            NodeKind.SphereMask or
            NodeKind.SlopeMask or
            NodeKind.Reroute or
            NodeKind.Frame;
    }
}

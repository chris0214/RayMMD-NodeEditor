namespace RayMmdNodeEditor.Graph;

public static class GraphWorkspaceRules
{
    private static readonly HashSet<NodeKind> ScreenSpaceKinds =
    [
        NodeKind.SceneColor,
        NodeKind.SceneDepth,
        NodeKind.SceneViewRay,
        NodeKind.LayerSource,
        NodeKind.OffscreenBufferSample,
        NodeKind.DepthVisualize,
        NodeKind.ScreenUvOffset,
        NodeKind.BufferCompare,
            NodeKind.DepthEdgeDetect,
            NodeKind.NormalEdgeDetect,
            NodeKind.MaskBufferDebug,
        NodeKind.ScreenSpaceRimCompose,
        NodeKind.BrightExtract,
        NodeKind.GaussianBlur,
        NodeKind.SceneNormalSample,
        NodeKind.ViewPositionFromDepth,
        NodeKind.BilateralBlur,
        NodeKind.Downsample,
        NodeKind.UpsampleBlend,
    ];

    private static readonly HashSet<NodeKind> StylizedExperimentalKinds =
    [
        NodeKind.RimShadow,
        NodeKind.ShadowRampColor,
        NodeKind.MatCapBlendMode,
    ];

    private static readonly HashSet<NodeKind> BufferExperimentalKinds =
    [
        NodeKind.RimDepthBuffer,
        NodeKind.RimMaskBuffer,
        NodeKind.LayerSourceOutput,
    ];

    private static readonly HashSet<NodeKind> BufferShadingKinds =
    [
        NodeKind.HalfVector,
        NodeKind.NdotL,
        NodeKind.NdotV,
        NodeKind.NdotH,
        NodeKind.Lambert,
        NodeKind.HalfLambert,
        NodeKind.BlinnPhong,
        NodeKind.SelfShadowLighting,
        NodeKind.RimLight,
    ];

    private static readonly HashSet<NodeKind> SceneUnsupportedMathKinds =
    [
        NodeKind.Fresnel,
    ];

    private static readonly HashSet<NodeKind> VertexStageKinds =
    [
        NodeKind.Scalar,
        NodeKind.Color,
        NodeKind.Float2Value,
        NodeKind.Float3Value,
        NodeKind.Float4Value,
        NodeKind.Time,
        NodeKind.ElapsedTime,
        NodeKind.SharedParameter,
        NodeKind.LocalPosition,
        NodeKind.LocalNormal,
        NodeKind.WorldPosition,
        NodeKind.WorldNormal,
        NodeKind.CameraPosition,
        NodeKind.ControlObjectPosition,
        NodeKind.ControlObjectValue,
        NodeKind.ControlObjectRotation,
        NodeKind.ControllerLightDirection,
        NodeKind.ControlObjectBonePosition,
        NodeKind.ControlObjectBoneDirection,
        NodeKind.ControlObjectCenter,
        NodeKind.ControlObjectVector,
        NodeKind.ControlObjectTransformDirection,
        NodeKind.ControlObjectAngleDirection,
        NodeKind.OffsetAlongNormal,
        NodeKind.VertexWave,
        NodeKind.AxisMask,
        NodeKind.Twist,
        NodeKind.Bend,
        NodeKind.NoiseDisplace,
    ];

    public static bool IsAllowed(GraphWorkspaceMode mode, NodeKind kind)
    {
        if (kind == NodeKind.Frame)
        {
            return true;
        }

        if (kind == NodeKind.Group)
        {
            return true;
        }

        if (kind is NodeKind.GroupInput or NodeKind.GroupOutput)
        {
            return false;
        }

        if (kind == NodeKind.Output)
        {
            return true;
        }

        if (kind == NodeKind.MaterialOutput)
        {
            return mode == GraphWorkspaceMode.ObjectMaterial;
        }

        return mode switch
        {
            GraphWorkspaceMode.ScenePostProcess => IsSceneAllowed(kind),
            GraphWorkspaceMode.BufferPass => IsBufferAllowed(kind),
            _ => IsObjectAllowed(kind),
        };
    }

    public static IEnumerable<NodeKind> GetDisallowedKinds(NodeGraph graph)
    {
        return graph.Nodes
            .Select(node => node.Kind)
            .Where(kind => !IsAllowed(graph.WorkspaceMode, kind))
            .Distinct();
    }

    public static bool IsVertexStageAllowed(GraphWorkspaceMode mode, NodeKind kind)
    {
        if (kind == NodeKind.Frame)
        {
            return true;
        }

        if (kind == NodeKind.Group)
        {
            return true;
        }

        if (mode == GraphWorkspaceMode.ScenePostProcess)
        {
            return false;
        }

        if (kind == NodeKind.Output || kind == NodeKind.MaterialOutput)
        {
            return true;
        }

        if (VertexStageKinds.Contains(kind))
        {
            return true;
        }

        return NodeRegistry.Get(kind).Category == NodeCategory.Math;
    }

    private static bool IsSceneAllowed(NodeKind kind)
    {
        if (ScreenSpaceKinds.Contains(kind))
        {
            return true;
        }

        if (SceneUnsupportedMathKinds.Contains(kind))
        {
            return false;
        }

        return kind switch
        {
            NodeKind.Scalar or
            NodeKind.Color or
            NodeKind.Float2Value or
            NodeKind.Float3Value or
            NodeKind.Float4Value or
            NodeKind.ScreenUv or
            NodeKind.SceneViewRay or
            NodeKind.Time or
            NodeKind.ElapsedTime or
            NodeKind.ViewportPixelSize or
            NodeKind.SharedParameter or
            NodeKind.ControlObjectValue or
            NodeKind.ControlObjectBoneDirection or
            NodeKind.ExternalTexture or
            NodeKind.SharedTextureSample or
            NodeKind.DiffuseEnvSample or
            NodeKind.SpecularEnvSample or
            NodeKind.DiffuseEnvBake or
            NodeKind.SpecularEnvBake or
            NodeKind.PipelineEnvironmentLighting or
            NodeKind.DomainWarp or
            NodeKind.NoiseTexture or
            NodeKind.VoronoiTexture or
            NodeKind.FbmNoise or
            NodeKind.MusgraveTexture or
            NodeKind.CellEdgeTexture or
            NodeKind.CurlNoise or
            NodeKind.AnisotropicNoise or
            NodeKind.GradientTexture or
            NodeKind.CheckerTexture or
            NodeKind.BrickTexture or
            NodeKind.WaveTexture => true,
            _ when NodeRegistry.Get(kind).Category == NodeCategory.Math => true,
            _ => false,
        };
    }

    private static bool IsBufferAllowed(NodeKind kind)
    {
        if (BufferExperimentalKinds.Contains(kind))
        {
            return true;
        }

        if (BufferShadingKinds.Contains(kind))
        {
            return true;
        }

        if (ScreenSpaceKinds.Contains(kind) || StylizedExperimentalKinds.Contains(kind))
        {
            return false;
        }

        return kind switch
        {
            _ when NodeRegistry.Get(kind).Category is NodeCategory.Input or NodeCategory.Geometry or NodeCategory.Texture or NodeCategory.Math => true,
            _ => false,
        };
    }

    private static bool IsObjectAllowed(NodeKind kind)
    {
        if (kind == NodeKind.LayerSourceOutput)
        {
            return true;
        }

        if (ScreenSpaceKinds.Contains(kind) || BufferExperimentalKinds.Contains(kind))
        {
            return false;
        }

        return true;
    }
}

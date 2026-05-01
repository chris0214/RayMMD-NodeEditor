namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    public GraphWorkspaceMode WorkspaceMode { get; set; } = GraphWorkspaceMode.ObjectMaterial;

    public List<GraphNode> Nodes { get; init; } = [];

    public List<GraphConnection> Connections { get; init; } = [];

    public GraphNode AddNode(NodeKind kind, float x, float y)
    {
        var definition = NodeRegistry.Get(kind);
        var node = new GraphNode
        {
            Kind = kind,
            X = x,
            Y = y,
        };

        foreach (var property in definition.Properties)
        {
            node.Properties[property.Name] = GetInitialPropertyValue(property);
        }

        if (kind == NodeKind.Output)
        {
            node.Properties["Pipeline"] = WorkspaceMode == GraphWorkspaceMode.ScenePostProcess
                ? "ScenePostProcess"
                : "Object";
        }

        Nodes.Add(node);
        return node;
    }

    private static string GetInitialPropertyValue(NodePropertyDefinition property)
    {
        if (property.Kind == NodePropertyKind.Text && IsFreeTextProperty(property.Name))
        {
            return string.Empty;
        }

        return property.DefaultValue;
    }

    private static bool IsFreeTextProperty(string propertyName)
    {
        return propertyName switch
        {
            "Type" => false,
            "TextureSource" => false,
            "TextureMode" => false,
            "AddressMode" => false,
            "FilterMode" => false,
            "ColorSpace" => false,
            "NoiseType" => false,
            "Dimensions" => false,
            "FractalMode" => false,
            "MusgraveType" => false,
            "GradientType" => false,
            "WaveType" => false,
            "WaveProfile" => false,
            "Source" => false,
            "StrandAxis" => false,
            "XChannel" => false,
            "YChannel" => false,
            "MaskChannel" => false,
            "Invert" => false,
            "InvertY" => false,
            "SourceSpace" => false,
            "TargetSpace" => false,
            "VectorType" => false,
            "NormalizeOutput" => false,
            "Axis" => false,
            "DiffuseMode" => false,
            "Channels" => false,
            "Mode" => false,
            "Clamp" => false,
            "LayerMode" => false,
            "MaskInvert" => false,
            "Pipeline" => false,
            "AlphaMode" => false,
            "CullMode" => false,
            "ZEnable" => false,
            "AlphaTestEnable" => false,
            "AlphaBlendEnable" => false,
            "SrcBlend" => false,
            "DestBlend" => false,
            "BlendOp" => false,
            "ZWriteEnable" => false,
            "ShadingMode" => false,
            "RenderTarget0Mode" => false,
            "SecondaryOutputMode" => false,
            "TechniqueStyle" => false,
            "ShadowTechniqueMode" => false,
            "ZplotTechniqueMode" => false,
            "ControllerName" => false,
            "DirectionItem" => false,
            "ShadowExtentItem" => false,
            "ShadowDepthItem" => false,
            "ShadowBiasItem" => false,
            "ShadowSoftnessItem" => false,
            "ShadowVarianceItem" => false,
            "ShadowBleedItem" => false,
            "NearBufferName" => false,
            "FarBufferName" => false,
            "QualityPreset" => false,
            "BlurSource" => false,
            "BlurDirection" => false,
            "Operator" => false,
            "PreserveAlpha" => false,
            "DecodeMode" => false,
            "NormalDecodeMode" => false,
            "UseLambert" => false,
            _ => true,
        };
    }

    public void RemoveNode(Guid nodeId)
    {
        Nodes.RemoveAll(node => node.Id == nodeId);
        Connections.RemoveAll(connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId);
    }

    public GraphNode? FindNode(Guid nodeId) => Nodes.FirstOrDefault(node => node.Id == nodeId);

    public GraphConnection? FindInputConnection(Guid targetNodeId, string targetPin)
    {
        return Connections.FirstOrDefault(connection =>
            connection.TargetNodeId == targetNodeId &&
            string.Equals(connection.TargetPin, targetPin, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<GraphConnection> GetOutgoing(Guid sourceNodeId)
    {
        return Connections.Where(connection => connection.SourceNodeId == sourceNodeId);
    }

    public void AddOrReplaceConnection(GraphConnection connection)
    {
        Connections.RemoveAll(existing =>
            existing.TargetNodeId == connection.TargetNodeId &&
            string.Equals(existing.TargetPin, connection.TargetPin, StringComparison.OrdinalIgnoreCase));

        Connections.Add(connection);
    }

    public void RemoveConnection(GraphConnection connection)
    {
        Connections.RemoveAll(existing =>
            existing.SourceNodeId == connection.SourceNodeId &&
            existing.TargetNodeId == connection.TargetNodeId &&
            string.Equals(existing.SourcePin, connection.SourcePin, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.TargetPin, connection.TargetPin, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveInputConnection(Guid targetNodeId, string targetPin)
    {
        Connections.RemoveAll(existing =>
            existing.TargetNodeId == targetNodeId &&
            string.Equals(existing.TargetPin, targetPin, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveOutgoingConnections(Guid sourceNodeId, string sourcePin)
    {
        Connections.RemoveAll(existing =>
            existing.SourceNodeId == sourceNodeId &&
            string.Equals(existing.SourcePin, sourcePin, StringComparison.OrdinalIgnoreCase));
    }

    public static NodeGraph CreateDefault()
    {
        return CreateDefault(GraphWorkspaceMode.ObjectMaterial);
    }

    public static NodeGraph CreateDefault(GraphWorkspaceMode workspaceMode)
    {
        return workspaceMode switch
        {
            GraphWorkspaceMode.BufferPass => CreateRimDepthBufferSample(),
            GraphWorkspaceMode.ScenePostProcess => CreateSceneColorPostProcessSample(),
            _ => CreateDefaultObjectGraph(),
        };
    }

    private static NodeGraph CreateDefaultObjectGraph()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };
        graph.AddNode(NodeKind.Output, 520, 180);
        return graph;
    }

    public static NodeGraph CreateColorSample()
    {
        return CreateColorSampleCore();
    }

    public static NodeGraph CreateExternalTextureSample()
    {
        return CreateExternalTextureSampleCore();
    }

    public static NodeGraph CreateParallaxSample()
    {
        return CreateParallaxSampleCore();
    }

    public static NodeGraph CreateTriplanarSample()
    {
        return CreateTriplanarSampleCore();
    }

    public static NodeGraph CreateMaterialUtilitySample()
    {
        return CreateMaterialUtilitySampleCore();
    }

    public static NodeGraph CreateProceduralNoiseSample()
    {
        return CreateProceduralNoiseSampleCore();
    }

    public static NodeGraph CreatePatternTextureSample()
    {
        return CreatePatternTextureSampleCore();
    }

    public static NodeGraph CreateFaceSdfShadowSample()
    {
        return CreateFaceSdfShadowSampleCore();
    }

    public static NodeGraph CreateVertexNormalOffsetSample()
    {
        return CreateVertexNormalOffsetSampleCore();
    }

    public static NodeGraph CreateVertexWaveSample()
    {
        return CreateVertexWaveSampleCore();
    }

    public static NodeGraph CreateControllerVertexDisplacementSample()
    {
        return CreateControllerVertexDisplacementSampleCore();
    }

    public static NodeGraph CreateShellOffsetSample()
    {
        return CreateShellOffsetSampleCore();
    }

    public static NodeGraph CreateTwistSample()
    {
        return CreateTwistSampleCore();
    }

    public static NodeGraph CreateBendSample()
    {
        return CreateBendSampleCore();
    }

    public static NodeGraph CreateNoiseDisplaceSample()
    {
        return CreateNoiseDisplaceSampleCore();
    }

    public static NodeGraph CreateOffsetShadowCaptureSample()
    {
        return CreateOffsetShadowCaptureSampleCore();
    }

    public static NodeGraph CreateOffsetShadowCaptureVertexSample()
    {
        return CreateOffsetShadowCaptureVertexSampleCore();
    }

    public static NodeGraph CreateOffsetShadowMaskSample()
    {
        return CreateOffsetShadowMaskSampleCore();
    }

    public static NodeGraph CreateOffsetShadowComposeSample()
    {
        return CreateOffsetShadowComposeSampleCore();
    }

    public static NodeGraph CreateBreathingPulseSample()
    {
        return CreateBreathingPulseSampleCore();
    }

    public static NodeGraph CreateEyeSeeThroughSample()
    {
        return CreateEyeSeeThroughSampleCore();
    }

    public static NodeGraph CreateEyeSeeThroughSoftSample()
    {
        return CreateEyeSeeThroughSoftSampleCore();
    }

    public static NodeGraph CreateAuraShellPixelSample()
    {
        return CreateAuraShellPixelSampleCore();
    }

    public static NodeGraph CreateAuraShellVertexSample()
    {
        return CreateAuraShellVertexSampleCore();
    }

    public static NodeGraph CreateTattooFlameShellPixelSample()
    {
        return CreateTattooFlameShellPixelSampleCore();
    }

    public static NodeGraph CreateTattooBaseBodySample()
    {
        return CreateTattooBaseBodySampleCore();
    }

    public static NodeGraph CreateTattooFlameShellVertexSample()
    {
        return CreateTattooFlameShellVertexSampleCore();
    }

    public static NodeGraph CreateNormalMapSample()
    {
        return CreateNormalMapSampleCore();
    }

    public static NodeGraph CreateTransformVectorSample()
    {
        return CreateTransformVectorSampleCore();
    }

    public static NodeGraph CreateTransformPositionSample()
    {
        return CreateTransformPositionSampleCore();
    }

    public static NodeGraph CreateTextureCoordinateSample()
    {
        return CreateTextureCoordinateSampleCore();
    }

    public static NodeGraph CreateFakeEnvReflectionSample()
    {
        return CreateFakeEnvReflectionHdriSampleCore();
    }

    public static NodeGraph CreateFakeEnvReflectionBasicSample()
    {
        return CreateFakeEnvReflectionBasicSampleCore();
    }

    public static NodeGraph CreateMatCapReflectionSample()
    {
        return CreateMatCapReflectionSampleCore();
    }

    public static NodeGraph CreateMatcapAtalsSample()
    {
        return CreateMatcapAtalsSampleCore();
    }

    public static NodeGraph CreateGenericRampSample()
    {
        return CreateGenericRampSampleCore();
    }

    public static NodeGraph CreateGenshinRampSample()
    {
        return CreateGenshinRampSampleCore();
    }

    public static NodeGraph CreateSnowBreakRampSample()
    {
        return CreateSnowBreakRampSampleCore();
    }

    public static NodeGraph CreateStarRailRampSample()
    {
        return CreateStarRailRampSampleCore();
    }

    public static NodeGraph CreateSkinPreintegratedLutSample()
    {
        return CreateSkinPreintegratedLutSampleCore();
    }

    public static NodeGraph CreateFakeEnvReflectionHdriSample()
    {
        return CreateFakeEnvReflectionHdriSampleCore();
    }

    public static NodeGraph CreateControllerLightSample()
    {
        return CreateControllerLightSampleCore();
    }

    public static NodeGraph CreateControllerRotationSample()
    {
        return CreateControllerRotationSampleCore();
    }

    public static NodeGraph CreateStylizedControllerLightSample()
    {
        return CreateStylizedControllerLightSampleCore();
    }

    public static NodeGraph CreateBoneDrivenLightSample()
    {
        return CreateBoneDrivenLightSampleCore();
    }

    public static NodeGraph CreateControllerDirectionTemplateSample()
    {
        return CreateControllerDirectionTemplateSampleCore();
    }

    public static NodeGraph CreateScreenSpaceRimPostProcessSample()
    {
        return CreateScreenSpaceRimPostProcessSampleCore();
    }

    public static NodeGraph CreateScreenSpaceRimManualPostProcessSample()
    {
        return CreateScreenSpaceRimManualPostProcessSampleCore();
    }

    public static NodeGraph CreateBrightExtractPostProcessSample()
    {
        return CreateBrightExtractPostProcessSampleCore();
    }

    public static NodeGraph CreateLinearDepthBufferSample()
    {
        return CreateLinearDepthBufferSampleCore();
    }

    public static NodeGraph CreateSceneDepthPreviewSample()
    {
        return CreateSceneDepthPreviewSampleCore();
    }

    public static NodeGraph CreateDepthFogPostProcessSample()
    {
        return CreateDepthFogPostProcessSampleCore();
    }

    public static NodeGraph CreateNormalBufferPreviewSample()
    {
        return CreateNormalBufferPreviewSampleCore();
    }

    public static NodeGraph CreateSceneBlurPostProcessSample()
    {
        return CreateSceneBlurPostProcessSampleCore();
    }

    public static NodeGraph CreateSoftFocusGlowPostProcessSample()
    {
        return CreateSoftFocusGlowPostProcessSampleCore();
    }

    public static NodeGraph CreateColorAdjustTonemapLitePostProcessSample()
    {
        return CreateColorAdjustTonemapLitePostProcessSampleCore();
    }

    public static NodeGraph CreateEdgeBlurPostProcessSample()
    {
        return CreateEdgeBlurPostProcessSampleCore();
    }

    public static NodeGraph CreateBloomPostProcessSample()
    {
        return CreateBloomPostProcessSampleCore();
    }

    public static NodeGraph CreateBloomDirtMapPostProcessSample()
    {
        return CreateBloomDirtMapPostProcessSampleCore();
    }

    public static NodeGraph CreateFocusDofLiteComposeSample()
    {
        return CreateFocusDofLiteComposeSampleCore();
    }

    public static NodeGraph CreateFocusDofLiteDepthBufferSample()
    {
        return CreateFocusDofLiteDepthBufferSampleCore();
    }

    public static NodeGraph CreateBasicLightingSample()
    {
        return CreateBasicLightingSampleCore();
    }

    public static NodeGraph CreateWetnessSample()
    {
        return CreateWetnessSampleCore();
    }

    public static NodeGraph CreateBrdfLightingSample()
    {
        return CreateBrdfLightingSampleCore();
    }

    public static NodeGraph CreateKajiyaKayHairSample()
    {
        return CreateKajiyaKayHairSampleCore();
    }

    public static NodeGraph CreateKajiyaKayHairRingSample()
    {
        return CreateKajiyaKayHairRingSampleCore();
    }

    public static NodeGraph CreateVertexChannelsSample()
    {
        return CreateVertexChannelsSampleCore();
    }

    public static NodeGraph CreateSelfShadowLightingSample()
    {
        return CreateSelfShadowLightingSampleCore();
    }

    public static NodeGraph CreateSelfShadowLambertTemplate()
    {
        return CreateSelfShadowLambertTemplateCore();
    }

    public static NodeGraph CreateSelfShadowHalfLambertTemplate()
    {
        return CreateSelfShadowHalfLambertTemplateCore();
    }

    public static NodeGraph CreateSelfShadowBlinnPhongTemplate()
    {
        return CreateSelfShadowBlinnPhongTemplateCore();
    }

    public static NodeGraph CreateSelfShadowSample()
    {
        return CreateSelfShadowSampleCore();
    }

    public static NodeGraph CreateZBufferCompatibilitySample()
    {
        return CreateZBufferCompatibilitySampleCore();
    }
}

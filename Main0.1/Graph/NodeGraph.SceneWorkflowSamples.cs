namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    public static NodeGraph CreateSceneColorPostProcessSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var sceneColor = graph.AddNode(NodeKind.SceneColor, 140, 180);
        var screenUv = graph.AddNode(NodeKind.ScreenUv, 140, 60);
        var output = graph.AddNode(NodeKind.Output, 520, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateEyeSeeThroughComposeSample()
    {
        return CreateEyeSeeThroughComposeSampleCore();
    }

    private static NodeGraph CreateEyeSeeThroughComposeSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 140);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 360, 120);

        var eyeLayer = graph.AddNode(NodeKind.LayerSource, 360, 340);
        eyeLayer.Properties["BufferName"] = "EyeSeeThroughLayerRT";
        eyeLayer.Properties["EffectFile"] = "eye_see_through_standard.fx";
        eyeLayer.Properties["EffectBinding"] = "self = hide; * = eye_see_through_standard.fx;";
        eyeLayer.Properties["Description"] = "Eye see through eye capture";
        eyeLayer.Properties["Format"] = "A16B16G16R16F";
        eyeLayer.Properties["FilterMode"] = "Linear";
        eyeLayer.Properties["EyeSeeThroughRole"] = "EyeLayer";

        var eyeSplit = graph.AddNode(NodeKind.SplitColor, 700, 360);
        var opacity = graph.AddNode(NodeKind.Scalar, 700, 520);
        opacity.Properties["Value"] = "1.0";

        var blend = graph.AddNode(NodeKind.LayerBlend, 1020, 220);
        blend.Properties["LayerMode"] = "Normal";
        blend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1360, 220);
        output.Properties["Pipeline"] = "ScenePostProcess";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = eyeLayer.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = eyeLayer.Id,
            SourcePin = "Color",
            TargetNodeId = eyeSplit.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = eyeLayer.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacity.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = eyeSplit.Id,
            SourcePin = "A",
            TargetNodeId = blend.Id,
            TargetPin = "Mask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateBrightExtractPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 120, 80);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 420, 80);
        var brightExtract = graph.AddNode(NodeKind.BrightExtract, 760, 120);
        brightExtract.Properties["Threshold"] = "0.35";
        brightExtract.Properties["SoftKnee"] = "0.25";
        brightExtract.Properties["Intensity"] = "3.0";
        brightExtract.Properties["PreserveAlpha"] = "False";
        var output = graph.AddNode(NodeKind.Output, 1100, 140);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = brightExtract.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = brightExtract.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateSceneDepthPreviewSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 40f, 20f, 620f, 170f, "1. 先在 Buffer 模板里加载 Linear Depth Buffer\n导出为 linear_depth_buffer.fx\n它会把角色深度写到颜色缓冲里，避免直接读取原始 SceneDepth 纯白。", 0.26f, 0.42f, 0.72f);
        AddFrame(graph, 700f, 20f, 720f, 170f, "2. 再加载这个 Scene Depth Preview 模板\n它通过 Layer Source 调用 linear_depth_buffer.fx\n在后处理里直接预览自定义深度图。", 0.24f, 0.50f, 0.38f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 320);
        var layerSource = graph.AddNode(NodeKind.LayerSource, 380, 260);
        layerSource.Properties["BufferName"] = "LinearDepthPreviewRT";
        layerSource.Properties["EffectFile"] = "linear_depth_buffer.fx";
        layerSource.Properties["Description"] = "Linear depth preview capture";
        layerSource.Properties["Format"] = "A16B16G16R16F";
        layerSource.Properties["FilterMode"] = "Linear";

        var output = graph.AddNode(NodeKind.Output, 760, 300);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = layerSource.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerSource.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateDepthFogPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 40f, 20f, 620f, 170f, "1. 先导出 Buffer 模板里的 Linear Depth Buffer\n保存为 linear_depth_buffer.fx\nDepth Fog 将通过它读取稳定的线性深度。", 0.26f, 0.42f, 0.72f);
        AddFrame(graph, 700f, 20f, 760f, 170f, "2. 再加载这个 Depth Fog 模板\n它通过 Layer Source 调用 linear_depth_buffer.fx\n用自定义深度图驱动雾化混合。", 0.24f, 0.50f, 0.38f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 280);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 360, 180);
        var linearDepth = graph.AddNode(NodeKind.LayerSource, 360, 420);
        linearDepth.Properties["BufferName"] = "LinearDepthFogRT";
        linearDepth.Properties["EffectFile"] = "linear_depth_buffer.fx";
        linearDepth.Properties["Description"] = "Linear depth for depth fog";
        linearDepth.Properties["Format"] = "A16B16G16R16F";
        linearDepth.Properties["FilterMode"] = "Linear";

        var depthMask = graph.AddNode(NodeKind.ComponentMask, 700, 420);
        depthMask.Properties["Channels"] = "R";
        var fogFactor = graph.AddNode(NodeKind.SmoothStep, 980, 420);
        fogFactor.Properties["Min"] = "0.25";
        fogFactor.Properties["Max"] = "0.85";
        var fogColor = graph.AddNode(NodeKind.Color, 980, 180);
        fogColor.Properties["R"] = "0.70";
        fogColor.Properties["G"] = "0.78";
        fogColor.Properties["B"] = "0.90";
        fogColor.Properties["A"] = "1.0";
        var blend = graph.AddNode(NodeKind.Lerp, 1280, 280);
        blend.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1580, 300);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = linearDepth.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = linearDepth.Id,
            SourcePin = "Color",
            TargetNodeId = depthMask.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = depthMask.Id,
            SourcePin = "Result",
            TargetNodeId = fogFactor.Id,
            TargetPin = "X",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = fogColor.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = fogFactor.Id,
            SourcePin = "Result",
            TargetNodeId = blend.Id,
            TargetPin = "T",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateNormalBufferPreviewSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 180);
        var normalSample = graph.AddNode(NodeKind.SceneNormalSample, 380, 180);
        normalSample.Properties["BufferName"] = "Na_RimDepth";
        normalSample.Properties["Description"] = "Normal/depth buffer preview";
        normalSample.Properties["Format"] = "A16B16G16R16F";
        normalSample.Properties["FilterMode"] = "Linear";
        normalSample.Properties["DecodeMode"] = "EncodedXYReconstructZ";
        var scale = graph.AddNode(NodeKind.Float4Value, 680, 80);
        scale.Properties["X"] = "0.5";
        scale.Properties["Y"] = "0.5";
        scale.Properties["Z"] = "0.5";
        scale.Properties["W"] = "0.0";
        var bias = graph.AddNode(NodeKind.Float4Value, 680, 280);
        bias.Properties["X"] = "0.5";
        bias.Properties["Y"] = "0.5";
        bias.Properties["Z"] = "0.5";
        bias.Properties["W"] = "0.0";
        var multiply = graph.AddNode(NodeKind.Multiply, 980, 140);
        multiply.Properties["Type"] = "Float4";
        var add = graph.AddNode(NodeKind.Add, 1260, 140);
        add.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1540, 140);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = normalSample.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = normalSample.Id,
            SourcePin = "Normal",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = scale.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = add.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bias.Id,
            SourcePin = "Value",
            TargetNodeId = add.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = add.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateSceneBlurPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 140, 120);
        var blur = graph.AddNode(NodeKind.GaussianBlur, 500, 120);
        blur.Properties["BlurSource"] = "SceneColor";
        blur.Properties["BlurDirection"] = "Horizontal";
        blur.Properties["StepScale"] = "1.0";
        blur.Properties["Strength"] = "1.0";
        blur.Properties["PreserveAlpha"] = "True";
        var output = graph.AddNode(NodeKind.Output, 860, 140);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = blur.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blur.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateSoftFocusGlowPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 160);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 320, 120);

        var blurHorizontal = graph.AddNode(NodeKind.GaussianBlur, 620, 40);
        blurHorizontal.Properties["BlurSource"] = "SceneColor";
        blurHorizontal.Properties["BlurDirection"] = "Horizontal";
        blurHorizontal.Properties["StepScale"] = "1.5";
        blurHorizontal.Properties["Strength"] = "1.0";
        blurHorizontal.Properties["PreserveAlpha"] = "True";

        var blurVertical = graph.AddNode(NodeKind.GaussianBlur, 620, 240);
        blurVertical.Properties["BlurSource"] = "SceneColor";
        blurVertical.Properties["BlurDirection"] = "Vertical";
        blurVertical.Properties["StepScale"] = "1.5";
        blurVertical.Properties["Strength"] = "1.0";
        blurVertical.Properties["PreserveAlpha"] = "True";

        var blurMixFactor = graph.AddNode(NodeKind.Scalar, 900, 360);
        blurMixFactor.Properties["Value"] = "0.5";

        var blurMix = graph.AddNode(NodeKind.Lerp, 920, 120);
        blurMix.Properties["Type"] = "Float4";

        var brightExtract = graph.AddNode(NodeKind.BrightExtract, 620, 500);
        brightExtract.Properties["Threshold"] = "0.6";
        brightExtract.Properties["SoftKnee"] = "0.2";
        brightExtract.Properties["Intensity"] = "2.0";
        brightExtract.Properties["PreserveAlpha"] = "False";

        var glowBlurHorizontal = graph.AddNode(NodeKind.GaussianBlur, 1180, 420);
        glowBlurHorizontal.Properties["BlurSource"] = "OffscreenBuffer";
        glowBlurHorizontal.Properties["BufferName"] = "SoftGlowBright";
        glowBlurHorizontal.Properties["Description"] = "Soft glow bright buffer";
        glowBlurHorizontal.Properties["Format"] = "A16B16G16R16F";
        glowBlurHorizontal.Properties["FilterMode"] = "Linear";
        glowBlurHorizontal.Properties["BlurDirection"] = "Horizontal";
        glowBlurHorizontal.Properties["StepScale"] = "2.0";
        glowBlurHorizontal.Properties["Strength"] = "1.0";
        glowBlurHorizontal.Properties["PreserveAlpha"] = "False";

        var glowBlurVertical = graph.AddNode(NodeKind.GaussianBlur, 1180, 620);
        glowBlurVertical.Properties["BlurSource"] = "OffscreenBuffer";
        glowBlurVertical.Properties["BufferName"] = "SoftGlowBright";
        glowBlurVertical.Properties["Description"] = "Soft glow bright buffer";
        glowBlurVertical.Properties["Format"] = "A16B16G16R16F";
        glowBlurVertical.Properties["FilterMode"] = "Linear";
        glowBlurVertical.Properties["BlurDirection"] = "Vertical";
        glowBlurVertical.Properties["StepScale"] = "2.0";
        glowBlurVertical.Properties["Strength"] = "1.0";
        glowBlurVertical.Properties["PreserveAlpha"] = "False";

        var glowMixFactor = graph.AddNode(NodeKind.Scalar, 1460, 700);
        glowMixFactor.Properties["Value"] = "0.5";

        var glowMix = graph.AddNode(NodeKind.Lerp, 1480, 520);
        glowMix.Properties["Type"] = "Float4";

        var softFocusOpacity = graph.AddNode(NodeKind.Scalar, 1220, 140);
        softFocusOpacity.Properties["Value"] = "0.35";

        var glowOpacity = graph.AddNode(NodeKind.Scalar, 1780, 620);
        glowOpacity.Properties["Value"] = "0.75";

        var softFocusBlend = graph.AddNode(NodeKind.LayerBlend, 1500, 180);
        softFocusBlend.Properties["LayerMode"] = "Screen";
        softFocusBlend.Properties["MaskInvert"] = "False";

        var finalBlend = graph.AddNode(NodeKind.LayerBlend, 2080, 300);
        finalBlend.Properties["LayerMode"] = "Add";
        finalBlend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 2400, 320);
        output.Properties["RenderTarget0Name"] = "SoftGlowBright";
        output.Properties["RenderTarget0Scale"] = "0.5";

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, screenUv, "UV", blurHorizontal, "UV");
        Connect(graph, screenUv, "UV", blurVertical, "UV");
        Connect(graph, screenUv, "UV", glowBlurHorizontal, "UV");
        Connect(graph, screenUv, "UV", glowBlurVertical, "UV");

        Connect(graph, blurHorizontal, "Color", blurMix, "A");
        Connect(graph, blurVertical, "Color", blurMix, "B");
        Connect(graph, blurMixFactor, "Value", blurMix, "T");

        Connect(graph, sceneColor, "Color", brightExtract, "Color");
        Connect(graph, glowBlurHorizontal, "Color", glowMix, "A");
        Connect(graph, glowBlurVertical, "Color", glowMix, "B");
        Connect(graph, glowMixFactor, "Value", glowMix, "T");

        Connect(graph, sceneColor, "Color", softFocusBlend, "Background");
        Connect(graph, blurMix, "Result", softFocusBlend, "Foreground");
        Connect(graph, softFocusOpacity, "Value", softFocusBlend, "Opacity");

        Connect(graph, softFocusBlend, "Result", finalBlend, "Background");
        Connect(graph, glowMix, "Result", finalBlend, "Foreground");
        Connect(graph, glowOpacity, "Value", finalBlend, "Opacity");

        Connect(graph, finalBlend, "Result", output, "Color");
        Connect(graph, brightExtract, "Color", output, "CaptureColor");

        return graph;
    }

    private static NodeGraph CreateColorAdjustTonemapLitePostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 120, 140);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 380, 140);

        var colorAdjust = graph.AddNode(NodeKind.ColorAdjust, 720, 140);
        colorAdjust.Properties["Exposure"] = "0.25";
        colorAdjust.Properties["HueShift"] = "0.0";
        colorAdjust.Properties["Temperature"] = "0.03";
        colorAdjust.Properties["Tint"] = "-0.02";
        colorAdjust.Properties["Contrast"] = "1.08";
        colorAdjust.Properties["Saturation"] = "1.05";
        colorAdjust.Properties["ShadowLift"] = "0.02";
        colorAdjust.Properties["HighlightCompress"] = "0.15";
        colorAdjust.Properties["Lift"] = "0.0";
        colorAdjust.Properties["Gamma"] = "1.0";
        colorAdjust.Properties["Gain"] = "1.0";

        var tonemap = graph.AddNode(NodeKind.TonemapLite, 1040, 140);
        tonemap.Properties["Operator"] = "ACESApprox";
        tonemap.Properties["Exposure"] = "0.0";
        tonemap.Properties["WhitePoint"] = "1.5";
        tonemap.Properties["Gamma"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 1360, 140);

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, sceneColor, "Color", colorAdjust, "Color");
        Connect(graph, colorAdjust, "Color", tonemap, "Color");
        Connect(graph, tonemap, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateEdgeBlurPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 40f, 20f, 620f, 170f, "1. 先导出 Buffer 模板里的 Linear Depth Buffer\n保存为 linear_depth_buffer.fx\nEdge Blur 通过它读取稳定的线性深度，而不是直接读取原始 SceneDepth。", 0.26f, 0.42f, 0.72f);
        AddFrame(graph, 700f, 20f, 760f, 170f, "2. 再加载这个 Edge Blur 模板\n它会使用 linear_depth_buffer.fx 的结果生成边缘遮罩\n只在轮廓附近混入模糊颜色。", 0.24f, 0.50f, 0.38f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 80, 240);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 300, 120);
        var linearDepthCenter = graph.AddNode(NodeKind.LayerSource, 300, 320);
        linearDepthCenter.Properties["BufferName"] = "LinearDepthEdgeRT";
        linearDepthCenter.Properties["EffectFile"] = "linear_depth_buffer.fx";
        linearDepthCenter.Properties["Description"] = "Linear depth for edge blur";
        linearDepthCenter.Properties["Format"] = "A16B16G16R16F";
        linearDepthCenter.Properties["FilterMode"] = "Linear";

        var dirRight = graph.AddNode(NodeKind.Float4Value, 300, 500);
        dirRight.Properties["X"] = "1.0";
        dirRight.Properties["Y"] = "0.0";
        dirRight.Properties["Z"] = "0.0";
        dirRight.Properties["W"] = "0.0";

        var dirUp = graph.AddNode(NodeKind.Float4Value, 300, 620);
        dirUp.Properties["X"] = "0.0";
        dirUp.Properties["Y"] = "1.0";
        dirUp.Properties["Z"] = "0.0";
        dirUp.Properties["W"] = "0.0";

        var widthScalar = graph.AddNode(NodeKind.Scalar, 300, 760);
        widthScalar.Properties["Value"] = "1.0";

        var offsetRightUv = graph.AddNode(NodeKind.ScreenUvOffset, 620, 520);
        offsetRightUv.Properties["Scale"] = "0.004";
        offsetRightUv.Properties["DepthPower"] = "0.75";
        offsetRightUv.Properties["SizeX"] = "1.0";
        offsetRightUv.Properties["SizeY"] = "1.0";

        var offsetUpUv = graph.AddNode(NodeKind.ScreenUvOffset, 620, 680);
        offsetUpUv.Properties["Scale"] = "0.004";
        offsetUpUv.Properties["DepthPower"] = "0.75";
        offsetUpUv.Properties["SizeX"] = "1.0";
        offsetUpUv.Properties["SizeY"] = "1.0";

        var linearDepthRight = graph.AddNode(NodeKind.OffscreenBufferSample, 940, 520);
        linearDepthRight.Properties["BufferName"] = "LinearDepthEdgeRT";
        linearDepthRight.Properties["Description"] = "Linear depth for edge blur";
        linearDepthRight.Properties["Format"] = "A16B16G16R16F";
        linearDepthRight.Properties["FilterMode"] = "Linear";

        var linearDepthUp = graph.AddNode(NodeKind.OffscreenBufferSample, 940, 680);
        linearDepthUp.Properties["BufferName"] = "LinearDepthEdgeRT";
        linearDepthUp.Properties["Description"] = "Linear depth for edge blur";
        linearDepthUp.Properties["Format"] = "A16B16G16R16F";
        linearDepthUp.Properties["FilterMode"] = "Linear";

        var edgeRight = graph.AddNode(NodeKind.DepthEdgeDetect, 1240, 520);
        edgeRight.Properties["DepthChannel"] = "R";
        edgeRight.Properties["EdgeMode"] = "AnyDifference";
        edgeRight.Properties["Scale"] = "250.0";
        edgeRight.Properties["Threshold"] = "0.0005";
        edgeRight.Properties["Power"] = "1.0";

        var edgeUp = graph.AddNode(NodeKind.DepthEdgeDetect, 1240, 680);
        edgeUp.Properties["DepthChannel"] = "R";
        edgeUp.Properties["EdgeMode"] = "AnyDifference";
        edgeUp.Properties["Scale"] = "250.0";
        edgeUp.Properties["Threshold"] = "0.0005";
        edgeUp.Properties["Power"] = "1.0";

        var edgeMask = graph.AddNode(NodeKind.Max, 1540, 600);
        edgeMask.Properties["Type"] = "Float1";

        var blurHorizontal = graph.AddNode(NodeKind.GaussianBlur, 620, 120);
        blurHorizontal.Properties["BlurSource"] = "SceneColor";
        blurHorizontal.Properties["BlurDirection"] = "Horizontal";
        blurHorizontal.Properties["StepScale"] = "1.0";
        blurHorizontal.Properties["Strength"] = "1.0";
        blurHorizontal.Properties["PreserveAlpha"] = "True";

        var blurVertical = graph.AddNode(NodeKind.GaussianBlur, 620, 260);
        blurVertical.Properties["BlurSource"] = "SceneColor";
        blurVertical.Properties["BlurDirection"] = "Vertical";
        blurVertical.Properties["StepScale"] = "1.0";
        blurVertical.Properties["Strength"] = "1.0";
        blurVertical.Properties["PreserveAlpha"] = "True";

        var blurMixFactor = graph.AddNode(NodeKind.Scalar, 920, 260);
        blurMixFactor.Properties["Value"] = "0.5";

        var blurMix = graph.AddNode(NodeKind.Lerp, 1240, 180);
        blurMix.Properties["Type"] = "Float4";

        var blurOpacity = graph.AddNode(NodeKind.Scalar, 1820, 700);
        blurOpacity.Properties["Value"] = "0.75";

        var scaledMask = graph.AddNode(NodeKind.Multiply, 1820, 560);
        scaledMask.Properties["Type"] = "Float1";

        var finalBlend = graph.AddNode(NodeKind.Lerp, 1820, 220);
        finalBlend.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 2140, 220);

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        // Use the exported linear depth buffer instead of raw SceneDepth so edge detection stays readable in MME.
        Connect(graph, screenUv, "UV", linearDepthCenter, "UV");
        Connect(graph, screenUv, "UV", blurHorizontal, "UV");
        Connect(graph, screenUv, "UV", blurVertical, "UV");
        Connect(graph, screenUv, "UV", offsetRightUv, "UV");
        Connect(graph, screenUv, "UV", offsetUpUv, "UV");

        Connect(graph, linearDepthCenter, "Color", offsetRightUv, "Depth");
        Connect(graph, linearDepthCenter, "Color", offsetUpUv, "Depth");
        Connect(graph, dirRight, "Value", offsetRightUv, "Direction");
        Connect(graph, dirUp, "Value", offsetUpUv, "Direction");
        Connect(graph, widthScalar, "Value", offsetRightUv, "Width");
        Connect(graph, widthScalar, "Value", offsetUpUv, "Width");

        Connect(graph, offsetRightUv, "UV", linearDepthRight, "UV");
        Connect(graph, offsetUpUv, "UV", linearDepthUp, "UV");

        Connect(graph, linearDepthCenter, "Color", edgeRight, "CenterDepth");
        Connect(graph, linearDepthRight, "Color", edgeRight, "OffsetDepth");
        Connect(graph, linearDepthCenter, "Color", edgeUp, "CenterDepth");
        Connect(graph, linearDepthUp, "Color", edgeUp, "OffsetDepth");

        Connect(graph, edgeRight, "Edge", edgeMask, "A");
        Connect(graph, edgeUp, "Edge", edgeMask, "B");

        Connect(graph, blurHorizontal, "Color", blurMix, "A");
        Connect(graph, blurVertical, "Color", blurMix, "B");
        Connect(graph, blurMixFactor, "Value", blurMix, "T");

        Connect(graph, edgeMask, "Result", scaledMask, "A");
        Connect(graph, blurOpacity, "Value", scaledMask, "B");

        Connect(graph, sceneColor, "Color", finalBlend, "A");
        Connect(graph, blurMix, "Result", finalBlend, "B");
        Connect(graph, scaledMask, "Result", finalBlend, "T");

        Connect(graph, finalBlend, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateBloomPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 120, 120);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 420, 80);

        var brightExtract = graph.AddNode(NodeKind.BrightExtract, 760, 80);
        brightExtract.Properties["Threshold"] = "0.18";
        brightExtract.Properties["SoftKnee"] = "0.35";
        brightExtract.Properties["Intensity"] = "6.0";
        brightExtract.Properties["PreserveAlpha"] = "False";

        var downsample = graph.AddNode(NodeKind.Downsample, 760, 280);
        downsample.Properties["Source"] = "OffscreenBuffer";
        downsample.Properties["BufferName"] = "BloomBright";
        downsample.Properties["Description"] = "Bloom bright downsample";
        downsample.Properties["Format"] = "A16B16G16R16F";
        downsample.Properties["FilterMode"] = "Linear";
        downsample.Properties["StepScale"] = "1.5";
        downsample.Properties["PreserveAlpha"] = "False";

        var bloomHigh = graph.AddNode(NodeKind.OffscreenBufferSample, 1120, 120);
        bloomHigh.Properties["BufferName"] = "BloomBright";
        bloomHigh.Properties["Description"] = "Bloom bright extract";
        bloomHigh.Properties["Format"] = "A16B16G16R16F";
        bloomHigh.Properties["FilterMode"] = "Linear";

        var bloomLowBlur = graph.AddNode(NodeKind.BilateralBlur, 1120, 320);
        bloomLowBlur.Properties["Source"] = "OffscreenBuffer";
        bloomLowBlur.Properties["BufferName"] = "BloomBlur";
        bloomLowBlur.Properties["Description"] = "Bloom quarter blur";
        bloomLowBlur.Properties["Format"] = "A16B16G16R16F";
        bloomLowBlur.Properties["FilterMode"] = "Linear";
        bloomLowBlur.Properties["NormalBufferName"] = string.Empty;
        bloomLowBlur.Properties["StepScale"] = "5.0";
        bloomLowBlur.Properties["Strength"] = "1.2";
        bloomLowBlur.Properties["DepthThreshold"] = "0.02";
        bloomLowBlur.Properties["NormalThreshold"] = "0.15";
        bloomLowBlur.Properties["PreserveAlpha"] = "False";

        var upsampleFactor = graph.AddNode(NodeKind.Scalar, 1120, 520);
        upsampleFactor.Properties["Value"] = "0.85";

        var upsampleBlend = graph.AddNode(NodeKind.UpsampleBlend, 1460, 220);
        upsampleBlend.Properties["Factor"] = "0.85";
        upsampleBlend.Properties["PreserveAlpha"] = "False";

        var opacity = graph.AddNode(NodeKind.Scalar, 1460, 520);
        opacity.Properties["Value"] = "1.0";

        var blend = graph.AddNode(NodeKind.LayerBlend, 1820, 180);
        blend.Properties["LayerMode"] = "Add";
        blend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 2180, 220);
        output.Properties["Pipeline"] = "ScenePostProcess";
        output.Properties["RenderTarget0Name"] = "BloomBright";
        output.Properties["RenderTarget0Scale"] = "1.0";
        output.Properties["RenderTarget1Name"] = "BloomBlur";
        output.Properties["RenderTarget1Scale"] = "0.5";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = downsample.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = bloomHigh.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = bloomLowBlur.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = brightExtract.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bloomHigh.Id,
            SourcePin = "Color",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "High",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bloomLowBlur.Id,
            SourcePin = "Color",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "Low",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = upsampleFactor.Id,
            SourcePin = "Value",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "Factor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = upsampleBlend.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacity.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = brightExtract.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "CaptureColor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = downsample.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "CaptureSecondaryColor",
        });

        return graph;
    }

    private static NodeGraph CreateBloomDirtMapPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 120, 120);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 420, 80);

        var brightExtract = graph.AddNode(NodeKind.BrightExtract, 760, 80);
        brightExtract.Properties["Threshold"] = "0.3";
        brightExtract.Properties["SoftKnee"] = "0.25";
        brightExtract.Properties["Intensity"] = "4.0";
        brightExtract.Properties["PreserveAlpha"] = "False";

        var downsample = graph.AddNode(NodeKind.Downsample, 760, 280);
        downsample.Properties["Source"] = "OffscreenBuffer";
        downsample.Properties["BufferName"] = "BloomBright";
        downsample.Properties["Description"] = "Bloom bright downsample";
        downsample.Properties["Format"] = "A16B16G16R16F";
        downsample.Properties["FilterMode"] = "Linear";
        downsample.Properties["StepScale"] = "2.0";
        downsample.Properties["PreserveAlpha"] = "False";

        var bloomHigh = graph.AddNode(NodeKind.OffscreenBufferSample, 1120, 120);
        bloomHigh.Properties["BufferName"] = "BloomBright";
        bloomHigh.Properties["Description"] = "Bloom bright extract";
        bloomHigh.Properties["Format"] = "A16B16G16R16F";
        bloomHigh.Properties["FilterMode"] = "Linear";

        var bloomLowBlur = graph.AddNode(NodeKind.BilateralBlur, 1120, 320);
        bloomLowBlur.Properties["Source"] = "OffscreenBuffer";
        bloomLowBlur.Properties["BufferName"] = "BloomBlur";
        bloomLowBlur.Properties["Description"] = "Bloom quarter blur";
        bloomLowBlur.Properties["Format"] = "A16B16G16R16F";
        bloomLowBlur.Properties["FilterMode"] = "Linear";
        bloomLowBlur.Properties["NormalBufferName"] = string.Empty;
        bloomLowBlur.Properties["StepScale"] = "4.0";
        bloomLowBlur.Properties["Strength"] = "1.0";
        bloomLowBlur.Properties["DepthThreshold"] = "0.01";
        bloomLowBlur.Properties["NormalThreshold"] = "0.15";
        bloomLowBlur.Properties["PreserveAlpha"] = "False";

        var upsampleFactor = graph.AddNode(NodeKind.Scalar, 1460, 120);
        upsampleFactor.Properties["Value"] = "0.65";

        var upsampleBlend = graph.AddNode(NodeKind.UpsampleBlend, 1460, 300);
        upsampleBlend.Properties["Factor"] = "0.65";
        upsampleBlend.Properties["PreserveAlpha"] = "False";

        var dirtMap = graph.AddNode(NodeKind.ExternalTexture, 1460, 520);
        dirtMap.Properties["ResourceName"] = "Textures/DirtMaskTextureExample.png";
        dirtMap.Properties["TextureMode"] = "Static";
        dirtMap.Properties["AddressMode"] = "Clamp";
        dirtMap.Properties["FilterMode"] = "Linear";
        dirtMap.Properties["ColorSpace"] = "Color";

        var dirtMul = graph.AddNode(NodeKind.Multiply, 1820, 460);
        dirtMul.Properties["Type"] = "Float4";

        var dirtIntensity = graph.AddNode(NodeKind.Scalar, 1820, 620);
        dirtIntensity.Properties["Value"] = "8.0";

        var dirtBlend = graph.AddNode(NodeKind.LayerBlend, 2180, 400);
        dirtBlend.Properties["LayerMode"] = "Add";
        dirtBlend.Properties["MaskInvert"] = "False";

        var opacity = graph.AddNode(NodeKind.Scalar, 2180, 760);
        opacity.Properties["Value"] = "1.0";

        var blend = graph.AddNode(NodeKind.LayerBlend, 2540, 180);
        blend.Properties["LayerMode"] = "Add";
        blend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 2900, 220);
        output.Properties["Pipeline"] = "ScenePostProcess";
        output.Properties["RenderTarget0Name"] = "BloomBright";
        output.Properties["RenderTarget0Scale"] = "0.5";
        output.Properties["RenderTarget1Name"] = "BloomBlur";
        output.Properties["RenderTarget1Scale"] = "0.25";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = downsample.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = bloomHigh.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = bloomLowBlur.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = dirtMap.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = brightExtract.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bloomHigh.Id,
            SourcePin = "Color",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "High",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bloomLowBlur.Id,
            SourcePin = "Color",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "Low",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = upsampleFactor.Id,
            SourcePin = "Value",
            TargetNodeId = upsampleBlend.Id,
            TargetPin = "Factor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = upsampleBlend.Id,
            SourcePin = "Color",
            TargetNodeId = dirtMul.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = dirtMap.Id,
            SourcePin = "Color",
            TargetNodeId = dirtMul.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = upsampleBlend.Id,
            SourcePin = "Color",
            TargetNodeId = dirtBlend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = dirtMul.Id,
            SourcePin = "Result",
            TargetNodeId = dirtBlend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = dirtIntensity.Id,
            SourcePin = "Value",
            TargetNodeId = dirtBlend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = dirtBlend.Id,
            SourcePin = "Result",
            TargetNodeId = blend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacity.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = brightExtract.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "CaptureColor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = downsample.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "CaptureSecondaryColor",
        });

        return graph;
    }

    public static NodeGraph CreateLayerSourceOutputSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 340, 120);
        materialTexture.Properties["ColorSpace"] = "Color";

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 640, 160);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 940, 160);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = layerOutput.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = layerOutput.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerOutput.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateLayerSourceMaterialSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 340, 120);
        materialTexture.Properties["ColorSpace"] = "Color";

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 640, 160);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 940, 160);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = layerOutput.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = layerOutput.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerOutput.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateLocalLightCaptureSample()
    {
        return CreateLocalLightCaptureSampleCore();
    }

    private static NodeGraph CreateLocalLightCaptureSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 140);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 320, 140);
        materialTexture.Properties["ColorSpace"] = "Color";
        var localLight = graph.AddNode(NodeKind.VirtualLight, 640, 160);
        localLight.Properties["Name"] = "(OffscreenOwner)";
        localLight.Properties["PositionItem"] = "LightPos";
        localLight.Properties["RItem"] = "R";
        localLight.Properties["GItem"] = "G";
        localLight.Properties["BItem"] = "B";
        localLight.Properties["IntensityItem"] = "Intensity";
        localLight.Properties["RangeItem"] = "Range";
        localLight.Properties["SoftnessItem"] = "Softness";
        localLight.Properties["SpecWeightItem"] = "SpecWeight";
        localLight.Properties["RimWeightItem"] = "RimWeight";

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 960, 180);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 1260, 180);
        output.Properties["AlphaMode"] = "ColorAlpha";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = localLight.Id,
            TargetPin = "BaseColor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localLight.Id,
            SourcePin = "Color",
            TargetNodeId = layerOutput.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = layerOutput.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerOutput.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateLocalLightComposeSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 120);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 360, 90);
        var layerSource = graph.AddNode(NodeKind.LayerSource, 360, 290);
        layerSource.Properties["BufferName"] = "VirtualLightRT0";
        layerSource.Properties["EffectFile"] = "virtual_light_capture.fx";
        layerSource.Properties["EffectBinding"] = BuildLocalLightEffectBinding();
        layerSource.Properties["Description"] = "Virtual light owner capture";
        layerSource.Properties["Format"] = "A16B16G16R16F";
        layerSource.Properties["FilterMode"] = "Linear";

        var opacity = graph.AddNode(NodeKind.Scalar, 680, 460);
        opacity.Properties["Value"] = "1.0";

        var layerBlend = graph.AddNode(NodeKind.LayerBlend, 720, 210);
        layerBlend.Properties["LayerMode"] = "Add";
        layerBlend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1040, 220);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = layerSource.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerSource.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacity.Id,
            SourcePin = "Value",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerBlend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateLocalLightOwnerSample()
    {
        return CreateLocalLightComposeSample();
    }

    public static NodeGraph CreateVirtualDirectionalShadowWorkflowSample()
    {
        return CreateVirtualDirectionalShadowWorkflowSampleCore();
    }

    private static NodeGraph CreateVirtualDirectionalShadowWorkflowSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 40f, 20f, 520f, 180f, "1. 导出整套运行文件\n文件 > 导出 > 导出虚拟定向阴影半兰伯特 Bundle\n导出时会自动生成半兰伯特版 VirtualDirectionalShadow_Object.fxsub，并附带 LightController.pmx 与中文说明。", 0.26f, 0.42f, 0.72f);
        AddFrame(graph, 600f, 20f, 620f, 180f, "2. 运行时文件\nLightController.pmx + virtual_directional_shadow.fx + VirtualDirectionalShadow_*.fxsub\n其中 Object.fxsub 负责半兰伯特受光，ShadowMap/WPos/合成负责阴影。\n主 FX 顶部可通过 VDS_QUALITY_PRESET 切换质量档。", 0.22f, 0.50f, 0.38f);
        AddFrame(graph, 40f, 240f, 1460f, 360f, "3. 场景工作流说明\n旋转控制器里的 Direction 骨骼来改变虚拟灯方向。\nLightIntensity / ShadowExtent / ShadowDepth / DebugShadow 由控制器形变驱动。\n下面四个缓冲节点只用于检查贡献图、近景阴影图、远景阴影图和世界坐标缓冲。", 0.52f, 0.36f, 0.18f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 360);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 340, 320);
        var directionalDraw = graph.AddNode(NodeKind.OffscreenBufferSample, 340, 470);
        directionalDraw.Properties["BufferName"] = "VirtualDirLightDraw";
        directionalDraw.Properties["Description"] = "Virtual directional light contribution buffer";
        directionalDraw.Properties["Format"] = "A16B16G16R16F";
        directionalDraw.Properties["FilterMode"] = "Linear";
        directionalDraw.Properties["MipLevels"] = "1";

        var shadowMap = graph.AddNode(NodeKind.OffscreenBufferSample, 620, 470);
        shadowMap.Properties["BufferName"] = "VirtualDirLightShadowMap";
        shadowMap.Properties["Description"] = "Virtual directional light shadow map";
        shadowMap.Properties["Format"] = "D3DFMT_R32F";
        shadowMap.Properties["FilterMode"] = "Point";
        shadowMap.Properties["MipLevels"] = "1";

        var shadowMapFar = graph.AddNode(NodeKind.OffscreenBufferSample, 900, 470);
        shadowMapFar.Properties["BufferName"] = "VirtualDirLightShadowMapFar";
        shadowMapFar.Properties["Description"] = "Virtual directional light far shadow map";
        shadowMapFar.Properties["Format"] = "D3DFMT_R32F";
        shadowMapFar.Properties["FilterMode"] = "Point";
        shadowMapFar.Properties["MipLevels"] = "1";

        var worldPos = graph.AddNode(NodeKind.OffscreenBufferSample, 1180, 470);
        worldPos.Properties["BufferName"] = "VirtualDirLightWPos";
        worldPos.Properties["Description"] = "Virtual directional light world-position buffer";
        worldPos.Properties["Format"] = "A32B32G32R32F";
        worldPos.Properties["FilterMode"] = "Linear";
        worldPos.Properties["MipLevels"] = "1";

        var output = graph.AddNode(NodeKind.Output, 960, 320);
        output.Properties["TemplateProfile"] = "VirtualDirectionalShadow";
        output.Properties["VdsQualityPreset"] = "VDS_QUALITY_HIGH";
        output.Properties["VdsAnchorObjectName"] = "TargetModel.pmx";
        output.Properties["VdsAnchorBoneName"] = "センター";
        output.Properties["VdsNearExtentScale"] = "0.75";
        output.Properties["VdsFarExtentScale"] = "2.75";
        output.Properties["VdsNearDepthScale"] = "0.85";
        output.Properties["VdsFarDepthScale"] = "3.50";
        output.Properties["VdsBlendStart"] = "0.55";
        output.Properties["VdsBlendEnd"] = "0.90";

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, screenUv, "UV", directionalDraw, "UV");
        Connect(graph, screenUv, "UV", shadowMap, "UV");
        Connect(graph, screenUv, "UV", shadowMapFar, "UV");
        Connect(graph, screenUv, "UV", worldPos, "UV");
        Connect(graph, sceneColor, "Color", output, "Color");

        return graph;
    }

    private static GraphNode AddFrame(NodeGraph graph, float x, float y, float width, float height, string title, float r, float g, float b)
    {
        var frame = graph.AddNode(NodeKind.Frame, x, y);
        frame.Properties["Title"] = title;
        frame.Properties["Width"] = width.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["Height"] = height.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintR"] = r.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintG"] = g.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintB"] = b.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["Opacity"] = "0.18";
        return frame;
    }

    private static string BuildLocalLightEffectBinding()
    {
        return "self = hide; virtual_light.pmx = hide; * = virtual_light_capture.fx;";
    }

    public static NodeGraph CreateLayerBlendWorkflowSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 120);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 360, 90);
        var layerSource = graph.AddNode(NodeKind.LayerSource, 360, 280);
        layerSource.Properties["BufferName"] = "LayerRT0";
        layerSource.Properties["EffectFile"] = "layer_source.fx";
        layerSource.Properties["Description"] = "Layer capture";
        layerSource.Properties["Format"] = "A16B16G16R16F";
        layerSource.Properties["FilterMode"] = "Linear";

        var opacity = graph.AddNode(NodeKind.Scalar, 660, 420);
        opacity.Properties["Value"] = "1.0";

        var layerBlend = graph.AddNode(NodeKind.LayerBlend, 700, 200);
        layerBlend.Properties["LayerMode"] = "Screen";
        layerBlend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1020, 210);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = layerSource.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerSource.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacity.Id,
            SourcePin = "Value",
            TargetNodeId = layerBlend.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerBlend.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateDualLayerBlendWorkflowSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 80, 170);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 320, 80);

        var layerSourceA = graph.AddNode(NodeKind.LayerSource, 320, 250);
        layerSourceA.Properties["BufferName"] = "LayerRTA";
        layerSourceA.Properties["EffectFile"] = "layer_a.fx";
        layerSourceA.Properties["Description"] = "Layer A capture";
        layerSourceA.Properties["Format"] = "A16B16G16R16F";
        layerSourceA.Properties["FilterMode"] = "Linear";

        var layerSourceB = graph.AddNode(NodeKind.LayerSource, 320, 430);
        layerSourceB.Properties["BufferName"] = "LayerRTB";
        layerSourceB.Properties["EffectFile"] = "layer_b.fx";
        layerSourceB.Properties["Description"] = "Layer B capture";
        layerSourceB.Properties["Format"] = "A16B16G16R16F";
        layerSourceB.Properties["FilterMode"] = "Linear";

        var opacityA = graph.AddNode(NodeKind.Scalar, 620, 330);
        opacityA.Properties["Value"] = "1.0";
        var opacityB = graph.AddNode(NodeKind.Scalar, 920, 500);
        opacityB.Properties["Value"] = "0.75";

        var layerBlendA = graph.AddNode(NodeKind.LayerBlend, 700, 210);
        layerBlendA.Properties["LayerMode"] = "Screen";
        layerBlendA.Properties["MaskInvert"] = "False";

        var layerBlendB = graph.AddNode(NodeKind.LayerBlend, 1020, 320);
        layerBlendB.Properties["LayerMode"] = "Add";
        layerBlendB.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1320, 320);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = sceneColor.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = layerSourceA.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = layerSourceB.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sceneColor.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlendA.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerSourceA.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlendA.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacityA.Id,
            SourcePin = "Value",
            TargetNodeId = layerBlendA.Id,
            TargetPin = "Opacity",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerBlendA.Id,
            SourcePin = "Result",
            TargetNodeId = layerBlendB.Id,
            TargetPin = "Background",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerSourceB.Id,
            SourcePin = "Color",
            TargetNodeId = layerBlendB.Id,
            TargetPin = "Foreground",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = opacityB.Id,
            SourcePin = "Value",
            TargetNodeId = layerBlendB.Id,
            TargetPin = "Opacity",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = layerBlendB.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateOffsetShadowComposeSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 40f, 20f, 700f, 180f, "1. 先导出物体模板里的 Offset Shadow Capture\n建议文件名：offset_shadow_capture.fx", 0.26f, 0.42f, 0.72f);
        AddFrame(graph, 780f, 20f, 700f, 180f, "2. 再导出 Offset Shadow Mask\n建议文件名：offset_shadow_mask_white.fx", 0.24f, 0.50f, 0.38f);
        AddFrame(graph, 1520f, 20f, 720f, 180f, "3. 最后导出这个合成模板\n再用 PostProcessCarrier.x 挂到场景后处理", 0.56f, 0.36f, 0.30f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 320);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 380, 250);

        var shadowLayer = graph.AddNode(NodeKind.LayerSource, 380, 480);
        shadowLayer.Properties["BufferName"] = "OffsetShadowRT";
        shadowLayer.Properties["EffectBinding"] = "self = hide; * = offset_shadow_capture.fx;";
        shadowLayer.Properties["Description"] = "Offset shadow capture";
        shadowLayer.Properties["Format"] = "A16B16G16R16F";
        shadowLayer.Properties["FilterMode"] = "Linear";

        var maskLayer = graph.AddNode(NodeKind.LayerSource, 380, 760);
        maskLayer.Properties["BufferName"] = "OffsetShadowMaskRT";
        maskLayer.Properties["EffectBinding"] = "self = hide; * = offset_shadow_mask_white.fx;";
        maskLayer.Properties["Description"] = "Offset shadow exclusion mask";
        maskLayer.Properties["Format"] = "A16B16G16R16F";
        maskLayer.Properties["FilterMode"] = "Linear";

        var maskSplit = graph.AddNode(NodeKind.SplitColor, 700, 760);
        var opacity = graph.AddNode(NodeKind.ControlObjectValue, 720, 500);
        opacity.Properties["Name"] = "(self)";
        opacity.Properties["Item"] = "Tr";

        var layerBlend = graph.AddNode(NodeKind.LayerBlend, 1040, 430);
        layerBlend.Properties["LayerMode"] = "Darken";
        layerBlend.Properties["MaskInvert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1360, 430);
        output.Properties["AlphaMode"] = "ColorAlpha";

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, screenUv, "UV", shadowLayer, "UV");
        Connect(graph, screenUv, "UV", maskLayer, "UV");
        Connect(graph, sceneColor, "Color", layerBlend, "Background");
        Connect(graph, shadowLayer, "Color", layerBlend, "Foreground");
        Connect(graph, opacity, "Value", layerBlend, "Opacity");
        Connect(graph, maskLayer, "Color", maskSplit, "Color");
        Connect(graph, maskSplit, "A", layerBlend, "Mask");
        Connect(graph, layerBlend, "Result", output, "Color");

        return graph;
    }

    public static NodeGraph CreateMaskBufferDebugPostProcessSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 120, 90);
        var maskBuffer = graph.AddNode(NodeKind.OffscreenBufferSample, 420, 140);
        maskBuffer.Properties["BufferName"] = "Na_RimMask";
        maskBuffer.Properties["Description"] = "Mask buffer";
        maskBuffer.Properties["Format"] = "D3DFMT_A8R8G8B8";
        maskBuffer.Properties["FilterMode"] = "Linear";

        var debug = graph.AddNode(NodeKind.MaskBufferDebug, 760, 160);
        debug.Properties["DisplayMode"] = "RGB";

        var output = graph.AddNode(NodeKind.Output, 1080, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = screenUv.Id,
            SourcePin = "UV",
            TargetNodeId = maskBuffer.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = maskBuffer.Id,
            SourcePin = "Color",
            TargetNodeId = debug.Id,
            TargetPin = "Buffer",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = debug.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }
}

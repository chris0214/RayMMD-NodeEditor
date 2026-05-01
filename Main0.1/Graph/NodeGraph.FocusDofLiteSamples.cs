namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateFocusDofLiteDepthBufferSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        AddFrame(
            graph,
            40f,
            20f,
            620f,
            180f,
            "Focus DOF Lite 深度捕捉\n导出为 focus_dof_lite_depth.fx 或工作流包自带的 *_depth.fx。\n主合成会读取红通道作为线性深度，DepthRange 用来控制景深距离范围。",
            0.30f,
            0.44f,
            0.72f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 140);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        materialTexture.Properties["ColorSpace"] = "Color";
        materialTexture.Properties["FilterMode"] = "Point";

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 80, 520);
        var splitMaterial = graph.AddNode(NodeKind.SplitColor, 360, 520);

        var worldPosition = graph.AddNode(NodeKind.WorldPosition, 80, 300);
        var cameraPosition = graph.AddNode(NodeKind.CameraPosition, 80, 400);
        var distance = graph.AddNode(NodeKind.Distance, 360, 340);

        var depthRange = graph.AddNode(NodeKind.SharedParameter, 660, 200);
        depthRange.Properties["Name"] = "FocusDofLiteDepthRange";
        depthRange.Properties["DefaultValue"] = "100.0";

        var divide = graph.AddNode(NodeKind.Divide, 660, 340);
        divide.Properties["Type"] = "Float1";

        var clamp = graph.AddNode(NodeKind.Clamp, 940, 340);
        clamp.Properties["Type"] = "Float1";
        clamp.Properties["Min"] = "0.0";
        clamp.Properties["Max"] = "1.0";

        var compose = graph.AddNode(NodeKind.ComposeColor, 1220, 260);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 1220, 520);
        alphaMultiply.Properties["Type"] = "Float1";

        var output = graph.AddNode(NodeKind.Output, 1500, 260);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.01";
        output.Properties["ShadowTechniqueMode"] = "Empty";
        output.Properties["ZplotTechniqueMode"] = "Empty";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialDiffuse, "Value", splitMaterial, "Color");
        Connect(graph, worldPosition, "Value", distance, "A");
        Connect(graph, cameraPosition, "Value", distance, "B");
        Connect(graph, distance, "Result", divide, "A");
        Connect(graph, depthRange, "Value", divide, "B");
        Connect(graph, divide, "Result", clamp, "Value");
        Connect(graph, clamp, "Result", compose, "R");
        Connect(graph, clamp, "Result", compose, "G");
        Connect(graph, clamp, "Result", compose, "B");
        Connect(graph, materialTexture, "A", alphaMultiply, "A");
        Connect(graph, splitMaterial, "A", alphaMultiply, "B");
        Connect(graph, alphaMultiply, "Result", compose, "A");
        Connect(graph, compose, "Color", output, "Color");
        Connect(graph, alphaMultiply, "Result", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateFocusDofLiteComposeSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };
        const string controllerName = "DofController.pmx";

        AddFrame(
            graph,
            40f,
            20f,
            720f,
            190f,
            "Focus DOF Lite 主合成\n先导出深度捕捉，再把 PostProcessCarrier.x 挂上 *_main.fx。\n景深参数默认读取 DofController.pmx 的 morph，DepthRange 仍保留为效果面板参数。",
            0.28f,
            0.44f,
            0.30f);
        AddFrame(
            graph,
            820f,
            20f,
            760f,
            190f,
            "建议调参顺序\n1. 先调 DepthRange\n2. 再调控制器里的 FocusCenter / FocusRange\n3. 再调 FarBlur / NearBlur / FarStrength / NearStrength\n4. DebugView=1 查看远景(红) / 合焦(绿) / 前景(蓝)",
            0.34f,
            0.34f,
            0.62f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 80, 280);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 340, 260);

        var depthLayer = graph.AddNode(NodeKind.LayerSource, 340, 460);
        depthLayer.Properties["FocusDofLiteRole"] = "DepthLayer";
        depthLayer.Properties["BufferName"] = "FocusDofLiteDepthRT";
        depthLayer.Properties["EffectFile"] = "focus_dof_lite_depth.fx";
        depthLayer.Properties["EffectBinding"] = "self = hide; DofController.pmx = hide; *_main.x = hide; * = focus_dof_lite_depth.fx;";
        depthLayer.Properties["Description"] = "Focus DOF Lite linear depth";
        depthLayer.Properties["Format"] = "A16B16G16R16F";
        depthLayer.Properties["FilterMode"] = "Point";

        var depthMask = graph.AddNode(NodeKind.ComponentMask, 620, 460);
        depthMask.Properties["Channels"] = "R";

        var focusCenter = graph.AddNode(NodeKind.ControlObjectValue, 620, 700);
        focusCenter.Properties["Name"] = controllerName;
        focusCenter.Properties["Item"] = "FocusCenter";

        var focusRange = graph.AddNode(NodeKind.ControlObjectValue, 620, 820);
        focusRange.Properties["Name"] = controllerName;
        focusRange.Properties["Item"] = "FocusRange";

        var farBlurStep = graph.AddNode(NodeKind.ControlObjectValue, 900, 660);
        farBlurStep.Properties["Name"] = controllerName;
        farBlurStep.Properties["Item"] = "FarBlur";

        var nearBlurStep = graph.AddNode(NodeKind.ControlObjectValue, 900, 780);
        nearBlurStep.Properties["Name"] = controllerName;
        nearBlurStep.Properties["Item"] = "NearBlur";

        var farStrength = graph.AddNode(NodeKind.ControlObjectValue, 900, 900);
        farStrength.Properties["Name"] = controllerName;
        farStrength.Properties["Item"] = "FarStrength";

        var nearStrength = graph.AddNode(NodeKind.ControlObjectValue, 900, 1020);
        nearStrength.Properties["Name"] = controllerName;
        nearStrength.Properties["Item"] = "NearStrength";

        var debugView = graph.AddNode(NodeKind.ControlObjectValue, 900, 1140);
        debugView.Properties["Name"] = controllerName;
        debugView.Properties["Item"] = "DebugView";

        var one = graph.AddNode(NodeKind.Scalar, 1180, 700);
        one.Properties["Value"] = "1.0";
        var half = graph.AddNode(NodeKind.Scalar, 1180, 780);
        half.Properties["Value"] = "0.5";

        var focusCenterClamp = graph.AddNode(NodeKind.Clamp, 1440, 700);
        focusCenterClamp.Properties["Type"] = "Float1";
        focusCenterClamp.Properties["Min"] = "0.0";
        focusCenterClamp.Properties["Max"] = "1.0";

        var focusRangeClamp = graph.AddNode(NodeKind.Clamp, 1440, 820);
        focusRangeClamp.Properties["Type"] = "Float1";
        focusRangeClamp.Properties["Min"] = "0.0";
        focusRangeClamp.Properties["Max"] = "0.49";

        var farStart = graph.AddNode(NodeKind.Add, 1720, 660);
        farStart.Properties["Type"] = "Float1";
        var farStartClamp = graph.AddNode(NodeKind.Clamp, 1980, 660);
        farStartClamp.Properties["Type"] = "Float1";
        farStartClamp.Properties["Min"] = "0.0";
        farStartClamp.Properties["Max"] = "1.0";

        var nearCut = graph.AddNode(NodeKind.Subtract, 1720, 820);
        nearCut.Properties["Type"] = "Float1";
        var nearCutClamp = graph.AddNode(NodeKind.Clamp, 1980, 820);
        nearCutClamp.Properties["Type"] = "Float1";
        nearCutClamp.Properties["Min"] = "0.0";
        nearCutClamp.Properties["Max"] = "1.0";

        var invDepth = graph.AddNode(NodeKind.OneMinus, 900, 460);
        invDepth.Properties["Type"] = "Float1";
        var invNearCut = graph.AddNode(NodeKind.OneMinus, 2260, 820);
        invNearCut.Properties["Type"] = "Float1";

        var farMask = graph.AddNode(NodeKind.SmoothStep, 2260, 640);
        farMask.Properties["Min"] = "0.5";
        farMask.Properties["Max"] = "1.0";
        var nearMask = graph.AddNode(NodeKind.SmoothStep, 2540, 820);
        nearMask.Properties["Min"] = "0.5";
        nearMask.Properties["Max"] = "1.0";

        var farOpacity = graph.AddNode(NodeKind.Multiply, 2820, 640);
        farOpacity.Properties["Type"] = "Float1";
        var nearOpacity = graph.AddNode(NodeKind.Multiply, 2820, 820);
        nearOpacity.Properties["Type"] = "Float1";

        var farBlurScale = graph.AddNode(NodeKind.Remap, 1180, 140);
        farBlurScale.Properties["InMin"] = "0.0";
        farBlurScale.Properties["InMax"] = "1.0";
        farBlurScale.Properties["OutMin"] = "0.5";
        farBlurScale.Properties["OutMax"] = "4.0";
        farBlurScale.Properties["Mode"] = "Linear";
        farBlurScale.Properties["Clamp"] = "True";

        var nearBlurScale = graph.AddNode(NodeKind.Remap, 1180, 320);
        nearBlurScale.Properties["InMin"] = "0.0";
        nearBlurScale.Properties["InMax"] = "1.0";
        nearBlurScale.Properties["OutMin"] = "0.5";
        nearBlurScale.Properties["OutMax"] = "6.0";
        nearBlurScale.Properties["Mode"] = "Linear";
        nearBlurScale.Properties["Clamp"] = "True";

        var focusRangeScale = graph.AddNode(NodeKind.Remap, 1180, 820);
        focusRangeScale.Properties["InMin"] = "0.0";
        focusRangeScale.Properties["InMax"] = "1.0";
        focusRangeScale.Properties["OutMin"] = "0.0";
        focusRangeScale.Properties["OutMax"] = "0.25";
        focusRangeScale.Properties["Mode"] = "Linear";
        focusRangeScale.Properties["Clamp"] = "True";

        var farBlurH = graph.AddNode(NodeKind.GaussianBlur, 1440, 140);
        farBlurH.Properties["BlurSource"] = "SceneColor";
        farBlurH.Properties["BlurDirection"] = "Horizontal";
        farBlurH.Properties["Strength"] = "1.0";
        farBlurH.Properties["PreserveAlpha"] = "True";

        var farBlurV = graph.AddNode(NodeKind.GaussianBlur, 1440, 320);
        farBlurV.Properties["BlurSource"] = "SceneColor";
        farBlurV.Properties["BlurDirection"] = "Vertical";
        farBlurV.Properties["Strength"] = "1.0";
        farBlurV.Properties["PreserveAlpha"] = "True";

        var farBlurMix = graph.AddNode(NodeKind.Lerp, 1740, 220);
        farBlurMix.Properties["Type"] = "Float4";
        farBlurMix.Properties["T"] = "0.5";

        var nearBlurH = graph.AddNode(NodeKind.GaussianBlur, 2020, 140);
        nearBlurH.Properties["BlurSource"] = "SceneColor";
        nearBlurH.Properties["BlurDirection"] = "Horizontal";
        nearBlurH.Properties["Strength"] = "1.0";
        nearBlurH.Properties["PreserveAlpha"] = "True";

        var nearBlurV = graph.AddNode(NodeKind.GaussianBlur, 2020, 320);
        nearBlurV.Properties["BlurSource"] = "SceneColor";
        nearBlurV.Properties["BlurDirection"] = "Vertical";
        nearBlurV.Properties["Strength"] = "1.0";
        nearBlurV.Properties["PreserveAlpha"] = "True";

        var nearBlurMix = graph.AddNode(NodeKind.Lerp, 2300, 220);
        nearBlurMix.Properties["Type"] = "Float4";
        nearBlurMix.Properties["T"] = "0.5";

        var farComposite = graph.AddNode(NodeKind.Lerp, 3400, 220);
        farComposite.Properties["Type"] = "Float4";

        var finalComposite = graph.AddNode(NodeKind.Lerp, 3680, 220);
        finalComposite.Properties["Type"] = "Float4";

        var combinedMask = graph.AddNode(NodeKind.Max, 3400, 960);
        combinedMask.Properties["Type"] = "Float1";
        var focusBand = graph.AddNode(NodeKind.OneMinus, 3680, 960);
        focusBand.Properties["Type"] = "Float1";
        var debugColor = graph.AddNode(NodeKind.ComposeColor, 3960, 900);
        var debugSwitch = graph.AddNode(NodeKind.Step, 3960, 1040);
        debugSwitch.Properties["Edge"] = "0.5";
        var debugMix = graph.AddNode(NodeKind.Lerp, 4240, 220);
        debugMix.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 4520, 220);

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, screenUv, "UV", depthLayer, "UV");
        Connect(graph, depthLayer, "Color", depthMask, "Value");
        Connect(graph, depthMask, "Result", invDepth, "Value");

        Connect(graph, focusCenter, "Value", focusCenterClamp, "Value");
        Connect(graph, focusRange, "Value", focusRangeScale, "Value");
        Connect(graph, focusRangeScale, "Result", focusRangeClamp, "Value");
        Connect(graph, focusCenterClamp, "Result", farStart, "A");
        Connect(graph, focusRangeClamp, "Result", farStart, "B");
        Connect(graph, farStart, "Result", farStartClamp, "Value");
        Connect(graph, focusCenterClamp, "Result", nearCut, "A");
        Connect(graph, focusRangeClamp, "Result", nearCut, "B");
        Connect(graph, nearCut, "Result", nearCutClamp, "Value");
        Connect(graph, nearCutClamp, "Result", invNearCut, "Value");

        Connect(graph, farStartClamp, "Result", farMask, "Min");
        Connect(graph, one, "Value", farMask, "Max");
        Connect(graph, depthMask, "Result", farMask, "X");

        Connect(graph, invNearCut, "Result", nearMask, "Min");
        Connect(graph, one, "Value", nearMask, "Max");
        Connect(graph, invDepth, "Result", nearMask, "X");

        Connect(graph, farMask, "Result", farOpacity, "A");
        Connect(graph, farStrength, "Value", farOpacity, "B");
        Connect(graph, nearMask, "Result", nearOpacity, "A");
        Connect(graph, nearStrength, "Value", nearOpacity, "B");

        Connect(graph, screenUv, "UV", farBlurH, "UV");
        Connect(graph, screenUv, "UV", farBlurV, "UV");
        Connect(graph, farBlurStep, "Value", farBlurScale, "Value");
        Connect(graph, farBlurScale, "Result", farBlurH, "StepScale");
        Connect(graph, farBlurScale, "Result", farBlurV, "StepScale");
        Connect(graph, farBlurH, "Color", farBlurMix, "A");
        Connect(graph, farBlurV, "Color", farBlurMix, "B");
        Connect(graph, half, "Value", farBlurMix, "T");

        Connect(graph, screenUv, "UV", nearBlurH, "UV");
        Connect(graph, screenUv, "UV", nearBlurV, "UV");
        Connect(graph, nearBlurStep, "Value", nearBlurScale, "Value");
        Connect(graph, nearBlurScale, "Result", nearBlurH, "StepScale");
        Connect(graph, nearBlurScale, "Result", nearBlurV, "StepScale");
        Connect(graph, nearBlurH, "Color", nearBlurMix, "A");
        Connect(graph, nearBlurV, "Color", nearBlurMix, "B");
        Connect(graph, half, "Value", nearBlurMix, "T");

        Connect(graph, sceneColor, "Color", farComposite, "A");
        Connect(graph, farBlurMix, "Result", farComposite, "B");
        Connect(graph, farOpacity, "Result", farComposite, "T");

        Connect(graph, farComposite, "Result", finalComposite, "A");
        Connect(graph, nearBlurMix, "Result", finalComposite, "B");
        Connect(graph, nearOpacity, "Result", finalComposite, "T");

        Connect(graph, farMask, "Result", combinedMask, "A");
        Connect(graph, nearMask, "Result", combinedMask, "B");
        Connect(graph, combinedMask, "Result", focusBand, "Value");

        Connect(graph, farMask, "Result", debugColor, "R");
        Connect(graph, focusBand, "Result", debugColor, "G");
        Connect(graph, nearMask, "Result", debugColor, "B");
        Connect(graph, one, "Value", debugColor, "A");
        Connect(graph, half, "Value", debugSwitch, "Edge");
        Connect(graph, debugView, "Value", debugSwitch, "X");

        Connect(graph, finalComposite, "Result", debugMix, "A");
        Connect(graph, debugColor, "Color", debugMix, "B");
        Connect(graph, debugSwitch, "Result", debugMix, "T");

        Connect(graph, debugMix, "Result", output, "Color");

        return graph;
    }
}

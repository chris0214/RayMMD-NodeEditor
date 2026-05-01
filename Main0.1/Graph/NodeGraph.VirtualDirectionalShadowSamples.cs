namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    public static NodeGraph CreateVirtualDirectionalShadowHalfLambertObjectSample()
    {
        return CreateVirtualDirectionalShadowHalfLambertObjectSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowMaskSample()
    {
        return CreateVirtualDirectionalShadowMaskSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowLambertBinaryShadowSample()
    {
        return CreateVirtualDirectionalShadowLambertBinaryShadowSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowSoftShadowPreviewSample()
    {
        return CreateVirtualDirectionalShadowSoftShadowPreviewSampleCore();
    }
    public static NodeGraph CreateVirtualDirectionalShadowLambertPreviewSample()
    {
        return CreateVirtualDirectionalShadowLambertPreviewSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowToonMaskMaterialSample()
    {
        return CreateVirtualDirectionalShadowToonMaskMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowToonLightMaterialSample()
    {
        return CreateVirtualDirectionalShadowToonLightMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowLambertMaterialSample()
    {
        return CreateVirtualDirectionalShadowLambertMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowHalfLambertMaterialSample()
    {
        return CreateVirtualDirectionalShadowHalfLambertMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowBlinnPhongMaterialSample()
    {
        return CreateVirtualDirectionalShadowBlinnPhongMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowDualToonLightMaterialSample()
    {
        return CreateVirtualDirectionalShadowDualToonLightMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowScreenShadowCaptureSample()
    {
        return CreateVirtualDirectionalShadowScreenShadowCaptureSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowScreenShadowPostProcessSample()
    {
        return CreateVirtualDirectionalShadowScreenShadowPostProcessSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowSoftToonLightMaterialSample()
    {
        return CreateVirtualDirectionalShadowSoftToonLightMaterialSampleCore();
    }

    public static NodeGraph CreateVirtualDirectionalShadowSoftCharacterMaterialSample()
    {
        return CreateVirtualDirectionalShadowSoftCharacterMaterialSampleCore();
    }

    private static NodeGraph CreateVirtualDirectionalShadowHalfLambertObjectSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1840f,
            120f,
            "用途说明\n这张图用于生成 VirtualDirectionalShadow_Object.fxsub。\n这里只负责“虚拟灯的半兰伯特受光贡献”，不要在这里接阴影节点。\n真正的阴影裁切由 VirtualDirectionalShadow.fx 的 ShadowMap + WPos + 合成 pass 统一完成。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            170f,
            520f,
            430f,
            "1. 材质底色\n先把材质漫反射色和材质贴图相乘。\n这样导出的贡献图会保留模型原本的材质颜色和透明度。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            590f,
            170f,
            520f,
            430f,
            "2. 虚拟灯方向\n读取 (OffscreenOwner) 控制器的 Direction 骨骼。\n这里取 -Z 轴作为照射到模型的入射方向，和 VirtualDirectionalShadow_Object.fxsub 旧版逻辑保持一致。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1150f,
            170f,
            720f,
            430f,
            "3. 半兰伯特贡献输出\nHalf-Lambert = dot(N, L) * 0.5 + 0.5。\n这里额外乘一个 0.35 的强度系数，避免贡献缓冲过亮。\n最后输出的是“未乘灯色/灯强/阴影可见度”的基础光照贡献。",
            0.54f,
            0.34f,
            0.20f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 220);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 390);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 360, 320);
        baseColorMultiply.Properties["Type"] = "Float4";

        var controllerDirection = graph.AddNode(NodeKind.ControlObjectBoneDirection, 650, 390);
        controllerDirection.Properties["Name"] = "(OffscreenOwner)";
        controllerDirection.Properties["Item"] = "Direction";
        controllerDirection.Properties["Axis"] = "-Z";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 650, 530);
        var halfLambert = graph.AddNode(NodeKind.HalfLambert, 930, 470);

        var diffuseScale = graph.AddNode(NodeKind.Scalar, 1200, 360);
        diffuseScale.Properties["Value"] = "0.35";

        var diffuseStrength = graph.AddNode(NodeKind.Multiply, 1200, 470);
        diffuseStrength.Properties["Type"] = "Float1";

        var lightMask = graph.AddNode(NodeKind.ComposeColor, 1480, 470);
        var lightMaskAlpha = graph.AddNode(NodeKind.Scalar, 1480, 640);
        lightMaskAlpha.Properties["Value"] = "1.0";

        var finalMultiply = graph.AddNode(NodeKind.Multiply, 1760, 390);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 2040, 390);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, worldNormal, "Value", halfLambert, "Normal");
        Connect(graph, controllerDirection, "Value", halfLambert, "LightDir");

        Connect(graph, halfLambert, "Result", diffuseStrength, "A");
        Connect(graph, diffuseScale, "Value", diffuseStrength, "B");

        Connect(graph, diffuseStrength, "Result", lightMask, "R");
        Connect(graph, diffuseStrength, "Result", lightMask, "G");
        Connect(graph, diffuseStrength, "Result", lightMask, "B");
        Connect(graph, lightMaskAlpha, "Value", lightMask, "A");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, lightMask, "Color", finalMultiply, "B");

        Connect(graph, finalMultiply, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowMaskSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1280f,
            140f,
            "用途说明\n这个模板只读取 VirtualDirectionalShadow 产生的阴影，不读取灯光颜色。\n输出是纯黑白遮罩：白 = 阴影，黑 = 受光。\n适合拿去做卡通渲染的分段、遮罩或阴影混合。",
            0.32f,
            0.42f,
            0.70f);

        AddFrame(
            graph,
            30f,
            190f,
            520f,
            360f,
            "1. 阴影读取\n节点会读取 VirtualDirLightShadowMap / VirtualDirLightShadowMapFar，\n并根据控制器方向和同样的近远参数重建虚拟灯阴影可见度。",
            0.42f,
            0.30f,
            0.20f);

        AddFrame(
            graph,
            590f,
            190f,
            500f,
            360f,
            "2. 结果输出\nLitFactor / ShadowMask 是已经过 Threshold / Softness 处理后的卡通化结果。\nRawLitFactor / RawShadowMask 是未经阈值化的原始结果。\nColor 输出默认是黑白预览，也可以接入自定义 LitColor / ShadowColor。",
            0.22f,
            0.48f,
            0.36f);

        var maskNode = graph.AddNode(NodeKind.VirtualDirectionalShadowMask, 120, 320);
        maskNode.Properties["ControllerName"] = "virtual_directional_shadow.pmx";
        maskNode.Properties["AnchorObjectName"] = "(self)";
        maskNode.Properties["AnchorBoneItem"] = "センター";
        maskNode.Properties["DirectionItem"] = "Direction";
        maskNode.Properties["ShadowExtentItem"] = "ShadowExtent";
        maskNode.Properties["ShadowDepthItem"] = "ShadowDepth";
        maskNode.Properties["ShadowBiasItem"] = "ShadowBias";
        maskNode.Properties["ShadowSoftnessItem"] = "ShadowSoftness";
        maskNode.Properties["ShadowVarianceItem"] = "ShadowVariance";
        maskNode.Properties["ShadowBleedItem"] = "ShadowBleed";
        maskNode.Properties["NearBufferName"] = "VirtualDirLightShadowMap";
        maskNode.Properties["FarBufferName"] = "VirtualDirLightShadowMapFar";
        maskNode.Properties["QualityPreset"] = "VDS_QUALITY_HIGH";
        maskNode.Properties["Threshold"] = "0.55";
        maskNode.Properties["Softness"] = "0.10";
        maskNode.Properties["ShadowAAMode"] = "High";
        var output = graph.AddNode(NodeKind.Output, 860, 320);
        output.Properties["AlphaMode"] = "Opaque";

        Connect(graph, maskNode, "ShadowMask", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowLambertBinaryShadowSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1760f,
            140f,
            "用途说明\n这个模板只保留两路最基础的虚拟灯光信号：\n1. 纯 Lambert 受光\n2. 纯二分自阴影\n默认输出是黑白二分阴影，方便直接看阴影质量。\nLambert 分支保留在图里，方便你后续继续接卡渲着色。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            190f,
            420f,
            420f,
            "1. 透明边界\n用 MaterialDiffuse.A × MaterialTexture.A 保留模型原本的透明边界。\n这样黑白阴影预览不会把透明材质涂满。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            500f,
            190f,
            480f,
            420f,
            "2. 虚拟灯光节点\nVirtualDirectionalShadowLight 只负责读虚拟灯方向和阴影图。\n这里强制关掉屏幕软阴影回采，避免污染纯二分结果。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1030f,
            190f,
            340f,
            420f,
            "3. 纯 Lambert\n用 WorldNormal + LightDir 单独算一条纯 Lambert 分支。\n这条线默认不直接输出，留给后续卡渲拼接。",
            0.54f,
            0.34f,
            0.20f);

        AddFrame(
            graph,
            1410f,
            190f,
            380f,
            420f,
            "4. 纯二分阴影预览\n把 ShadowVisibility 经过 Step(0.5) 做二分。\n输出结果：白 = 受光，黑 = 阴影。",
            0.30f,
            0.44f,
            0.22f);

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 280);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 450);
        var textureSplit = graph.AddNode(NodeKind.SplitColor, 280, 280);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 280, 450);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 470, 360);
        alphaMultiply.Properties["Type"] = "Float1";

        var virtualLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 690, 360);
        ConfigureVirtualDirectionalShadowLightMaterialNode(virtualLight, "Lambert");
        virtualLight.Properties["ProcessedShadowBufferName"] = string.Empty;
        virtualLight.Properties["ProcessedShadowBlend"] = "0.0";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 1090, 300);
        var lambert = graph.AddNode(NodeKind.Lambert, 1310, 300);
        var lambertPreview = graph.AddNode(NodeKind.ComposeColor, 1530, 260);
        var lambertAlpha = graph.AddNode(NodeKind.Scalar, 1530, 420);
        lambertAlpha.Properties["Value"] = "1.0";

        var binaryStep = graph.AddNode(NodeKind.Step, 1310, 500);
        binaryStep.Properties["Edge"] = "0.5";
        var binaryPreview = graph.AddNode(NodeKind.ComposeColor, 1530, 500);
        var binaryAlpha = graph.AddNode(NodeKind.Scalar, 1530, 660);
        binaryAlpha.Properties["Value"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 1770, 520);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialTexture, "Color", textureSplit, "Value");
        Connect(graph, materialDiffuse, "Value", diffuseSplit, "Value");
        Connect(graph, textureSplit, "A", alphaMultiply, "A");
        Connect(graph, diffuseSplit, "A", alphaMultiply, "B");

        Connect(graph, worldNormal, "Value", lambert, "Normal");
        Connect(graph, virtualLight, "LightDir", lambert, "LightDir");

        Connect(graph, lambert, "Result", lambertPreview, "R");
        Connect(graph, lambert, "Result", lambertPreview, "G");
        Connect(graph, lambert, "Result", lambertPreview, "B");
        Connect(graph, lambertAlpha, "Value", lambertPreview, "A");

        Connect(graph, virtualLight, "ShadowVisibility", binaryStep, "X");
        Connect(graph, binaryStep, "Result", binaryPreview, "R");
        Connect(graph, binaryStep, "Result", binaryPreview, "G");
        Connect(graph, binaryStep, "Result", binaryPreview, "B");
        Connect(graph, binaryAlpha, "Value", binaryPreview, "A");

        Connect(graph, binaryPreview, "Color", output, "Color");
        Connect(graph, alphaMultiply, "Result", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowLambertPreviewSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1600f,
            140f,
            "用途说明\n这个模板只看虚拟灯光方向驱动下的纯 Lambert 结果。\n它不读屏幕软阴影回采，也不做二分阈值。\n默认输出是灰阶 Lambert 预览，方便你单独观察受光分布。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            190f,
            420f,
            400f,
            "1. 透明边界\n用 MaterialDiffuse.A × MaterialTexture.A 保留模型原本的透明边界。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            500f,
            190f,
            460f,
            400f,
            "2. 虚拟灯光方向\nVirtualDirectionalShadowLight 只拿 LightDir。\n这里强制关掉屏幕软阴影回采，避免 Lambert 预览被污染。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1010f,
            190f,
            520f,
            400f,
            "3. 纯 Lambert 预览\nWorldNormal + LightDir -> Lambert。\n输出结果：黑 = 背光，白 = 正对光。",
            0.54f,
            0.34f,
            0.20f);

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 280);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 430);
        var textureSplit = graph.AddNode(NodeKind.SplitColor, 280, 280);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 280, 430);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 470, 350);
        alphaMultiply.Properties["Type"] = "Float1";

        var virtualLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 700, 350);
        ConfigureVirtualDirectionalShadowLightMaterialNode(virtualLight, "Lambert");
        virtualLight.Properties["ProcessedShadowBufferName"] = string.Empty;
        virtualLight.Properties["ProcessedShadowBlend"] = "0.0";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 1040, 280);
        var lambert = graph.AddNode(NodeKind.Lambert, 1260, 320);
        var lambertPreview = graph.AddNode(NodeKind.ComposeColor, 1480, 320);
        var lambertAlpha = graph.AddNode(NodeKind.Scalar, 1480, 490);
        lambertAlpha.Properties["Value"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 1720, 340);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialTexture, "Color", textureSplit, "Value");
        Connect(graph, materialDiffuse, "Value", diffuseSplit, "Value");
        Connect(graph, textureSplit, "A", alphaMultiply, "A");
        Connect(graph, diffuseSplit, "A", alphaMultiply, "B");

        Connect(graph, worldNormal, "Value", lambert, "Normal");
        Connect(graph, virtualLight, "LightDir", lambert, "LightDir");

        Connect(graph, lambert, "Result", lambertPreview, "R");
        Connect(graph, lambert, "Result", lambertPreview, "G");
        Connect(graph, lambert, "Result", lambertPreview, "B");
        Connect(graph, lambertAlpha, "Value", lambertPreview, "A");

        Connect(graph, lambertPreview, "Color", output, "Color");
        Connect(graph, alphaMultiply, "Result", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowSoftShadowPreviewSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1760f,
            140f,
            "用途说明\n这个模板专门预览 VDS 软阴影回采结果。\n它不混材质底色，也不混 Lambert，只看虚拟灯光阴影遮罩本身。\n默认输出是回采后的软阴影遮罩，图里同时保留连续可见度预览分支。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            190f,
            420f,
            420f,
            "1. 透明边界\n用 MaterialDiffuse.A × MaterialTexture.A 保留模型原本的透明边界。\n这样预览只显示真实参与阴影的区域。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            500f,
            190f,
            500f,
            420f,
            "2. 软阴影回采\nVirtualDirectionalShadowMask 读取原始 VDS 阴影，再接回 VdsSoftShadowProcessed。\n这里不参与 Lambert，只保留阴影可见度本身。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1060f,
            190f,
            700f,
            420f,
            "3. 两路预览\nRawLitFactor = 回采后的连续可见度。\nShadowMask = 回采结果再经过阈值化后的软阴影遮罩。\n默认输出 ShadowMask，方便直观看软阴影是否只是柔化了边缘。",
            0.54f,
            0.34f,
            0.20f);

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 280);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 450);
        var textureSplit = graph.AddNode(NodeKind.SplitColor, 280, 280);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 280, 450);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 470, 360);
        alphaMultiply.Properties["Type"] = "Float1";

        var shadowMask = graph.AddNode(NodeKind.VirtualDirectionalShadowMask, 820, 360);
        ConfigureVirtualDirectionalShadowMaskMaterialNode(shadowMask);
        ConfigureVirtualDirectionalShadowProcessedScreenShadow(shadowMask, "VdsSoftShadowProcessed", "1.0", "R");
        shadowMask.Properties["Softness"] = "0.0";
        shadowMask.Properties["Threshold"] = "0.5";
        shadowMask.Properties["ShadowAAMode"] = "High";

        var previewColor = graph.AddNode(NodeKind.ComposeColor, 1180, 360);
        var previewAlpha = graph.AddNode(NodeKind.Scalar, 1180, 530);
        previewAlpha.Properties["Value"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 1460, 360);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialTexture, "Color", textureSplit, "Value");
        Connect(graph, materialDiffuse, "Value", diffuseSplit, "Value");
        Connect(graph, textureSplit, "A", alphaMultiply, "A");
        Connect(graph, diffuseSplit, "A", alphaMultiply, "B");

        Connect(graph, shadowMask, "SoftShadowMask", previewColor, "R");
        Connect(graph, shadowMask, "SoftShadowMask", previewColor, "G");
        Connect(graph, shadowMask, "SoftShadowMask", previewColor, "B");
        Connect(graph, previewAlpha, "Value", previewColor, "A");

        Connect(graph, previewColor, "Color", output, "Color");
        Connect(graph, alphaMultiply, "Result", output, "Alpha");

        return graph;
    }
    private static NodeGraph CreateVirtualDirectionalShadowToonMaskMaterialSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1620f,
            140f,
            "用途说明\n这是给团队直接用的“卡通阴影遮罩材质模板”。\n它会读取 VirtualDirectionalShadow 生成的黑白阴影遮罩，再把材质底色在受光色和阴影色之间分段混合。\n如果只想拿纯遮罩，请改用“虚拟定向灯阴影遮罩”模板。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            190f,
            500f,
            420f,
            "1. 材质底色\n材质漫反射色 × 材质贴图。\n保留模型原本的颜色、透明度和材质 tint。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            570f,
            190f,
            480f,
            420f,
            "2. 阴影遮罩\nVirtualDirectionalShadowMask 只读取虚拟灯阴影。\nThreshold / Softness 已内置在节点里，适合直接做卡渲分段。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1080f,
            190f,
            570f,
            420f,
            "3. 卡通混合\n直接把 LitColor / ShadowColor 接进节点内部。\n节点 Color 会输出已经完成卡通分段的亮暗色，再乘回材质底色即可。",
            0.54f,
            0.34f,
            0.20f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 260);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 420);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 320, 340);
        baseColorMultiply.Properties["Type"] = "Float4";

        var shadowMask = graph.AddNode(NodeKind.VirtualDirectionalShadowMask, 650, 340);
        shadowMask.Properties["ControllerName"] = "virtual_directional_shadow.pmx";
        shadowMask.Properties["AnchorObjectName"] = "(self)";
        shadowMask.Properties["AnchorBoneItem"] = "センター";
        shadowMask.Properties["DirectionItem"] = "Direction";
        shadowMask.Properties["ShadowExtentItem"] = "ShadowExtent";
        shadowMask.Properties["ShadowDepthItem"] = "ShadowDepth";
        shadowMask.Properties["ShadowBiasItem"] = "ShadowBias";
        shadowMask.Properties["ShadowSoftnessItem"] = "ShadowSoftness";
        shadowMask.Properties["ShadowVarianceItem"] = "ShadowVariance";
        shadowMask.Properties["ShadowBleedItem"] = "ShadowBleed";
        shadowMask.Properties["NearBufferName"] = "VirtualDirLightShadowMap";
        shadowMask.Properties["FarBufferName"] = "VirtualDirLightShadowMapFar";
        shadowMask.Properties["QualityPreset"] = "VDS_QUALITY_HIGH";
        shadowMask.Properties["Threshold"] = "0.55";
        shadowMask.Properties["Softness"] = "0.10";
        shadowMask.Properties["ShadowAAMode"] = "High";
        shadowMask.Properties["ShadowStrength"] = "1.0";

        var shadowTint = graph.AddNode(NodeKind.Color, 1110, 270);
        shadowTint.Properties["R"] = "0.28";
        shadowTint.Properties["G"] = "0.28";
        shadowTint.Properties["B"] = "0.32";
        shadowTint.Properties["A"] = "1.0";

        var litTint = graph.AddNode(NodeKind.Color, 1110, 430);
        litTint.Properties["R"] = "1.0";
        litTint.Properties["G"] = "1.0";
        litTint.Properties["B"] = "1.0";
        litTint.Properties["A"] = "1.0";

        var finalMultiply = graph.AddNode(NodeKind.Multiply, 1610, 340);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1870, 340);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, litTint, "Color", shadowMask, "LitColor");
        Connect(graph, shadowTint, "Color", shadowMask, "ShadowColor");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, shadowMask, "Color", finalMultiply, "B");

        Connect(graph, finalMultiply, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowToonLightMaterialSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(
            graph,
            30f,
            20f,
            1780f,
            140f,
            "用途说明\n这个模板把“虚拟灯方向 + 二分受光 + 角色被照出的阴影 + 角色自阴影”合成到同一个节点里。\n节点会输出 LightFactor / ShadowMask / LightDir。\n这样你后面可以继续接高光、金属、菲涅耳，而不用再自己拼虚拟灯方向和阴影。",
            0.30f,
            0.42f,
            0.72f);

        AddFrame(
            graph,
            30f,
            190f,
            500f,
            440f,
            "1. 材质底色\n材质漫反射色 × 材质贴图。\n这里仍然保留底色，方便把虚拟灯的明暗结果乘回模型原本颜色。",
            0.44f,
            0.30f,
            0.18f);

        AddFrame(
            graph,
            560f,
            190f,
            520f,
            440f,
            "2. 虚拟灯卡通光照\nVirtualDirectionalShadowLight 会读取虚拟灯方向和阴影图。\nDiffuseMode 建议默认 HalfLambert，再用 Threshold / Softness 做二分阴影。\nLightDir 输出可以继续拿去算高光和金属。",
            0.24f,
            0.50f,
            0.36f);

        AddFrame(
            graph,
            1110f,
            190f,
            700f,
            440f,
            "3. 最终结果\n先用节点内部的 LitColor / ShadowColor 得到卡通明暗，再乘回材质底色。\n如果你要继续加高光，直接从这个节点取 LightDir 去连 BlinnPhong / GGX 之类节点即可。",
            0.54f,
            0.34f,
            0.20f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 270);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 430);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 330, 350);
        baseColorMultiply.Properties["Type"] = "Float4";

        var toonLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 680, 350);
        toonLight.Properties["ControllerName"] = "virtual_directional_shadow.pmx";
        toonLight.Properties["AnchorObjectName"] = "(self)";
        toonLight.Properties["AnchorBoneItem"] = "センター";
        toonLight.Properties["DirectionItem"] = "Direction";
        toonLight.Properties["ShadowExtentItem"] = "ShadowExtent";
        toonLight.Properties["ShadowDepthItem"] = "ShadowDepth";
        toonLight.Properties["ShadowBiasItem"] = "ShadowBias";
        toonLight.Properties["ShadowSoftnessItem"] = "ShadowSoftness";
        toonLight.Properties["ShadowVarianceItem"] = "ShadowVariance";
        toonLight.Properties["ShadowBleedItem"] = "ShadowBleed";
        toonLight.Properties["NearBufferName"] = "VirtualDirLightShadowMap";
        toonLight.Properties["FarBufferName"] = "VirtualDirLightShadowMapFar";
        toonLight.Properties["QualityPreset"] = "VDS_QUALITY_HIGH";
        toonLight.Properties["DiffuseMode"] = "HalfLambert";
        toonLight.Properties["Threshold"] = "0.55";
        toonLight.Properties["Softness"] = "0.10";
        toonLight.Properties["ShadowAAMode"] = "High";
        toonLight.Properties["ShadowStrength"] = "1.0";

        var shadowTint = graph.AddNode(NodeKind.Color, 1130, 280);
        shadowTint.Properties["R"] = "0.28";
        shadowTint.Properties["G"] = "0.28";
        shadowTint.Properties["B"] = "0.32";
        shadowTint.Properties["A"] = "1.0";

        var litTint = graph.AddNode(NodeKind.Color, 1130, 450);
        litTint.Properties["R"] = "1.0";
        litTint.Properties["G"] = "1.0";
        litTint.Properties["B"] = "1.0";
        litTint.Properties["A"] = "1.0";

        var finalMultiply = graph.AddNode(NodeKind.Multiply, 1450, 350);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1730, 350);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, litTint, "Color", toonLight, "LitColor");
        Connect(graph, shadowTint, "Color", toonLight, "ShadowColor");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, toonLight, "Color", finalMultiply, "B");

        Connect(graph, finalMultiply, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowLambertMaterialSampleCore()
    {
        return CreateVirtualDirectionalShadowDiffuseMaterialSampleCore(
            "Lambert",
            "用途说明\n这个模板只做“虚拟光源 Lambert 受光 + 虚拟光源投射阴影”。\n受光方向和阴影都只由 virtual_directional_shadow.pmx 控制。",
            "3. Lambert 输出\n这里直接使用 VirtualDirectionalShadowLight 的 RawLightFactor。\n它等于 Lambert 受光再乘虚拟光源阴影可见度，不额外做二分阈值。");
    }

    private static NodeGraph CreateVirtualDirectionalShadowHalfLambertMaterialSampleCore()
    {
        return CreateVirtualDirectionalShadowDiffuseMaterialSampleCore(
            "HalfLambert",
            "用途说明\n这个模板只做“虚拟光源 Half-Lambert 受光 + 虚拟光源投射阴影”。\n受光方向和阴影都只由 virtual_directional_shadow.pmx 控制。",
            "3. Half-Lambert 输出\n这里直接使用 VirtualDirectionalShadowLight 的 RawLightFactor。\n它等于 Half-Lambert 受光再乘虚拟光源阴影可见度，不额外做二分阈值。");
    }

    private static NodeGraph CreateVirtualDirectionalShadowDiffuseMaterialSampleCore(
        string diffuseMode,
        string headerText,
        string outputText)
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(graph, 30f, 20f, 1760f, 140f, headerText, 0.30f, 0.42f, 0.72f);
        AddFrame(graph, 30f, 190f, 430f, 430f, "1. 材质底色\n材质漫反射色 × 材质贴图。\n保留模型原本颜色和透明度。", 0.44f, 0.30f, 0.18f);
        AddFrame(graph, 500f, 190f, 520f, 430f, "2. 虚拟光源方向与阴影\nVirtualDirectionalShadowLight 负责读取 Direction 和 ShadowMap。\n这里只用它的 RawLightFactor，保留连续明暗和虚拟光源投射阴影。", 0.24f, 0.50f, 0.36f);
        AddFrame(graph, 1080f, 190f, 620f, 430f, outputText, 0.54f, 0.34f, 0.20f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 280);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 440);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 290, 360);
        baseColorMultiply.Properties["Type"] = "Float4";

        var virtualLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 620, 360);
        ConfigureVirtualDirectionalShadowLightMaterialNode(virtualLight, diffuseMode);

        var lightMask = graph.AddNode(NodeKind.ComposeColor, 980, 320);
        var lightMaskAlpha = graph.AddNode(NodeKind.Scalar, 980, 490);
        lightMaskAlpha.Properties["Value"] = "1.0";

        var finalMultiply = graph.AddNode(NodeKind.Multiply, 1260, 360);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1540, 360);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, virtualLight, "RawLightFactor", lightMask, "R");
        Connect(graph, virtualLight, "RawLightFactor", lightMask, "G");
        Connect(graph, virtualLight, "RawLightFactor", lightMask, "B");
        Connect(graph, lightMaskAlpha, "Value", lightMask, "A");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, lightMask, "Color", finalMultiply, "B");
        Connect(graph, finalMultiply, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowBlinnPhongMaterialSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(graph, 30f, 20f, 1840f, 140f, "用途说明\n这个模板只做“虚拟光源 Blinn-Phong 高光 + 虚拟光源投射阴影”。\n高光方向和阴影都只由 virtual_directional_shadow.pmx 控制。", 0.30f, 0.42f, 0.72f);
        AddFrame(graph, 30f, 190f, 420f, 460f, "1. Alpha 保留\n材质漫反射色 × 材质贴图只用于保留原始 alpha。\n高光颜色来自 MaterialSpecular。", 0.44f, 0.30f, 0.18f);
        AddFrame(graph, 490f, 190f, 560f, 460f, "2. 虚拟光源方向与阴影\nVirtualDirectionalShadowLight 只提供 LightDir 和 ShadowVisibility。\nBlinn-Phong 的方向和遮挡都由虚拟光源控制。", 0.24f, 0.50f, 0.36f);
        AddFrame(graph, 1100f, 190f, 690f, 460f, "3. Blinn-Phong 输出\nBlinn-Phong 结果再乘虚拟光源阴影可见度。\n输出仅保留高光 RGB，并继承原材质 alpha。", 0.54f, 0.34f, 0.20f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 280);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 440);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 280, 360);
        baseColorMultiply.Properties["Type"] = "Float4";

        var virtualLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 620, 360);
        ConfigureVirtualDirectionalShadowLightMaterialNode(virtualLight, "HalfLambert");

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 920, 250);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 920, 430);
        var blinnPhong = graph.AddNode(NodeKind.BlinnPhong, 1180, 340);
        blinnPhong.Properties["Power"] = "32.0";

        var specFactor = graph.AddNode(NodeKind.Multiply, 1450, 340);
        specFactor.Properties["Type"] = "Float1";
        var specMask = graph.AddNode(NodeKind.ComposeColor, 1730, 300);
        var specMaskAlpha = graph.AddNode(NodeKind.Scalar, 1730, 470);
        specMaskAlpha.Properties["Value"] = "0.0";

        var materialSpecular = graph.AddNode(NodeKind.MaterialSpecularColor, 1730, 620);
        var specularColor = graph.AddNode(NodeKind.Multiply, 2010, 380);
        specularColor.Properties["Type"] = "Float4";

        var splitSpecular = graph.AddNode(NodeKind.SplitColor, 2290, 380);
        var splitBase = graph.AddNode(NodeKind.SplitColor, 2290, 580);
        var finalColor = graph.AddNode(NodeKind.ComposeColor, 2570, 420);
        var output = graph.AddNode(NodeKind.Output, 2850, 420);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, worldNormal, "Value", blinnPhong, "Normal");
        Connect(graph, viewDirection, "Value", blinnPhong, "ViewDir");
        Connect(graph, virtualLight, "LightDir", blinnPhong, "LightDir");

        Connect(graph, blinnPhong, "Result", specFactor, "A");
        Connect(graph, virtualLight, "ShadowVisibility", specFactor, "B");

        Connect(graph, specFactor, "Result", specMask, "R");
        Connect(graph, specFactor, "Result", specMask, "G");
        Connect(graph, specFactor, "Result", specMask, "B");
        Connect(graph, specMaskAlpha, "Value", specMask, "A");

        Connect(graph, materialSpecular, "Value", specularColor, "A");
        Connect(graph, specMask, "Color", specularColor, "B");

        Connect(graph, specularColor, "Result", splitSpecular, "Value");
        Connect(graph, baseColorMultiply, "Result", splitBase, "Value");

        Connect(graph, splitSpecular, "R", finalColor, "R");
        Connect(graph, splitSpecular, "G", finalColor, "G");
        Connect(graph, splitSpecular, "B", finalColor, "B");
        Connect(graph, splitBase, "A", finalColor, "A");
        Connect(graph, finalColor, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowDualToonLightMaterialSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(graph, 30f, 20f, 2020f, 140f, "用途说明\n这个模板把角色自阴影(Self)和外部投射阴影(Cast)拆成两套 VDS 系统。\nSelf 负责角色本体受光与自阴影，Cast 只负责额外遮挡，再在材质侧合并。", 0.30f, 0.42f, 0.72f);
        AddFrame(graph, 30f, 190f, 430f, 520f, "1. 材质底色\n材质漫反射色 x 材质贴图。\n最后仍然乘回原本底色，保持材质 alpha。", 0.44f, 0.30f, 0.18f);
        AddFrame(graph, 500f, 190f, 1080f, 520f, "2. Dual 节点\nVirtualDirectionalShadowDual 内部同时读取 Self / Cast 两套 VDS。\nSelf 负责角色本体受光与自阴影，Cast 负责额外投射阴影，并在节点里合并。", 0.24f, 0.50f, 0.36f);
        AddFrame(graph, 1610f, 190f, 400f, 520f, "3. 合并结果\n节点直接输出 CombinedLightFactor / CombinedShadowMask / LightDir。\n这样图里只保留一个阴影节点，后续材质拓展会简单很多。", 0.38f, 0.38f, 0.18f);

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 90, 300);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 90, 470);
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 280, 390);
        baseColorMultiply.Properties["Type"] = "Float4";

        var dualShadow = graph.AddNode(NodeKind.VirtualDirectionalShadowDual, 780, 390);
        dualShadow.Properties["AnchorObjectName"] = "(self)";
        dualShadow.Properties["AnchorBoneItem"] = "センター";
        dualShadow.Properties["SelfControllerName"] = "virtual_directional_shadow_self.pmx";
        dualShadow.Properties["AnchorBoneItem"] = "センター";
        dualShadow.Properties["SelfDirectionItem"] = "Direction";
        dualShadow.Properties["SelfShadowExtentItem"] = "ShadowExtent";
        dualShadow.Properties["SelfShadowDepthItem"] = "ShadowDepth";
        dualShadow.Properties["SelfShadowBiasItem"] = "ShadowBias";
        dualShadow.Properties["SelfShadowSoftnessItem"] = "ShadowSoftness";
        dualShadow.Properties["SelfShadowVarianceItem"] = "ShadowVariance";
        dualShadow.Properties["SelfShadowBleedItem"] = "ShadowBleed";
        dualShadow.Properties["SelfNearBufferName"] = "VirtualDirSelfLightShadowMap";
        dualShadow.Properties["SelfFarBufferName"] = "VirtualDirSelfLightShadowMapFar";
        dualShadow.Properties["SelfQualityPreset"] = "VDS_QUALITY_HIGH";
        dualShadow.Properties["SelfNearExtentScale"] = "0.55";
        dualShadow.Properties["SelfFarExtentScale"] = "1.70";
        dualShadow.Properties["SelfNearDepthScale"] = "0.75";
        dualShadow.Properties["SelfFarDepthScale"] = "2.20";
        dualShadow.Properties["SelfBlendStart"] = "0.45";
        dualShadow.Properties["SelfBlendEnd"] = "0.82";
        dualShadow.Properties["SelfThreshold"] = "0.55";
        dualShadow.Properties["SelfSoftness"] = "0.10";
        dualShadow.Properties["SelfShadowAAMode"] = "High";
        dualShadow.Properties["SelfShadowStrength"] = "1.0";
        dualShadow.Properties["CastControllerName"] = "virtual_directional_shadow_cast.pmx";
        dualShadow.Properties["CastDirectionItem"] = "Direction";
        dualShadow.Properties["CastShadowExtentItem"] = "ShadowExtent";
        dualShadow.Properties["CastShadowDepthItem"] = "ShadowDepth";
        dualShadow.Properties["CastShadowBiasItem"] = "ShadowBias";
        dualShadow.Properties["CastShadowSoftnessItem"] = "ShadowSoftness";
        dualShadow.Properties["CastShadowVarianceItem"] = "ShadowVariance";
        dualShadow.Properties["CastShadowBleedItem"] = "ShadowBleed";
        dualShadow.Properties["CastNearBufferName"] = "VirtualDirCastLightShadowMap";
        dualShadow.Properties["CastFarBufferName"] = "VirtualDirCastLightShadowMapFar";
        dualShadow.Properties["CastQualityPreset"] = "VDS_QUALITY_HIGH";
        dualShadow.Properties["CastNearExtentScale"] = "0.90";
        dualShadow.Properties["CastFarExtentScale"] = "3.20";
        dualShadow.Properties["CastNearDepthScale"] = "0.90";
        dualShadow.Properties["CastFarDepthScale"] = "3.80";
        dualShadow.Properties["CastBlendStart"] = "0.55";
        dualShadow.Properties["CastBlendEnd"] = "0.90";
        dualShadow.Properties["CastThreshold"] = "0.55";
        dualShadow.Properties["CastSoftness"] = "0.10";
        dualShadow.Properties["CastShadowAAMode"] = "High";
        dualShadow.Properties["CastShadowStrength"] = "1.0";
        dualShadow.Properties["DiffuseMode"] = "HalfLambert";
        dualShadow.Properties["CombineMode"] = "Min";
        dualShadow.Properties["Threshold"] = "0.55";
        dualShadow.Properties["Softness"] = "0.10";
        dualShadow.Properties["ShadowAAMode"] = "High";
        dualShadow.Properties["EdgeSofteningMode"] = "Fast";
        dualShadow.Properties["EdgeSofteningBand"] = "0.06";
        dualShadow.Properties["EdgeSofteningStrength"] = "0.75";
        dualShadow.Properties["ShadowStrength"] = "1.0";

        var shadowTint = graph.AddNode(NodeKind.Color, 1500, 520);
        shadowTint.Properties["R"] = "0.28";
        shadowTint.Properties["G"] = "0.28";
        shadowTint.Properties["B"] = "0.32";
        shadowTint.Properties["A"] = "1.0";

        var litTint = graph.AddNode(NodeKind.Color, 1500, 650);
        litTint.Properties["R"] = "1.0";
        litTint.Properties["G"] = "1.0";
        litTint.Properties["B"] = "1.0";
        litTint.Properties["A"] = "1.0";

        var shadowMix = graph.AddNode(NodeKind.ShadowColorMix, 1760, 470);
        var finalMultiply = graph.AddNode(NodeKind.Multiply, 2020, 470);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 2280, 470);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");

        Connect(graph, shadowTint, "Color", shadowMix, "ShadowColor");
        Connect(graph, litTint, "Color", shadowMix, "LitColor");
        Connect(graph, dualShadow, "CombinedLightFactor", shadowMix, "Factor");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, shadowMix, "Color", finalMultiply, "B");
        Connect(graph, finalMultiply, "Result", output, "Color");

        Connect(graph, litTint, "Color", dualShadow, "LitColor");
        Connect(graph, shadowTint, "Color", dualShadow, "ShadowColor");

        return graph;
    }

    private static void ConfigureVirtualDirectionalShadowSystem(GraphNode node, string controllerName, string nearBufferName, string farBufferName)
    {
        node.Properties["ControllerName"] = controllerName;
        node.Properties["NearBufferName"] = nearBufferName;
        node.Properties["FarBufferName"] = farBufferName;
    }

    private static void ConfigureVirtualDirectionalShadowMaskMaterialNode(GraphNode node)
    {
        node.Properties["ControllerName"] = "virtual_directional_shadow.pmx";
        node.Properties["AnchorObjectName"] = "(self)";
        node.Properties["AnchorBoneItem"] = "センター";
        node.Properties["DirectionItem"] = "Direction";
        node.Properties["AnchorBoneItem"] = "センター";
        node.Properties["ShadowExtentItem"] = "ShadowExtent";
        node.Properties["ShadowDepthItem"] = "ShadowDepth";
        node.Properties["ShadowBiasItem"] = "ShadowBias";
        node.Properties["ShadowSoftnessItem"] = "ShadowSoftness";
        node.Properties["ShadowVarianceItem"] = "ShadowVariance";
        node.Properties["ShadowBleedItem"] = "ShadowBleed";
        node.Properties["NearBufferName"] = "VirtualDirLightShadowMap";
        node.Properties["FarBufferName"] = "VirtualDirLightShadowMapFar";
        node.Properties["QualityPreset"] = "VDS_QUALITY_HIGH";
        node.Properties["Threshold"] = "0.55";
        node.Properties["Softness"] = "0.10";
        node.Properties["ShadowAAMode"] = "High";
        node.Properties["ShadowStrength"] = "1.0";
    }

    private static void ConfigureVirtualDirectionalShadowProcessedScreenShadow(GraphNode node, string bufferName, string blend, string channel = "R")
    {
        node.Properties["ProcessedShadowBufferName"] = bufferName;
        node.Properties["ProcessedShadowChannel"] = channel;
        node.Properties["ProcessedShadowBlend"] = blend;
    }

    private static void ConfigureVirtualDirectionalShadowLightMaterialNode(GraphNode node, string diffuseMode)
    {
        node.Properties["ControllerName"] = "virtual_directional_shadow.pmx";
        node.Properties["AnchorObjectName"] = "(self)";
        node.Properties["AnchorBoneItem"] = "センター";
        node.Properties["DirectionItem"] = "Direction";
        node.Properties["ShadowExtentItem"] = "ShadowExtent";
        node.Properties["ShadowDepthItem"] = "ShadowDepth";
        node.Properties["ShadowBiasItem"] = "ShadowBias";
        node.Properties["ShadowSoftnessItem"] = "ShadowSoftness";
        node.Properties["ShadowVarianceItem"] = "ShadowVariance";
        node.Properties["ShadowBleedItem"] = "ShadowBleed";
        node.Properties["NearBufferName"] = "VirtualDirLightShadowMap";
        node.Properties["FarBufferName"] = "VirtualDirLightShadowMapFar";
        node.Properties["QualityPreset"] = "VDS_QUALITY_HIGH";
        node.Properties["DiffuseMode"] = diffuseMode;
        node.Properties["Threshold"] = "0.55";
        node.Properties["Softness"] = "0.10";
        node.Properties["ShadowAAMode"] = "High";
        node.Properties["ShadowStrength"] = "1.0";
    }

    private static NodeGraph CreateVirtualDirectionalShadowScreenShadowCaptureSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddFrame(graph, 30f, 20f, 1780f, 140f, "VDS 软阴影捕获\n这张图负责把 VirtualDirectionalShadow 的原始屏幕阴影可见度写到离屏缓冲。\n后面的 ScenePostProcess 模板会把这张缓冲做模糊，再由材质模板读回。", 0.30f, 0.42f, 0.72f);
        AddFrame(graph, 30f, 200f, 480f, 420f, "1. 透明裁切\n用 MaterialDiffuse.A × MaterialTexture.A 保留模型原本的透明边界。", 0.44f, 0.30f, 0.18f);
        AddFrame(graph, 560f, 200f, 520f, 420f, "2. 阴影读取\nVirtualDirectionalShadowMask 直接输出 RawLitFactor。\n这里保留连续可见度，不做二分阈值。", 0.24f, 0.50f, 0.36f);
        AddFrame(graph, 1140f, 200f, 620f, 420f, "3. 层缓冲输出\nLayerSourceOutput 把灰度阴影写到场景层缓冲。\n推荐导出文件名：virtual_directional_shadow_screen_shadow_capture.fx", 0.54f, 0.34f, 0.20f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 280);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 300, 260);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 300, 420);
        var textureSplit = graph.AddNode(NodeKind.SplitColor, 520, 260);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 520, 420);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 760, 360);
        alphaMultiply.Properties["Type"] = "Float1";

        var shadowMask = graph.AddNode(NodeKind.VirtualDirectionalShadowMask, 760, 540);
        ConfigureVirtualDirectionalShadowMaskMaterialNode(shadowMask);
        var clipPosition = graph.AddNode(NodeKind.ClipSpacePosition, 760, 700);
        var clipSplit = graph.AddNode(NodeKind.SplitColor, 980, 700);
        var clipDepth = graph.AddNode(NodeKind.Divide, 1200, 700);
        clipDepth.Properties["Type"] = "Float1";

        var penumbraStrength = graph.AddNode(NodeKind.Min, 1440, 700);
        penumbraStrength.Properties["Type"] = "Float1";

        var shadowColor = graph.AddNode(NodeKind.ComposeColor, 1700, 520);
        var oneAlpha = graph.AddNode(NodeKind.Scalar, 1440, 700);
        oneAlpha.Properties["Value"] = "1.0";

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 2000, 500);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 2280, 500);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialTexture, "Color", textureSplit, "Value");
        Connect(graph, materialDiffuse, "Value", diffuseSplit, "Value");
        Connect(graph, textureSplit, "A", alphaMultiply, "A");
        Connect(graph, diffuseSplit, "A", alphaMultiply, "B");

        Connect(graph, clipPosition, "Value", clipSplit, "Value");
        Connect(graph, clipSplit, "B", clipDepth, "A");
        Connect(graph, clipSplit, "A", clipDepth, "B");

        Connect(graph, shadowMask, "RawLitFactor", penumbraStrength, "A");
        Connect(graph, shadowMask, "RawShadowMask", penumbraStrength, "B");
        Connect(graph, shadowMask, "RawShadowMask", shadowColor, "R");
        Connect(graph, clipDepth, "Result", shadowColor, "G");
        Connect(graph, penumbraStrength, "Result", shadowColor, "B");
        Connect(graph, oneAlpha, "Value", shadowColor, "A");

        Connect(graph, shadowColor, "Color", layerOutput, "Color");
        Connect(graph, alphaMultiply, "Result", layerOutput, "AlphaMask");
        Connect(graph, layerOutput, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowScreenShadowPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        AddFrame(graph, 30f, 20f, 2120f, 150f, "VDS 软阴影处理\n这张图读取 VdsSoftShadowRaw 的多通道数据：R=阴影可见度，G=屏幕深度，B=软化强度。\n后处理使用两段 depth-aware bilateral blur，最终结果写入 VdsSoftShadowProcessed，角色材质再把它混回 VDS 节点。", 0.30f, 0.42f, 0.72f);
        AddFrame(graph, 30f, 210f, 440f, 420f, "1. 原始阴影缓冲\nOffscreenBufferSample 读取角色材质额外写出的 VdsSoftShadowRaw。", 0.44f, 0.30f, 0.18f);
        AddFrame(graph, 530f, 210f, 720f, 420f, "2. 深度感知软化\nBilateralBlur 直接用 Raw.G 当深度 guide、Raw.B 当半径调制。\n这样阴影边缘会保留得更稳，不会像普通整屏 GaussianBlur 那样糊穿。", 0.24f, 0.50f, 0.36f);
        AddFrame(graph, 1320f, 210f, 830f, 420f, "3. 输出\nCaptureColor 先写中间缓冲 VdsSoftShadowTemp，CaptureSecondaryColor 再写最终 VdsSoftShadowProcessed。\nColor 端仍然回传原始 SceneColor，所以这个后处理不会改变最终画面。", 0.54f, 0.34f, 0.20f);

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 100, 340);
        var sceneColor = graph.AddNode(NodeKind.SceneColor, 320, 260);

        var rawLayer = graph.AddNode(NodeKind.OffscreenBufferSample, 320, 500);
        rawLayer.Properties["BufferName"] = "VdsSoftShadowRaw";
        rawLayer.Properties["Description"] = "VDS raw screen shadow capture";
        rawLayer.Properties["Format"] = "A16B16G16R16F";
        rawLayer.Properties["FilterMode"] = "Linear";

        var blurPrimary = graph.AddNode(NodeKind.BilateralBlur, 760, 260);
        blurPrimary.Properties["Source"] = "OffscreenBuffer";
        blurPrimary.Properties["BufferName"] = "VdsSoftShadowRaw";
        blurPrimary.Properties["Description"] = "VDS raw screen shadow capture";
        blurPrimary.Properties["Format"] = "A16B16G16R16F";
        blurPrimary.Properties["FilterMode"] = "Linear";
        blurPrimary.Properties["GuideBufferName"] = "VdsSoftShadowRaw";
        blurPrimary.Properties["DepthSource"] = "GuideChannel";
        blurPrimary.Properties["DepthChannel"] = "G";
        blurPrimary.Properties["AdaptiveStepChannel"] = "B";
        blurPrimary.Properties["AdaptiveStepScale"] = "1.75";
        blurPrimary.Properties["AdaptiveStepBias"] = "0.35";
        blurPrimary.Properties["StepScale"] = "1.5";
        blurPrimary.Properties["Strength"] = "1.0";
        blurPrimary.Properties["DepthThreshold"] = "0.0035";
        blurPrimary.Properties["NormalThreshold"] = "0.15";
        blurPrimary.Properties["PreserveAlpha"] = "True";

        var blurSecondary = graph.AddNode(NodeKind.BilateralBlur, 1260, 260);
        blurSecondary.Properties["Source"] = "OffscreenBuffer";
        blurSecondary.Properties["BufferName"] = "VdsSoftShadowTemp";
        blurSecondary.Properties["Description"] = "VDS soft shadow intermediate";
        blurSecondary.Properties["Format"] = "A16B16G16R16F";
        blurSecondary.Properties["FilterMode"] = "Linear";
        blurSecondary.Properties["GuideBufferName"] = "VdsSoftShadowRaw";
        blurSecondary.Properties["DepthSource"] = "GuideChannel";
        blurSecondary.Properties["DepthChannel"] = "G";
        blurSecondary.Properties["AdaptiveStepChannel"] = "B";
        blurSecondary.Properties["AdaptiveStepScale"] = "1.50";
        blurSecondary.Properties["AdaptiveStepBias"] = "0.25";
        blurSecondary.Properties["StepScale"] = "1.25";
        blurSecondary.Properties["Strength"] = "1.0";
        blurSecondary.Properties["DepthThreshold"] = "0.0035";
        blurSecondary.Properties["NormalThreshold"] = "0.15";
        blurSecondary.Properties["PreserveAlpha"] = "True";

        var output = graph.AddNode(NodeKind.Output, 1860, 360);
        output.Properties["Pipeline"] = "ScenePostProcess";
        output.Properties["RenderTarget0Name"] = "VdsSoftShadowTemp";
        output.Properties["RenderTarget0Scale"] = "1.0";
        output.Properties["RenderTarget1Name"] = "VdsSoftShadowProcessed";
        output.Properties["RenderTarget1Scale"] = "1.0";

        Connect(graph, screenUv, "UV", sceneColor, "UV");
        Connect(graph, screenUv, "UV", rawLayer, "UV");
        Connect(graph, screenUv, "UV", blurPrimary, "UV");
        Connect(graph, screenUv, "UV", blurSecondary, "UV");

        Connect(graph, sceneColor, "Color", output, "Color");
        Connect(graph, blurPrimary, "Color", output, "CaptureColor");
        Connect(graph, blurSecondary, "Color", output, "CaptureSecondaryColor");

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowSoftToonLightMaterialSampleCore()
    {
        var graph = CreateVirtualDirectionalShadowToonLightMaterialSampleCore();

        AddFrame(graph, 30f, 660f, 1780f, 180f, "软阴影接回说明\n把 ScenePostProcess 模板输出的 VdsSoftShadowProcessed 读回 VDS 节点。\nProcessedShadowBlend 控制“原始 VDS 阴影”和“屏幕软阴影”之间的混合比例。", 0.52f, 0.38f, 0.20f);

        var toonLight = graph.Nodes.FirstOrDefault(node => node.Kind == NodeKind.VirtualDirectionalShadowLight);
        if (toonLight is not null)
        {
            ConfigureVirtualDirectionalShadowProcessedScreenShadow(toonLight, "VdsSoftShadowProcessed", "0.75", "R");
            toonLight.Properties["Softness"] = "0.14";
            toonLight.Properties["ShadowAAMode"] = "High";
        }

        return graph;
    }

    private static NodeGraph CreateVirtualDirectionalShadowSoftCharacterMaterialSampleCore()
    {
        var graph = CreateVirtualDirectionalShadowSoftToonLightMaterialSampleCore();

        AddFrame(graph, 30f, 880f, 1780f, 180f, "角色材质说明\n这张模板同时负责两件事：\n1. 正常显示角色材质\n2. 把角色自身的连续阴影写到 VdsSoftShadowRaw，供后处理模糊\n这样角色主体自阴影也能进入软阴影链。", 0.30f, 0.46f, 0.22f);

        var toonLight = graph.Nodes.FirstOrDefault(node => node.Kind == NodeKind.VirtualDirectionalShadowLight);
        var output = graph.Nodes.FirstOrDefault(node => node.Kind == NodeKind.Output);
        if (toonLight is null || output is null)
        {
            return graph;
        }

        output.Properties["RenderTarget0Name"] = string.Empty;
        output.Properties["RenderTarget0Scale"] = "1.0";
        output.Properties["RenderTarget1Name"] = "VdsSoftShadowRaw";
        output.Properties["RenderTarget1Scale"] = "1.0";

        var captureShadowMask = graph.AddNode(NodeKind.VirtualDirectionalShadowMask, 1460, 600);
        ConfigureVirtualDirectionalShadowMaskMaterialNode(captureShadowMask);

        var clipPosition = graph.AddNode(NodeKind.ClipSpacePosition, 1460, 760);
        var clipSplit = graph.AddNode(NodeKind.SplitColor, 1460, 930);
        var clipDepth = graph.AddNode(NodeKind.Divide, 1760, 930);
        clipDepth.Properties["Type"] = "Float1";

        var penumbraStrength = graph.AddNode(NodeKind.Min, 1760, 1090);
        penumbraStrength.Properties["Type"] = "Float1";

        var captureColor = graph.AddNode(NodeKind.ComposeColor, 2060, 760);
        var captureAlpha = graph.AddNode(NodeKind.Scalar, 2060, 930);
        captureAlpha.Properties["Value"] = "1.0";

        Connect(graph, clipPosition, "Value", clipSplit, "Value");
        Connect(graph, clipSplit, "B", clipDepth, "A");
        Connect(graph, clipSplit, "A", clipDepth, "B");
        Connect(graph, captureShadowMask, "RawLitFactor", penumbraStrength, "A");
        Connect(graph, captureShadowMask, "RawShadowMask", penumbraStrength, "B");
        Connect(graph, captureShadowMask, "RawShadowMask", captureColor, "R");
        Connect(graph, clipDepth, "Result", captureColor, "G");
        Connect(graph, penumbraStrength, "Result", captureColor, "B");
        Connect(graph, captureAlpha, "Value", captureColor, "A");
        Connect(graph, captureColor, "Color", output, "SecondaryColor");

        return graph;
    }
}



namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateKajiyaKayHairSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 60);

        var hairTangent = graph.AddNode(NodeKind.HairTangent, 260, 260);
        hairTangent.Properties["Source"] = "MainUV";
        hairTangent.Properties["StrandAxis"] = "V";
        hairTangent.Properties["Invert"] = "False";

        var specularColor = graph.AddNode(NodeKind.Color, 520, 300);
        specularColor.Properties["R"] = "1.0";
        specularColor.Properties["G"] = "0.92";
        specularColor.Properties["B"] = "0.78";
        specularColor.Properties["A"] = "1.0";

        var primaryShift = graph.AddNode(NodeKind.Scalar, 520, 420);
        primaryShift.Properties["Value"] = "0.08";
        var secondaryShift = graph.AddNode(NodeKind.Scalar, 520, 520);
        secondaryShift.Properties["Value"] = "0.16";

        var kajiyaKay = graph.AddNode(NodeKind.KajiyaKay, 840, 140);
        kajiyaKay.Properties["DiffuseStrength"] = "0.35";
        kajiyaKay.Properties["PrimaryShift"] = "0.08";
        kajiyaKay.Properties["SecondaryShift"] = "0.16";
        kajiyaKay.Properties["PrimaryExponent"] = "96.0";
        kajiyaKay.Properties["SecondaryExponent"] = "24.0";
        kajiyaKay.Properties["PrimaryStrength"] = "1.0";
        kajiyaKay.Properties["SecondaryStrength"] = "0.45";
        kajiyaKay.Properties["StrandAxis"] = "V";
        kajiyaKay.Properties["Invert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1180, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = hairTangent.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "Albedo",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = hairTangent.Id,
            SourcePin = "Value",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "Tangent",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specularColor.Id,
            SourcePin = "Color",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "SpecularColor",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = primaryShift.Id,
            SourcePin = "Value",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "PrimaryShift",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = secondaryShift.Id,
            SourcePin = "Value",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "SecondaryShift",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = kajiyaKay.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateKajiyaKayHairRingSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 60);
        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 260, 260);

        var worldDown = graph.AddNode(NodeKind.Float4Value, 520, 120);
        worldDown.Properties["X"] = "0.0";
        worldDown.Properties["Y"] = "-1.0";
        worldDown.Properties["Z"] = "0.0";
        worldDown.Properties["W"] = "0.0";

        var flowDot = graph.AddNode(NodeKind.Dot, 520, 280);
        var normalProjection = graph.AddNode(NodeKind.Multiply, 760, 240);
        normalProjection.Properties["Type"] = "Float4";
        var projectedFlow = graph.AddNode(NodeKind.Subtract, 980, 210);
        projectedFlow.Properties["Type"] = "Float4";
        var ringTangent = graph.AddNode(NodeKind.Normalize, 1220, 210);

        var specularColor = graph.AddNode(NodeKind.Color, 520, 420);
        specularColor.Properties["R"] = "1.0";
        specularColor.Properties["G"] = "0.92";
        specularColor.Properties["B"] = "0.78";
        specularColor.Properties["A"] = "1.0";

        var primaryShift = graph.AddNode(NodeKind.Scalar, 760, 420);
        primaryShift.Properties["Value"] = "0.03";
        var secondaryShift = graph.AddNode(NodeKind.Scalar, 760, 520);
        secondaryShift.Properties["Value"] = "0.10";

        var kajiyaKay = graph.AddNode(NodeKind.KajiyaKay, 1460, 140);
        kajiyaKay.Properties["DiffuseStrength"] = "0.25";
        kajiyaKay.Properties["PrimaryShift"] = "0.03";
        kajiyaKay.Properties["SecondaryShift"] = "0.10";
        kajiyaKay.Properties["PrimaryExponent"] = "128.0";
        kajiyaKay.Properties["SecondaryExponent"] = "28.0";
        kajiyaKay.Properties["PrimaryStrength"] = "1.15";
        kajiyaKay.Properties["SecondaryStrength"] = "0.35";
        kajiyaKay.Properties["StrandAxis"] = "V";
        kajiyaKay.Properties["Invert"] = "False";

        var output = graph.AddNode(NodeKind.Output, 1800, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = flowDot.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldDown.Id,
            SourcePin = "Value",
            TargetNodeId = flowDot.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = normalProjection.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = flowDot.Id,
            SourcePin = "Result",
            TargetNodeId = normalProjection.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldDown.Id,
            SourcePin = "Value",
            TargetNodeId = projectedFlow.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = normalProjection.Id,
            SourcePin = "Result",
            TargetNodeId = projectedFlow.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = projectedFlow.Id,
            SourcePin = "Result",
            TargetNodeId = ringTangent.Id,
            TargetPin = "Value",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "Albedo",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ringTangent.Id,
            SourcePin = "Result",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "Tangent",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specularColor.Id,
            SourcePin = "Color",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "SpecularColor",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = primaryShift.Id,
            SourcePin = "Value",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "PrimaryShift",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = secondaryShift.Id,
            SourcePin = "Value",
            TargetNodeId = kajiyaKay.Id,
            TargetPin = "SecondaryShift",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = kajiyaKay.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateBrdfLightingSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 240, 60);
        var normalMap = graph.AddNode(NodeKind.NormalMap, 240, 260);
        normalMap.Properties["ResourceName"] = "normal.png";

        var roughness = graph.AddNode(NodeKind.Scalar, 520, 120);
        roughness.Properties["Value"] = "0.5";
        var metallic = graph.AddNode(NodeKind.Scalar, 520, 220);
        metallic.Properties["Value"] = "0.0";
        var specular = graph.AddNode(NodeKind.Scalar, 520, 320);
        specular.Properties["Value"] = "0.5";

        var brdf = graph.AddNode(NodeKind.BRDFLighting, 860, 140);
        brdf.Properties["Roughness"] = "0.5";
        brdf.Properties["Metallic"] = "0.0";
        brdf.Properties["Specular"] = "0.5";
        brdf.Properties["Occlusion"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 1220, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = normalMap.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = brdf.Id,
            TargetPin = "Albedo",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = normalMap.Id,
            SourcePin = "Normal",
            TargetNodeId = brdf.Id,
            TargetPin = "Normal",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = roughness.Id,
            SourcePin = "Value",
            TargetNodeId = brdf.Id,
            TargetPin = "Roughness",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = metallic.Id,
            SourcePin = "Value",
            TargetNodeId = brdf.Id,
            TargetPin = "Metallic",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specular.Id,
            SourcePin = "Value",
            TargetNodeId = brdf.Id,
            TargetPin = "Specular",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = brdf.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateWetnessSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 64);
        var lighting = graph.AddNode(NodeKind.BasicLighting, 540, 100);
        lighting.Properties["UseSpecular"] = "1.0";
        lighting.Properties["UseToon"] = "0.0";

        var wetness = graph.AddNode(NodeKind.Scalar, 260, 260);
        wetness.Properties["Value"] = "0.9";

        var porosity = graph.AddNode(NodeKind.Scalar, 260, 360);
        porosity.Properties["Value"] = "0.8";

        var wetSurface = graph.AddNode(NodeKind.Wetness, 860, 140);
        wetSurface.Properties["DarkenStrength"] = "0.75";
        wetSurface.Properties["SpecularStrength"] = "1.0";
        wetSurface.Properties["MinPower"] = "24.0";
        wetSurface.Properties["MaxPower"] = "128.0";
        wetSurface.Properties["FresnelStrength"] = "0.15";

        var output = graph.AddNode(NodeKind.Output, 1180, 180);

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
            TargetNodeId = lighting.Id,
            TargetPin = "Albedo",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = lighting.Id,
            SourcePin = "Color",
            TargetNodeId = wetSurface.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = wetness.Id,
            SourcePin = "Value",
            TargetNodeId = wetSurface.Id,
            TargetPin = "Wetness",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = porosity.Id,
            SourcePin = "Value",
            TargetNodeId = wetSurface.Id,
            TargetPin = "Porosity",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = wetSurface.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateBasicLightingSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var uvTransform = graph.AddNode(NodeKind.UvTransform, 260, 64);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 480, 64);
        var lighting = graph.AddNode(NodeKind.BasicLighting, 760, 110);
        lighting.Properties["UseSpecular"] = "1.0";
        lighting.Properties["UseToon"] = "0.0";
        var output = graph.AddNode(NodeKind.Output, 1060, 190);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = uvTransform.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = uvTransform.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = lighting.Id,
            TargetPin = "Albedo",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = lighting.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateVertexChannelsSampleCore()
    {
        var graph = new NodeGraph();

        var uvChannel = graph.AddNode(NodeKind.TexCoord, 60, 80);
        uvChannel.Properties["Source"] = "AddUV1";

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 340, 80);
        materialTexture.Properties["ColorSpace"] = "Color";

        var aoChannel = graph.AddNode(NodeKind.VertexChannel, 60, 320);
        aoChannel.Properties["Source"] = "AddUV3";

        var aoMask = graph.AddNode(NodeKind.ComponentMask, 340, 320);
        aoMask.Properties["Channels"] = "R";

        var tintChannel = graph.AddNode(NodeKind.VertexChannel, 60, 560);
        tintChannel.Properties["Source"] = "AddUV4";

        var tintMask = graph.AddNode(NodeKind.ComponentMask, 340, 560);
        tintMask.Properties["Channels"] = "A";

        var multiply = graph.AddNode(NodeKind.Multiply, 660, 180);
        multiply.Properties["Type"] = "Float4";

        var blend = graph.AddNode(NodeKind.Lerp, 980, 220);
        blend.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1280, 220);
        output.Properties["AlphaMode"] = "ColorAlpha";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = uvChannel.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = aoChannel.Id,
            SourcePin = "Value",
            TargetNodeId = aoMask.Id,
            TargetPin = "Value",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = tintChannel.Id,
            SourcePin = "Value",
            TargetNodeId = tintMask.Id,
            TargetPin = "Value",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = aoMask.Id,
            SourcePin = "Result",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = blend.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = tintChannel.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = tintMask.Id,
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

    private static NodeGraph CreateSelfShadowLightingSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 64);
        materialTexture.Properties["ColorSpace"] = "Color";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 260, 260);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 260, 420);
        var selfShadowLighting = graph.AddNode(NodeKind.SelfShadowLighting, 560, 260);
        selfShadowLighting.Properties["Bias"] = "0.3";
        selfShadowLighting.Properties["Scale"] = "1500.0";
        selfShadowLighting.Properties["Power"] = "32.0";

        var diffuseMultiply = graph.AddNode(NodeKind.Multiply, 900, 120);
        diffuseMultiply.Properties["Type"] = "Float3";
        var specularMultiply = graph.AddNode(NodeKind.Multiply, 900, 360);
        specularMultiply.Properties["Type"] = "Float3";
        var add = graph.AddNode(NodeKind.Add, 1180, 220);
        add.Properties["Type"] = "Float3";

        var specularColor = graph.AddNode(NodeKind.MaterialSpecularColor, 560, 520);
        var output = graph.AddNode(NodeKind.Output, 1440, 220);

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, worldNormal, "Value", selfShadowLighting, "Normal");
        Connect(graph, viewDirection, "Value", selfShadowLighting, "ViewDir");

        Connect(graph, materialTexture, "RGB", diffuseMultiply, "A");
        Connect(graph, selfShadowLighting, "HalfLambert", diffuseMultiply, "B");

        Connect(graph, specularColor, "Value", specularMultiply, "A");
        Connect(graph, selfShadowLighting, "BlinnPhong", specularMultiply, "B");

        Connect(graph, diffuseMultiply, "Result", add, "A");
        Connect(graph, specularMultiply, "Result", add, "B");
        Connect(graph, add, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateSelfShadowLambertTemplateCore()
    {
        return CreateSelfShadowDiffuseTemplateCore("Lambert");
    }

    private static NodeGraph CreateSelfShadowHalfLambertTemplateCore()
    {
        return CreateSelfShadowDiffuseTemplateCore("HalfLambert");
    }

    private static NodeGraph CreateSelfShadowDiffuseTemplateCore(string mode)
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 40, 220);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 64);
        materialTexture.Properties["ColorSpace"] = "Color";
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 260, 180);
        baseColorMultiply.Properties["Type"] = "Float4";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 260, 300);
        var vdsLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 560, 80);
        ConfigureVirtualDirectionalShadowLightNode(vdsLight, mode);
        var diffuseLighting = graph.AddNode(
            string.Equals(mode, "Lambert", StringComparison.OrdinalIgnoreCase) ? NodeKind.Lambert : NodeKind.HalfLambert,
            560,
            300);
        var lightMask = graph.AddNode(NodeKind.ComposeColor, 1180, 180);
        var lightMaskAlpha = graph.AddNode(NodeKind.Scalar, 1180, 340);
        lightMaskAlpha.Properties["Value"] = "1.0";
        var shadowMultiply = graph.AddNode(NodeKind.Multiply, 900, 220);
        shadowMultiply.Properties["Type"] = "Float1";
        var finalMultiply = graph.AddNode(NodeKind.Multiply, 1460, 180);
        finalMultiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1740, 200);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");
        Connect(graph, worldNormal, "Value", diffuseLighting, "Normal");
        Connect(graph, vdsLight, "LightDir", diffuseLighting, "LightDir");

        Connect(graph, diffuseLighting, "Result", shadowMultiply, "A");
        Connect(graph, vdsLight, "ShadowVisibility", shadowMultiply, "B");

        Connect(graph, shadowMultiply, "Result", lightMask, "R");
        Connect(graph, shadowMultiply, "Result", lightMask, "G");
        Connect(graph, shadowMultiply, "Result", lightMask, "B");
        Connect(graph, lightMaskAlpha, "Value", lightMask, "A");

        Connect(graph, baseColorMultiply, "Result", finalMultiply, "A");
        Connect(graph, lightMask, "Color", finalMultiply, "B");
        Connect(graph, finalMultiply, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateSelfShadowBlinnPhongTemplateCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 40, 220);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 64);
        materialTexture.Properties["ColorSpace"] = "Color";
        var baseColorMultiply = graph.AddNode(NodeKind.Multiply, 260, 180);
        baseColorMultiply.Properties["Type"] = "Float4";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 260, 260);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 260, 420);
        var vdsLight = graph.AddNode(NodeKind.VirtualDirectionalShadowLight, 560, 80);
        ConfigureVirtualDirectionalShadowLightNode(vdsLight, "HalfLambert");

        var materialSpecular = graph.AddNode(NodeKind.MaterialSpecularColor, 900, 500);
        var shadowMultiply = graph.AddNode(NodeKind.Multiply, 900, 280);
        shadowMultiply.Properties["Type"] = "Float1";
        var blinnPhong = graph.AddNode(NodeKind.BlinnPhong, 900, 120);
        blinnPhong.Properties["Power"] = "32.0";
        var specularMultiply = graph.AddNode(NodeKind.Multiply, 1180, 360);
        specularMultiply.Properties["Type"] = "Float3";
        var baseSplit = graph.AddNode(NodeKind.SplitColor, 1180, 180);
        var finalColor = graph.AddNode(NodeKind.ComposeColor, 1460, 280);

        var output = graph.AddNode(NodeKind.Output, 1740, 280);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialDiffuse, "Value", baseColorMultiply, "A");
        Connect(graph, materialTexture, "Color", baseColorMultiply, "B");
        Connect(graph, worldNormal, "Value", blinnPhong, "Normal");
        Connect(graph, viewDirection, "Value", blinnPhong, "ViewDir");
        Connect(graph, vdsLight, "LightDir", blinnPhong, "LightDir");

        Connect(graph, blinnPhong, "Result", shadowMultiply, "A");
        Connect(graph, vdsLight, "ShadowVisibility", shadowMultiply, "B");

        Connect(graph, materialSpecular, "Value", specularMultiply, "A");
        Connect(graph, shadowMultiply, "Result", specularMultiply, "B");

        Connect(graph, baseColorMultiply, "Result", baseSplit, "Value");
        Connect(graph, specularMultiply, "Result", finalColor, "R");
        Connect(graph, specularMultiply, "Result", finalColor, "G");
        Connect(graph, specularMultiply, "Result", finalColor, "B");
        Connect(graph, baseSplit, "A", finalColor, "A");

        Connect(graph, finalColor, "Color", output, "Color");

        return graph;
    }

    private static void ConfigureVirtualDirectionalShadowLightNode(GraphNode node, string diffuseMode)
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
        node.Properties["ShadowStrength"] = "1.0";
    }

    private static NodeGraph CreateSelfShadowSampleCore()
    {
        var graph = new NodeGraph();

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 72);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 64);
        materialTexture.Properties["ColorSpace"] = "Color";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 260, 260);
        var selfShadow = graph.AddNode(NodeKind.SelfShadowFactor, 520, 260);
        selfShadow.Properties["Bias"] = "0.3";
        selfShadow.Properties["Scale"] = "1500.0";

        var diffuseShadow = graph.AddNode(NodeKind.DiffuseShadow, 780, 220);
        diffuseShadow.Properties["DiffuseMode"] = "HalfLambert";

        var multiply = graph.AddNode(NodeKind.Multiply, 1060, 120);
        multiply.Properties["Type"] = "Float3";

        var output = graph.AddNode(NodeKind.Output, 1320, 140);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = diffuseShadow.Id,
            TargetPin = "Normal",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = selfShadow.Id,
            SourcePin = "Result",
            TargetNodeId = diffuseShadow.Id,
            TargetPin = "Shadow",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "RGB",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuseShadow.Id,
            SourcePin = "Result",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }
}

namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateControllerLightSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 140);
        var controllerLight = graph.AddNode(NodeKind.ControllerLightDirection, 380, 140);
        controllerLight.Properties["Name"] = "LightController.pmx";
        controllerLight.Properties["Item"] = "XYZ";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 380, 320);
        var ndotl = graph.AddNode(NodeKind.Dot, 700, 230);
        var saturate = graph.AddNode(NodeKind.Saturate, 980, 230);
        var compose = graph.AddNode(NodeKind.ComposeColor, 1220, 230);
        var multiply = graph.AddNode(NodeKind.Multiply, 1480, 180);
        var output = graph.AddNode(NodeKind.Output, 1740, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = ndotl.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = controllerLight.Id,
            SourcePin = "Value",
            TargetNodeId = ndotl.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndotl.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "R",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndotl.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "G",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndotl.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "B",
        });

        var alpha = graph.AddNode(NodeKind.Scalar, 1220, 390);
        alpha.Properties["Value"] = "1.0";
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alpha.Id,
            SourcePin = "Value",
            TargetNodeId = compose.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = compose.Id,
            SourcePin = "Color",
            TargetNodeId = saturate.Id,
            TargetPin = "Color",
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
            SourceNodeId = saturate.Id,
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

    private static NodeGraph CreateControllerRotationSampleCore()
    {
        var graph = new NodeGraph();

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 140);
        var controllerRotation = graph.AddNode(NodeKind.ControlObjectRotation, 360, 120);
        controllerRotation.Properties["Name"] = "LightController.pmx";
        controllerRotation.Properties["Item"] = "Rxyz";

        var rotationDirection = graph.AddNode(NodeKind.EulerToDirection, 650, 120);
        rotationDirection.Properties["BaseX"] = "0.0";
        rotationDirection.Properties["BaseY"] = "0.0";
        rotationDirection.Properties["BaseZ"] = "1.0";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 650, 300);
        var ndot = graph.AddNode(NodeKind.Dot, 960, 200);
        var compose = graph.AddNode(NodeKind.ComposeColor, 1220, 200);
        var alpha = graph.AddNode(NodeKind.Scalar, 1220, 380);
        alpha.Properties["Value"] = "1.0";

        var multiply = graph.AddNode(NodeKind.Multiply, 1480, 180);
        var output = graph.AddNode(NodeKind.Output, 1750, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = controllerRotation.Id,
            SourcePin = "Value",
            TargetNodeId = rotationDirection.Id,
            TargetPin = "Rotation",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = ndot.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rotationDirection.Id,
            SourcePin = "Value",
            TargetNodeId = ndot.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndot.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "R",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndot.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "G",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ndot.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alpha.Id,
            SourcePin = "Value",
            TargetNodeId = compose.Id,
            TargetPin = "A",
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
            SourceNodeId = compose.Id,
            SourcePin = "Color",
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

    private static NodeGraph CreateStylizedControllerLightSampleCore()
    {
        var graph = new NodeGraph();

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 60, 180);
        var controllerLight = graph.AddNode(NodeKind.ControlObjectTransformDirection, 60, 420);
        controllerLight.Properties["Name"] = "LightController.pmx";
        controllerLight.Properties["Axis"] = "Z";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 360, 180);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 360, 460);

        var halfLambert = graph.AddNode(NodeKind.HalfLambert, 660, 180);
        var diffuseBand = graph.AddNode(NodeKind.SmoothStep, 960, 180);
        diffuseBand.Properties["Min"] = "0.38";
        diffuseBand.Properties["Max"] = "0.78";

        var ambientColor = graph.AddNode(NodeKind.MaterialAmbient, 960, 360);
        var toonColor = graph.AddNode(NodeKind.MaterialToonColor, 960, 470);
        var diffuseColor = graph.AddNode(NodeKind.Lerp, 1260, 360);
        var diffuseMultiply = graph.AddNode(NodeKind.Multiply, 1560, 250);

        var blinnPhong = graph.AddNode(NodeKind.BlinnPhong, 660, 500);
        blinnPhong.Properties["Power"] = "40.0";
        var specBand = graph.AddNode(NodeKind.SmoothStep, 960, 610);
        specBand.Properties["Min"] = "0.72";
        specBand.Properties["Max"] = "0.92";
        var specularColor = graph.AddNode(NodeKind.MaterialSpecularColor, 1260, 610);
        var specularMultiply = graph.AddNode(NodeKind.Multiply, 1560, 560);

        var add = graph.AddNode(NodeKind.Add, 1840, 360);
        var alpha = graph.AddNode(NodeKind.Scalar, 1840, 560);
        alpha.Properties["Value"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 2120, 360);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = controllerLight.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "LightDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = halfLambert.Id,
            SourcePin = "Result",
            TargetNodeId = diffuseBand.Id,
            TargetPin = "X",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ambientColor.Id,
            SourcePin = "Value",
            TargetNodeId = diffuseColor.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = toonColor.Id,
            SourcePin = "Value",
            TargetNodeId = diffuseColor.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuseBand.Id,
            SourcePin = "Result",
            TargetNodeId = diffuseColor.Id,
            TargetPin = "T",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = diffuseMultiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuseColor.Id,
            SourcePin = "Result",
            TargetNodeId = diffuseMultiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = blinnPhong.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = controllerLight.Id,
            SourcePin = "Value",
            TargetNodeId = blinnPhong.Id,
            TargetPin = "LightDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = viewDirection.Id,
            SourcePin = "Value",
            TargetNodeId = blinnPhong.Id,
            TargetPin = "ViewDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blinnPhong.Id,
            SourcePin = "Result",
            TargetNodeId = specBand.Id,
            TargetPin = "X",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specularColor.Id,
            SourcePin = "Value",
            TargetNodeId = specularMultiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specBand.Id,
            SourcePin = "Result",
            TargetNodeId = specularMultiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuseMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = add.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = specularMultiply.Id,
            SourcePin = "Result",
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
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alpha.Id,
            SourcePin = "Value",
            TargetNodeId = output.Id,
            TargetPin = "Alpha",
        });

        return graph;
    }

    private static NodeGraph CreateBoneDrivenLightSampleCore()
    {
        var graph = new NodeGraph();

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 160);
        var sphereMap = graph.AddNode(NodeKind.MaterialSphereMap, 80, 360);
        var bonePosition = graph.AddNode(NodeKind.ControlObjectBonePosition, 80, 580);
        bonePosition.Properties["Name"] = "LightController.pmx";
        bonePosition.Properties["Item"] = "Direction";
        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 400, 180);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 400, 420);
        var pointLightDir = graph.AddNode(NodeKind.PointLightDirection, 400, 600);
        var halfLambert = graph.AddNode(NodeKind.HalfLambert, 720, 180);
        var softness = graph.AddNode(NodeKind.ShadowSoftness, 1020, 180);
        softness.Properties["Threshold"] = "0.58";
        softness.Properties["Softness"] = "0.08";
        var toonRamp = graph.AddNode(NodeKind.ToonRampSample, 1320, 180);
        var diffuseMultiply = graph.AddNode(NodeKind.Multiply, 1620, 180);
        diffuseMultiply.Properties["Type"] = "Float3";

        var rimLight = graph.AddNode(NodeKind.RimLight, 1020, 460);
        rimLight.Properties["Power"] = "2.8";
        rimLight.Properties["Intensity"] = "0.45";
        var rimThreshold = graph.AddNode(NodeKind.ShadowThreshold, 1320, 500);
        rimThreshold.Properties["Threshold"] = "0.68";
        var matCapMix = graph.AddNode(NodeKind.MatCapMix, 1920, 260);

        var alpha = graph.AddNode(NodeKind.Scalar, 2200, 520);
        alpha.Properties["Value"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 2480, 300);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bonePosition.Id,
            SourcePin = "Value",
            TargetNodeId = pointLightDir.Id,
            TargetPin = "LightPos",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = pointLightDir.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "LightDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = halfLambert.Id,
            SourcePin = "Result",
            TargetNodeId = softness.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = softness.Id,
            SourcePin = "Result",
            TargetNodeId = toonRamp.Id,
            TargetPin = "Factor",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "RGB",
            TargetNodeId = diffuseMultiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = toonRamp.Id,
            SourcePin = "Color",
            TargetNodeId = diffuseMultiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = rimLight.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = viewDirection.Id,
            SourcePin = "Value",
            TargetNodeId = rimLight.Id,
            TargetPin = "ViewDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimLight.Id,
            SourcePin = "Result",
            TargetNodeId = rimThreshold.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimThreshold.Id,
            SourcePin = "Result",
            TargetNodeId = matCapMix.Id,
            TargetPin = "Factor",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuseMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = matCapMix.Id,
            TargetPin = "Base",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = sphereMap.Id,
            SourcePin = "Color",
            TargetNodeId = matCapMix.Id,
            TargetPin = "MatCap",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = matCapMix.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alpha.Id,
            SourcePin = "Value",
            TargetNodeId = output.Id,
            TargetPin = "Alpha",
        });

        return graph;
    }

    private static NodeGraph CreateControllerDirectionTemplateSampleCore()
    {
        var graph = new NodeGraph();

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 180);
        var angleDirection = graph.AddNode(NodeKind.ControlObjectAngleDirection, 80, 420);
        angleDirection.Properties["Name"] = "LightController.pmx";
        angleDirection.Properties["UpItem"] = "ShadowUp";
        angleDirection.Properties["DownItem"] = "ShadowBottom";
        angleDirection.Properties["LeftItem"] = "ShadowLeft";
        angleDirection.Properties["RightItem"] = "ShadowRight";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 400, 180);
        var halfLambert = graph.AddNode(NodeKind.HalfLambert, 720, 180);
        var smooth = graph.AddNode(NodeKind.SmoothStep, 1020, 180);
        smooth.Properties["Min"] = "0.42";
        smooth.Properties["Max"] = "0.78";

        var ambient = graph.AddNode(NodeKind.MaterialAmbient, 1320, 120);
        var diffuse = graph.AddNode(NodeKind.MaterialDiffuse, 1320, 300);
        var blend = graph.AddNode(NodeKind.Lerp, 1620, 180);
        var multiply = graph.AddNode(NodeKind.Multiply, 1920, 180);
        var alpha = graph.AddNode(NodeKind.Scalar, 1920, 360);
        alpha.Properties["Value"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 2200, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldNormal.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = angleDirection.Id,
            SourcePin = "Value",
            TargetNodeId = halfLambert.Id,
            TargetPin = "LightDir",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = halfLambert.Id,
            SourcePin = "Result",
            TargetNodeId = smooth.Id,
            TargetPin = "X",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = ambient.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = diffuse.Id,
            SourcePin = "Value",
            TargetNodeId = blend.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = smooth.Id,
            SourcePin = "Result",
            TargetNodeId = blend.Id,
            TargetPin = "T",
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
            SourceNodeId = blend.Id,
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
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alpha.Id,
            SourcePin = "Value",
            TargetNodeId = output.Id,
            TargetPin = "Alpha",
        });

        return graph;
    }

}

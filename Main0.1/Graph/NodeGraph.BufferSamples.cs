namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    public static NodeGraph CreateRimDepthBufferSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 180);
        var rimDepth = graph.AddNode(NodeKind.RimDepthBuffer, 420, 170);
        rimDepth.Properties["AlphaThreshold"] = "0.3";
        rimDepth.Properties["DepthScale"] = "1000.0";
        var output = graph.AddNode(NodeKind.Output, 820, 200);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = rimDepth.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimDepth.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateZBufferCompatibilitySampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 60, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 300, 120);
        materialTexture.Properties["ColorSpace"] = "Color";
        materialTexture.Properties["FilterMode"] = "Point";
        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 60, 500);
        var splitMaterial = graph.AddNode(NodeKind.SplitColor, 300, 500);

        var clipPosition = graph.AddNode(NodeKind.ClipSpacePosition, 60, 320);
        var split = graph.AddNode(NodeKind.SplitXYZW, 320, 320);
        var negate = graph.AddNode(NodeKind.Multiply, 560, 280);
        negate.Properties["Type"] = "Float1";
        var minusOne = graph.AddNode(NodeKind.Scalar, 320, 440);
        minusOne.Properties["Value"] = "-1.0";

        var remap = graph.AddNode(NodeKind.Remap, 800, 300);
        remap.Properties["InMin"] = "0.0";
        remap.Properties["InMax"] = "100.0";
        remap.Properties["OutMin"] = "0.0";
        remap.Properties["OutMax"] = "1.0";
        remap.Properties["Mode"] = "Linear";
        remap.Properties["Clamp"] = "True";

        var compose = graph.AddNode(NodeKind.ComposeColor, 1040, 260);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 1040, 500);
        alphaMultiply.Properties["Type"] = "Float1";
        var output = graph.AddNode(NodeKind.Output, 1300, 240);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.01";
        output.Properties["TechniqueStyle"] = "LegacyZBuffer";
        output.Properties["ShadowTechniqueMode"] = "Empty";
        output.Properties["ZplotTechniqueMode"] = "Empty";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialDiffuse.Id,
            SourcePin = "Value",
            TargetNodeId = splitMaterial.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = clipPosition.Id,
            SourcePin = "Value",
            TargetNodeId = split.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = split.Id,
            SourcePin = "W",
            TargetNodeId = negate.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = minusOne.Id,
            SourcePin = "Value",
            TargetNodeId = negate.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = negate.Id,
            SourcePin = "Result",
            TargetNodeId = remap.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = remap.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "R",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = remap.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "G",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = remap.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = alphaMultiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = splitMaterial.Id,
            SourcePin = "A",
            TargetNodeId = alphaMultiply.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alphaMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = compose.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alphaMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Alpha",
        });

        return graph;
    }

    public static NodeGraph CreateLinearDepthBufferSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 60, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 300, 120);
        materialTexture.Properties["ColorSpace"] = "Color";
        materialTexture.Properties["FilterMode"] = "Point";

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 60, 500);
        var splitMaterial = graph.AddNode(NodeKind.SplitColor, 300, 500);

        var worldPosition = graph.AddNode(NodeKind.WorldPosition, 60, 280);
        var cameraPosition = graph.AddNode(NodeKind.CameraPosition, 60, 380);
        var distance = graph.AddNode(NodeKind.Distance, 320, 320);
        var remap = graph.AddNode(NodeKind.Remap, 620, 320);
        remap.Properties["InMin"] = "0.0";
        remap.Properties["InMax"] = "100.0";
        remap.Properties["OutMin"] = "0.0";
        remap.Properties["OutMax"] = "1.0";
        remap.Properties["Mode"] = "Linear";
        remap.Properties["Clamp"] = "True";

        var oneMinus = graph.AddNode(NodeKind.OneMinus, 900, 320);
        oneMinus.Properties["Type"] = "Float1";

        var compose = graph.AddNode(NodeKind.ComposeColor, 1140, 260);
        var alphaMultiply = graph.AddNode(NodeKind.Multiply, 1140, 500);
        alphaMultiply.Properties["Type"] = "Float1";
        var output = graph.AddNode(NodeKind.Output, 1400, 240);
        output.Properties["AlphaMode"] = "ColorAlpha";
        output.Properties["AlphaClipThreshold"] = "0.01";
        output.Properties["ShadowTechniqueMode"] = "Empty";
        output.Properties["ZplotTechniqueMode"] = "Empty";

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialDiffuse.Id,
            SourcePin = "Value",
            TargetNodeId = splitMaterial.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldPosition.Id,
            SourcePin = "Value",
            TargetNodeId = distance.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = cameraPosition.Id,
            SourcePin = "Value",
            TargetNodeId = distance.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = distance.Id,
            SourcePin = "Result",
            TargetNodeId = remap.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = remap.Id,
            SourcePin = "Result",
            TargetNodeId = oneMinus.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = oneMinus.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "R",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = oneMinus.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "G",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = oneMinus.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = alphaMultiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = splitMaterial.Id,
            SourcePin = "A",
            TargetNodeId = alphaMultiply.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alphaMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = compose.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = compose.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = alphaMultiply.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Alpha",
        });

        return graph;
    }

    public static NodeGraph CreateRimMaskBufferSample()
    {
        return CreateRimMaskBufferOnSample();
    }

    public static NodeGraph CreateRimMaskBufferOnSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 300);
        var rim = graph.AddNode(NodeKind.Scalar, 80, 80);
        rim.Properties["Value"] = "1.0";
        var block = graph.AddNode(NodeKind.Scalar, 80, 160);
        block.Properties["Value"] = "1.0";
        var rimMask = graph.AddNode(NodeKind.RimMaskBuffer, 420, 180);
        rimMask.Properties["AlphaThreshold"] = "0.65";
        rimMask.Properties["RimSize"] = "1.0";
        rimMask.Properties["UsePhongMask"] = "False";
        var output = graph.AddNode(NodeKind.Output, 820, 220);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rim.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Rim",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = block.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Block",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = rimMask.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimMask.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateRimMaskBufferPhongSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 300);
        var rim = graph.AddNode(NodeKind.Scalar, 80, 80);
        rim.Properties["Value"] = "1.0";
        var block = graph.AddNode(NodeKind.Scalar, 80, 160);
        block.Properties["Value"] = "1.0";
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 240);
        var lightDirection = graph.AddNode(NodeKind.ControlObjectBoneDirection, 80, 380);
        lightDirection.Properties["Name"] = "LightController.pmx";
        lightDirection.Properties["Item"] = "Direction";
        lightDirection.Properties["Axis"] = "Z";
        var dot = graph.AddNode(NodeKind.Dot, 360, 300);
        var oneMinus = graph.AddNode(NodeKind.OneMinus, 620, 300);
        oneMinus.Properties["Type"] = "Float1";
        var rimMask = graph.AddNode(NodeKind.RimMaskBuffer, 420, 180);
        rimMask.Properties["AlphaThreshold"] = "0.65";
        rimMask.Properties["RimSize"] = "1.0";
        rimMask.Properties["UsePhongMask"] = "True";
        var output = graph.AddNode(NodeKind.Output, 820, 220);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rim.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Rim",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = block.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Block",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localNormal.Id,
            SourcePin = "Value",
            TargetNodeId = dot.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = lightDirection.Id,
            SourcePin = "Value",
            TargetNodeId = dot.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = dot.Id,
            SourcePin = "Result",
            TargetNodeId = oneMinus.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = oneMinus.Id,
            SourcePin = "Result",
            TargetNodeId = rimMask.Id,
            TargetPin = "PhongMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = rimMask.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimMask.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    public static NodeGraph CreateRimMaskBufferOffSample()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.BufferPass,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 80, 300);
        var rim = graph.AddNode(NodeKind.Scalar, 80, 80);
        rim.Properties["Value"] = "0.0";
        var block = graph.AddNode(NodeKind.Scalar, 80, 160);
        block.Properties["Value"] = "0.0";
        var rimMask = graph.AddNode(NodeKind.RimMaskBuffer, 420, 180);
        rimMask.Properties["AlphaThreshold"] = "0.65";
        rimMask.Properties["RimSize"] = "0.0";
        rimMask.Properties["UsePhongMask"] = "False";
        var output = graph.AddNode(NodeKind.Output, 820, 220);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rim.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Rim",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = block.Id,
            SourcePin = "Value",
            TargetNodeId = rimMask.Id,
            TargetPin = "Block",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "A",
            TargetNodeId = rimMask.Id,
            TargetPin = "AlphaMask",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = rimMask.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }
}

namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateScreenSpaceRimManualPostProcessSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ScenePostProcess,
        };

        var screenUv = graph.AddNode(NodeKind.ScreenUv, 80, 120);

        var sceneColor = graph.AddNode(NodeKind.SceneColor, 360, 60);
        var rimDepthCenter = graph.AddNode(NodeKind.OffscreenBufferSample, 360, 230);
        rimDepthCenter.Properties["BufferName"] = "Na_RimDepth";
        rimDepthCenter.Properties["Description"] = "Depth and normal buffer";
        rimDepthCenter.Properties["Format"] = "A16B16G16R16F";
        rimDepthCenter.Properties["FilterMode"] = "Linear";

        var rimMaskCenter = graph.AddNode(NodeKind.OffscreenBufferSample, 360, 460);
        rimMaskCenter.Properties["BufferName"] = "Na_RimMask";
        rimMaskCenter.Properties["Description"] = "Mask buffer";
        rimMaskCenter.Properties["Format"] = "D3DFMT_A8R8G8B8";
        rimMaskCenter.Properties["FilterMode"] = "Point";

        var normalMask = graph.AddNode(NodeKind.ComponentMask, 700, 230);
        normalMask.Properties["Channels"] = "RG";
        var depthMask = graph.AddNode(NodeKind.ComponentMask, 700, 350);
        depthMask.Properties["Channels"] = "B";
        var outerMaskChannel = graph.AddNode(NodeKind.ComponentMask, 700, 620);
        outerMaskChannel.Properties["Channels"] = "R";
        var innerMaskChannel = graph.AddNode(NodeKind.ComponentMask, 700, 740);
        innerMaskChannel.Properties["Channels"] = "G";

        var vec2Two = graph.AddNode(NodeKind.Float2Value, 980, 150);
        vec2Two.Properties["X"] = "2.0";
        vec2Two.Properties["Y"] = "2.0";
        var vec2One = graph.AddNode(NodeKind.Float2Value, 980, 260);
        vec2One.Properties["X"] = "1.0";
        vec2One.Properties["Y"] = "1.0";
        var mulNormal = graph.AddNode(NodeKind.Multiply, 1240, 210);
        mulNormal.Properties["Type"] = "Float2";
        var subNormal = graph.AddNode(NodeKind.Subtract, 1500, 210);
        subNormal.Properties["Type"] = "Float2";
        var splitNormal = graph.AddNode(NodeKind.SplitXY, 1760, 210);
        var zero = graph.AddNode(NodeKind.Scalar, 1760, 360);
        zero.Properties["Value"] = "0.0";
        var directionX = graph.AddNode(NodeKind.MergeXYZW, 2020, 160);
        var directionY = graph.AddNode(NodeKind.MergeXYZW, 2020, 310);
        var widthX = graph.AddNode(NodeKind.Scalar, 2020, 470);
        widthX.Properties["Value"] = "1.3";
        var widthY = graph.AddNode(NodeKind.Scalar, 2020, 550);
        widthY.Properties["Value"] = "1.3";

        var offsetUvX = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 140);
        offsetUvX.Properties["Scale"] = "0.005";
        offsetUvX.Properties["DepthPower"] = "0.75";
        offsetUvX.Properties["SizeX"] = "1.0";
        offsetUvX.Properties["SizeY"] = "1.0";

        var offsetUvY = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 360);
        offsetUvY.Properties["Scale"] = "0.005";
        offsetUvY.Properties["DepthPower"] = "0.75";
        offsetUvY.Properties["SizeX"] = "1.0";
        offsetUvY.Properties["SizeY"] = "1.0";

        var rimDepthOffsetX = graph.AddNode(NodeKind.OffscreenBufferSample, 2560, 140);
        rimDepthOffsetX.Properties["BufferName"] = "Na_RimDepth";
        rimDepthOffsetX.Properties["Description"] = "Depth and normal buffer";
        rimDepthOffsetX.Properties["Format"] = "A16B16G16R16F";
        rimDepthOffsetX.Properties["FilterMode"] = "Linear";

        var rimDepthOffsetY = graph.AddNode(NodeKind.OffscreenBufferSample, 2560, 360);
        rimDepthOffsetY.Properties["BufferName"] = "Na_RimDepth";
        rimDepthOffsetY.Properties["Description"] = "Depth and normal buffer";
        rimDepthOffsetY.Properties["Format"] = "A16B16G16R16F";
        rimDepthOffsetY.Properties["FilterMode"] = "Linear";

        var depthEdgeX = graph.AddNode(NodeKind.DepthEdgeDetect, 2860, 80);
        depthEdgeX.Properties["DepthChannel"] = "B";
        depthEdgeX.Properties["EdgeMode"] = "BehindOnly";
        depthEdgeX.Properties["Scale"] = "300.0";
        depthEdgeX.Properties["Threshold"] = "0.0";
        depthEdgeX.Properties["Power"] = "1.0";
        var depthEdgeY = graph.AddNode(NodeKind.DepthEdgeDetect, 2860, 200);
        depthEdgeY.Properties["DepthChannel"] = "B";
        depthEdgeY.Properties["EdgeMode"] = "BehindOnly";
        depthEdgeY.Properties["Scale"] = "300.0";
        depthEdgeY.Properties["Threshold"] = "0.0";
        depthEdgeY.Properties["Power"] = "1.0";

        var normalEdgeX = graph.AddNode(NodeKind.NormalEdgeDetect, 2860, 320);
        normalEdgeX.Properties["Threshold"] = "0.01";
        normalEdgeX.Properties["Power"] = "1.0";
        var normalEdgeY = graph.AddNode(NodeKind.NormalEdgeDetect, 2860, 440);
        normalEdgeY.Properties["Threshold"] = "0.01";
        normalEdgeY.Properties["Power"] = "1.0";

        var outerMaskMulX = graph.AddNode(NodeKind.Multiply, 3160, 80);
        outerMaskMulX.Properties["Type"] = "Float1";
        var outerMaskMulY = graph.AddNode(NodeKind.Multiply, 3160, 180);
        outerMaskMulY.Properties["Type"] = "Float1";
        var innerMaskMulX = graph.AddNode(NodeKind.Multiply, 3160, 320);
        innerMaskMulX.Properties["Type"] = "Float1";
        var innerMaskMulY = graph.AddNode(NodeKind.Multiply, 3160, 420);
        innerMaskMulY.Properties["Type"] = "Float1";

        var outerAdd = graph.AddNode(NodeKind.Add, 3460, 120);
        outerAdd.Properties["Type"] = "Float1";
        var innerAdd = graph.AddNode(NodeKind.Add, 3460, 370);
        innerAdd.Properties["Type"] = "Float1";

        var outerColor = graph.AddNode(NodeKind.Color, 3740, 80);
        outerColor.Properties["R"] = "1.0";
        outerColor.Properties["G"] = "0.35";
        outerColor.Properties["B"] = "0.2";
        outerColor.Properties["A"] = "1.0";
        var innerColor = graph.AddNode(NodeKind.Color, 3740, 330);
        innerColor.Properties["R"] = "1.0";
        innerColor.Properties["G"] = "0.35";
        innerColor.Properties["B"] = "0.2";
        innerColor.Properties["A"] = "1.0";

        var outerColorMul = graph.AddNode(NodeKind.Multiply, 4020, 120);
        outerColorMul.Properties["Type"] = "Float4";
        var innerColorMul = graph.AddNode(NodeKind.Multiply, 4020, 370);
        innerColorMul.Properties["Type"] = "Float4";

        var addOuter = graph.AddNode(NodeKind.Add, 4320, 180);
        addOuter.Properties["Type"] = "Float4";
        var addInner = graph.AddNode(NodeKind.Add, 4600, 220);
        addInner.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 4880, 220);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = sceneColor.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = rimDepthCenter.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = rimMaskCenter.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = normalMask.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = depthMask.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = outerMaskChannel.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = innerMaskChannel.Id, TargetPin = "Value" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = normalMask.Id, SourcePin = "Result", TargetNodeId = mulNormal.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = vec2Two.Id, SourcePin = "Value", TargetNodeId = mulNormal.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = mulNormal.Id, SourcePin = "Result", TargetNodeId = subNormal.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = vec2One.Id, SourcePin = "Value", TargetNodeId = subNormal.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = subNormal.Id, SourcePin = "Result", TargetNodeId = splitNormal.Id, TargetPin = "Value" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = splitNormal.Id, SourcePin = "X", TargetNodeId = directionX.Id, TargetPin = "X" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionX.Id, TargetPin = "Y" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionX.Id, TargetPin = "Z" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionX.Id, TargetPin = "W" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionY.Id, TargetPin = "X" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = splitNormal.Id, SourcePin = "Y", TargetNodeId = directionY.Id, TargetPin = "Y" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionY.Id, TargetPin = "Z" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = zero.Id, SourcePin = "Value", TargetNodeId = directionY.Id, TargetPin = "W" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = offsetUvX.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = directionX.Id, SourcePin = "Result", TargetNodeId = offsetUvX.Id, TargetPin = "Direction" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthMask.Id, SourcePin = "Result", TargetNodeId = offsetUvX.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = widthX.Id, SourcePin = "Value", TargetNodeId = offsetUvX.Id, TargetPin = "Width" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = offsetUvY.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = directionY.Id, SourcePin = "Result", TargetNodeId = offsetUvY.Id, TargetPin = "Direction" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthMask.Id, SourcePin = "Result", TargetNodeId = offsetUvY.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = widthY.Id, SourcePin = "Value", TargetNodeId = offsetUvY.Id, TargetPin = "Width" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetUvX.Id, SourcePin = "UV", TargetNodeId = rimDepthOffsetX.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetUvY.Id, SourcePin = "UV", TargetNodeId = rimDepthOffsetY.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = depthEdgeX.Id, TargetPin = "CenterDepth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetX.Id, SourcePin = "Color", TargetNodeId = depthEdgeX.Id, TargetPin = "OffsetDepth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = depthEdgeY.Id, TargetPin = "CenterDepth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetY.Id, SourcePin = "Color", TargetNodeId = depthEdgeY.Id, TargetPin = "OffsetDepth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = normalEdgeX.Id, TargetPin = "CenterNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetX.Id, SourcePin = "Color", TargetNodeId = normalEdgeX.Id, TargetPin = "OffsetNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = normalEdgeY.Id, TargetPin = "CenterNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetY.Id, SourcePin = "Color", TargetNodeId = normalEdgeY.Id, TargetPin = "OffsetNormal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthEdgeX.Id, SourcePin = "Edge", TargetNodeId = outerMaskMulX.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskChannel.Id, SourcePin = "Result", TargetNodeId = outerMaskMulX.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthEdgeY.Id, SourcePin = "Edge", TargetNodeId = outerMaskMulY.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskChannel.Id, SourcePin = "Result", TargetNodeId = outerMaskMulY.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = normalEdgeX.Id, SourcePin = "Edge", TargetNodeId = innerMaskMulX.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskChannel.Id, SourcePin = "Result", TargetNodeId = innerMaskMulX.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = normalEdgeY.Id, SourcePin = "Edge", TargetNodeId = innerMaskMulY.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskChannel.Id, SourcePin = "Result", TargetNodeId = innerMaskMulY.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskMulX.Id, SourcePin = "Result", TargetNodeId = outerAdd.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskMulY.Id, SourcePin = "Result", TargetNodeId = outerAdd.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskMulX.Id, SourcePin = "Result", TargetNodeId = innerAdd.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskMulY.Id, SourcePin = "Result", TargetNodeId = innerAdd.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerColor.Id, SourcePin = "Color", TargetNodeId = outerColorMul.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerAdd.Id, SourcePin = "Result", TargetNodeId = outerColorMul.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerColor.Id, SourcePin = "Color", TargetNodeId = innerColorMul.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerAdd.Id, SourcePin = "Result", TargetNodeId = innerColorMul.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = sceneColor.Id, SourcePin = "Color", TargetNodeId = addOuter.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerColorMul.Id, SourcePin = "Result", TargetNodeId = addOuter.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = addOuter.Id, SourcePin = "Result", TargetNodeId = addInner.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerColorMul.Id, SourcePin = "Result", TargetNodeId = addInner.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = addInner.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Color" });

        return graph;
    }
}

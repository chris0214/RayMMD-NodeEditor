namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateScreenSpaceRimPostProcessSampleCore()
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
        rimMaskCenter.Properties["Description"] = "Rim mask buffer";
        rimMaskCenter.Properties["Format"] = "D3DFMT_A8R8G8B8";
        rimMaskCenter.Properties["FilterMode"] = "Point";

        var normalMask = graph.AddNode(NodeKind.ComponentMask, 700, 230);
        normalMask.Properties["Channels"] = "RG";
        var depthMask = graph.AddNode(NodeKind.ComponentMask, 700, 350);
        depthMask.Properties["Channels"] = "B";
        var alphaMask = graph.AddNode(NodeKind.ComponentMask, 700, 500);
        alphaMask.Properties["Channels"] = "A";
        var outerMaskChannel = graph.AddNode(NodeKind.ComponentMask, 700, 620);
        outerMaskChannel.Properties["Channels"] = "R";
        var innerMaskChannel = graph.AddNode(NodeKind.ComponentMask, 700, 740);
        innerMaskChannel.Properties["Channels"] = "G";
        var phongMaskChannel = graph.AddNode(NodeKind.ComponentMask, 700, 860);
        phongMaskChannel.Properties["Channels"] = "B";
        var one = graph.AddNode(NodeKind.Scalar, 980, 860);
        one.Properties["Value"] = "1.0";
        var phongToggle = graph.AddNode(NodeKind.Scalar, 1240, 860);
        phongToggle.Properties["Value"] = "0.0";
        var phongSelect = graph.AddNode(NodeKind.Lerp, 1500, 860);
        phongSelect.Properties["Type"] = "Float1";

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
        var offsetUvX = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 140);
        offsetUvX.Properties["Scale"] = "0.005";
        offsetUvX.Properties["DepthPower"] = "0.75";
        offsetUvX.Properties["SizeX"] = "1.0";
        offsetUvX.Properties["SizeY"] = "1.0";
        offsetUvX.Properties["Role"] = "MainOffsetX";

        var offsetUvY = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 360);
        offsetUvY.Properties["Scale"] = "0.005";
        offsetUvY.Properties["DepthPower"] = "0.75";
        offsetUvY.Properties["SizeX"] = "1.0";
        offsetUvY.Properties["SizeY"] = "1.0";
        offsetUvY.Properties["Role"] = "MainOffsetY";

        var offsetRejectUvX = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 580);
        offsetRejectUvX.Properties["Scale"] = "0.005";
        offsetRejectUvX.Properties["DepthPower"] = "0.75";
        offsetRejectUvX.Properties["SizeX"] = "1.0";
        offsetRejectUvX.Properties["SizeY"] = "1.0";
        offsetRejectUvX.Properties["Role"] = "RejectOffset";

        var offsetRejectUvY = graph.AddNode(NodeKind.ScreenUvOffset, 2280, 760);
        offsetRejectUvY.Properties["Scale"] = "0.005";
        offsetRejectUvY.Properties["DepthPower"] = "0.75";
        offsetRejectUvY.Properties["SizeX"] = "1.0";
        offsetRejectUvY.Properties["SizeY"] = "1.0";
        offsetRejectUvY.Properties["Role"] = "RejectOffset";

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

        var rimMaskOffsetX = graph.AddNode(NodeKind.OffscreenBufferSample, 2560, 580);
        rimMaskOffsetX.Properties["BufferName"] = "Na_RimMask";
        rimMaskOffsetX.Properties["Description"] = "Mask buffer";
        rimMaskOffsetX.Properties["Format"] = "D3DFMT_A8R8G8B8";
        rimMaskOffsetX.Properties["FilterMode"] = "Point";

        var rimMaskOffsetY = graph.AddNode(NodeKind.OffscreenBufferSample, 2560, 760);
        rimMaskOffsetY.Properties["BufferName"] = "Na_RimMask";
        rimMaskOffsetY.Properties["Description"] = "Mask buffer";
        rimMaskOffsetY.Properties["Format"] = "D3DFMT_A8R8G8B8";
        rimMaskOffsetY.Properties["FilterMode"] = "Point";

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
        var innerMaskMulX = graph.AddNode(NodeKind.Multiply, 3160, 340);
        innerMaskMulX.Properties["Type"] = "Float1";
        var innerMaskMulY = graph.AddNode(NodeKind.Multiply, 3160, 440);
        innerMaskMulY.Properties["Type"] = "Float1";

        var compose = graph.AddNode(NodeKind.ScreenSpaceRimCompose, 4040, 230);
        compose.Properties["ControlMode"] = "Advanced";
        compose.Properties["SimpleWidth"] = "4.0";
        compose.Properties["SimpleBrightness"] = "1.25";
        compose.Properties["XWidth"] = "1.3";
        compose.Properties["YWidth"] = "1.3";
        compose.Properties["XDirection"] = "0.5";
        compose.Properties["YDirection"] = "0.5";
        compose.Properties["XIntensity"] = "0.8";
        compose.Properties["YIntensity"] = "0.8";
        compose.Properties["PhongMaskInfluence"] = "0.0";
        compose.Properties["SimpleR"] = "1.0";
        compose.Properties["SimpleG"] = "0.35";
        compose.Properties["SimpleB"] = "0.2";
        compose.Properties["SimpleA"] = "1.0";
        compose.Properties["OuterR"] = "1.0";
        compose.Properties["OuterG"] = "0.35";
        compose.Properties["OuterB"] = "0.2";
        compose.Properties["OuterA"] = "1.0";
        compose.Properties["InnerR"] = "1.0";
        compose.Properties["InnerG"] = "0.35";
        compose.Properties["InnerB"] = "0.2";
        compose.Properties["InnerA"] = "1.0";
        compose.Properties["OuterThreshold"] = "0.0";
        compose.Properties["InnerThreshold"] = "0.0";
        compose.Properties["OuterPower"] = "1.0";
        compose.Properties["InnerPower"] = "1.0";
        compose.Properties["BlockCut"] = "0.05";
        compose.Properties["BlockPower"] = "0.2";
        compose.Properties["OuterIntensity"] = "1.0";
        compose.Properties["InnerIntensity"] = "0.6";
        compose.Properties["EnableOuter"] = "True";
        compose.Properties["EnableInner"] = "True";
        compose.Properties["EnableBlockerReject"] = "True";
        compose.Properties["EnableDepthReject"] = "True";
        compose.Properties["RejectWidth"] = "16.0";
        compose.Properties["RejectAlphaThreshold"] = "0.5";
        compose.Properties["RejectMaskThreshold"] = "0.5";
        compose.Properties["DepthRejectThreshold"] = "0.0002";
        compose.Properties["ComposeMode"] = "Add";
        compose.Properties["DebugView"] = "Final";
        compose.Properties["PreserveAlpha"] = "True";

        var output = graph.AddNode(NodeKind.Output, 4360, 230);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = sceneColor.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = rimDepthCenter.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = rimMaskCenter.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = normalMask.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = depthMask.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = alphaMask.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = outerMaskChannel.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = innerMaskChannel.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = phongMaskChannel.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = one.Id, SourcePin = "Value", TargetNodeId = phongSelect.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = phongMaskChannel.Id, SourcePin = "Result", TargetNodeId = phongSelect.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = phongToggle.Id, SourcePin = "Value", TargetNodeId = phongSelect.Id, TargetPin = "T" });

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
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = alphaMask.Id, SourcePin = "Result", TargetNodeId = offsetUvX.Id, TargetPin = "Mask" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = offsetUvY.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = directionY.Id, SourcePin = "Result", TargetNodeId = offsetUvY.Id, TargetPin = "Direction" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthMask.Id, SourcePin = "Result", TargetNodeId = offsetUvY.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = alphaMask.Id, SourcePin = "Result", TargetNodeId = offsetUvY.Id, TargetPin = "Mask" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetUvX.Id, SourcePin = "UV", TargetNodeId = rimDepthOffsetX.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetUvY.Id, SourcePin = "UV", TargetNodeId = rimDepthOffsetY.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = offsetRejectUvX.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = directionX.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvX.Id, TargetPin = "Direction" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthMask.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvX.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = alphaMask.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvX.Id, TargetPin = "Mask" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = screenUv.Id, SourcePin = "UV", TargetNodeId = offsetRejectUvY.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = directionY.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvY.Id, TargetPin = "Direction" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = depthMask.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvY.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = alphaMask.Id, SourcePin = "Result", TargetNodeId = offsetRejectUvY.Id, TargetPin = "Mask" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetRejectUvX.Id, SourcePin = "UV", TargetNodeId = rimMaskOffsetX.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetRejectUvY.Id, SourcePin = "UV", TargetNodeId = rimMaskOffsetY.Id, TargetPin = "UV" });

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

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = sceneColor.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "SceneColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskMulX.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "OuterMaskX" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskMulY.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "OuterMaskY" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskMulX.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "InnerMaskX" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskMulY.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "InnerMaskY" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outerMaskChannel.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "OuterMask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = innerMaskChannel.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "InnerMask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = phongSelect.Id, SourcePin = "Result", TargetNodeId = compose.Id, TargetPin = "PhongMask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthCenter.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "CenterDepthInfo" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetX.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "OffsetDepthX" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimDepthOffsetY.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "OffsetDepthY" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskCenter.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "CenterMask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskOffsetX.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "OffsetMaskX" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rimMaskOffsetY.Id, SourcePin = "Color", TargetNodeId = compose.Id, TargetPin = "OffsetMaskY" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = compose.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });

        return graph;
    }
}

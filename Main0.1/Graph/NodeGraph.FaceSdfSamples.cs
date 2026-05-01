namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateFaceSdfShadowSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        void Connect(GraphNode sourceNode, string sourcePin, GraphNode targetNode, string targetPin)
        {
            graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = sourceNode.Id,
                SourcePin = sourcePin,
                TargetNodeId = targetNode.Id,
                TargetPin = targetPin,
            });
        }

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 200);

        var faceTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        faceTexture.Properties["ColorSpace"] = "Color";

        var lighting = graph.AddNode(NodeKind.BasicLighting, 660, 120);
        lighting.Properties["UseSpecular"] = "0.0";
        lighting.Properties["UseToon"] = "0.0";

        var headDirection = graph.AddNode(NodeKind.ControlObjectBoneDirection, 80, 420);
        headDirection.Properties["Name"] = "(self)";
        headDirection.Properties["Item"] = "\u982D";
        headDirection.Properties["Axis"] = "Z";

        var theta = graph.AddNode(NodeKind.ControlObjectValue, 360, 420);
        theta.Properties["Name"] = "SDF Controller.pmx";
        theta.Properties["Item"] = "Rotation";

        var faceSdf = graph.AddNode(NodeKind.FaceSdfSample, 660, 380);
        faceSdf.Properties["ResourceName"] = "face_sdf.png";
        faceSdf.Properties["TextureMode"] = "Static";
        faceSdf.Properties["AddressMode"] = "Wrap";
        faceSdf.Properties["FilterMode"] = "Linear";
        faceSdf.Properties["RotationOffset"] = "0.25";

        var materialToonColor = graph.AddNode(NodeKind.MaterialToonColor, 960, 520);

        var smooth = graph.AddNode(NodeKind.ControlObjectValue, 960, 380);
        smooth.Properties["Name"] = "SDF Controller.pmx";
        smooth.Properties["Item"] = "SDF_Smooth";

        var smoothScale = graph.AddNode(NodeKind.Scalar, 1260, 300);
        smoothScale.Properties["Value"] = "30.0";

        var smoothNormalized = graph.AddNode(NodeKind.Divide, 1260, 380);
        smoothNormalized.Properties["Type"] = "Float1";

        var faceShadow = graph.AddNode(NodeKind.FaceSdfShadow, 1560, 280);
        faceShadow.Properties["Smooth"] = "0.05";

        var output = graph.AddNode(NodeKind.Output, 1860, 280);
        output.Properties["AlphaMode"] = "ColorAlpha";

        Connect(texCoord, "UV", faceTexture, "UV");
        Connect(faceTexture, "Color", lighting, "Albedo");

        Connect(texCoord, "UV", faceSdf, "UV");
        Connect(headDirection, "Value", faceSdf, "HeadDirection");
        Connect(theta, "Value", faceSdf, "Theta");

        Connect(smooth, "Value", smoothNormalized, "A");
        Connect(smoothScale, "Value", smoothNormalized, "B");

        Connect(lighting, "Color", faceShadow, "LitColor");
        Connect(materialToonColor, "Value", faceShadow, "ShadowColor");
        Connect(faceSdf, "Sample", faceShadow, "Sample");
        Connect(faceSdf, "Threshold", faceShadow, "Threshold");
        Connect(smoothNormalized, "Result", faceShadow, "Smooth");

        Connect(faceShadow, "Color", output, "Color");

        return graph;
    }
}

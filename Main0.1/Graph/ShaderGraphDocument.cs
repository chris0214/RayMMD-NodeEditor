namespace RayMmdNodeEditor.Graph;

public sealed class ShaderGraphDocument
{
    public string FormatVersion { get; set; } = "2.0";

    public NodeGraph PixelGraph { get; set; } = NodeGraph.CreateDefault(GraphWorkspaceMode.ObjectMaterial);

    public NodeGraph VertexGraph { get; set; } = NodeGraph.CreateDefault(GraphWorkspaceMode.ObjectMaterial);

    public List<NodeGroupDefinition> Groups { get; set; } = [];
}

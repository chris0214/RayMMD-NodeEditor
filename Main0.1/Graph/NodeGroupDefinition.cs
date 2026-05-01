namespace RayMmdNodeEditor.Graph;

public sealed class NodeGroupDefinition
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = "Group";

    public GraphWorkspaceMode WorkspaceMode { get; set; } = GraphWorkspaceMode.ObjectMaterial;

    public NodeGraph Graph { get; set; } = new();

    public List<NodePinDefinition> Inputs { get; init; } = [];

    public List<NodePinDefinition> Outputs { get; init; } = [];
}

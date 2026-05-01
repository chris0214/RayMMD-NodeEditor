namespace RayMmdNodeEditor.Graph;

public sealed class GraphNode
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public NodeKind Kind { get; init; }

    public float X { get; set; }

    public float Y { get; set; }

    public Dictionary<string, string> Properties { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public List<NodePinDefinition> CustomInputs { get; init; } = [];

    public List<NodePinDefinition> CustomOutputs { get; init; } = [];
}

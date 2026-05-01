namespace RayMmdNodeEditor.Graph;

public sealed class GraphConnection
{
    public Guid SourceNodeId { get; init; }

    public string SourcePin { get; init; } = string.Empty;

    public Guid TargetNodeId { get; init; }

    public string TargetPin { get; init; } = string.Empty;
}

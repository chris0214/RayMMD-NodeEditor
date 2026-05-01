using System.Drawing;

namespace RayMmdNodeEditor.Graph;

public sealed record NodeDefinition(
    NodeKind Kind,
    string Title,
    string Description,
    NodeCategory Category,
    Color AccentColor,
    IReadOnlyList<NodePinDefinition> Inputs,
    IReadOnlyList<NodePinDefinition> Outputs,
    IReadOnlyList<NodePropertyDefinition> Properties);

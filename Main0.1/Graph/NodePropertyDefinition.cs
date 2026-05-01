namespace RayMmdNodeEditor.Graph;

public sealed record NodePropertyDefinition(
    string Name,
    string DisplayName,
    string DefaultValue,
    NodePropertyKind Kind = NodePropertyKind.Float,
    NodePropertyEditorKind Editor = NodePropertyEditorKind.Auto,
    bool AllowInlineEditor = false,
    float? Minimum = null,
    float? Maximum = null,
    float? Step = null,
    string? InlinePin = null);

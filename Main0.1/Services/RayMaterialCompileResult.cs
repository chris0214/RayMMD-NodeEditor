namespace RayMmdNodeEditor.Services;

public sealed record RayMaterialCompileResult(
    bool Success,
    string MaterialText,
    IReadOnlyList<string> Messages);

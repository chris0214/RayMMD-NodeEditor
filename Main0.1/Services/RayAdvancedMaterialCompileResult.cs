namespace RayMmdNodeEditor.Services;

public sealed record RayAdvancedMaterialCompileResult(
    bool Success,
    string CommonPatchBlock,
    IReadOnlySet<string> Slots,
    IReadOnlyList<string> TextureFiles,
    IReadOnlyList<string> Messages,
    string ShadingPatchBlock = "",
    IReadOnlySet<string>? ShadingSlots = null);

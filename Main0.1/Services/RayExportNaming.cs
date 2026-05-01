namespace RayMmdNodeEditor.Services;

public static class RayExportNaming
{
    public const string DefaultMaterialFileName = "material_2.0.fx";

    public static string GetMaterialFileName(RayDocument document)
    {
        return NormalizeMaterialFileName(document.MaterialFileName);
    }

    public static string NormalizeMaterialFileName(string? fileName)
    {
        var name = string.IsNullOrWhiteSpace(fileName)
            ? DefaultMaterialFileName
            : Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(name) ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return DefaultMaterialFileName;
        }

        return string.Equals(Path.GetExtension(name), ".fx", StringComparison.OrdinalIgnoreCase)
            ? name
            : name + ".fx";
    }
}

using System.Text.RegularExpressions;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public sealed record RayTextureExportResult(string MaterialText, string CommonText, IReadOnlyList<string> CopiedFiles, IReadOnlyList<string> Warnings);

public static class RayTextureExportManager
{
    public static RayTextureExportResult CopyTexturesAndRewriteMaterialToDirectory(RayDocument document, string materialText, string materialDirectory)
    {
        return CopyTexturesAndRewriteMaterialAndCommonToDirectory(document, materialText, string.Empty, materialDirectory);
    }

    public static RayTextureExportResult CopyTexturesAndRewriteMaterialAndCommonToDirectory(RayDocument document, string materialText, string commonText, string materialDirectory)
    {
        if (!document.CopyTextureFiles)
        {
            return new RayTextureExportResult(materialText, commonText, [], []);
        }

        var copied = new List<string>();
        var warnings = new List<string>();
        var textureDir = Path.Combine(materialDirectory, "textures");
        Directory.CreateDirectory(textureDir);

        foreach (var node in document.Graph.Nodes.Where(node => node.Kind == NodeKind.RayTextureSlot))
        {
            var source = Read(node, "Source", "File");
            if (!RayCompatibilityChecker.IsFileSource(source))
            {
                continue;
            }

            var originalPath = Read(node, "File", string.Empty);
            var absolutePath = RayCompatibilityChecker.ResolveTexturePath(document, originalPath);
            if (absolutePath is null || !File.Exists(absolutePath))
            {
                warnings.Add($"Missing texture skipped: {originalPath}");
                continue;
            }

            var destinationFileName = MakeUniqueFileName(textureDir, Path.GetFileName(absolutePath));
            var destinationPath = Path.Combine(textureDir, destinationFileName);
            File.Copy(absolutePath, destinationPath, overwrite: true);
            copied.Add(destinationPath);

            var relativeFxPath = $"textures/{destinationFileName}".Replace('\\', '/');
            materialText = ReplaceMapFile(materialText, originalPath, relativeFxPath);
            commonText = ReplaceMapFile(commonText, originalPath, relativeFxPath);
        }

        return new RayTextureExportResult(materialText, commonText, copied, warnings);
    }

    public static RayTextureExportResult CopyTexturesAndRewriteMaterial(RayDocument document, string materialText, string commonText, string exportRoot)
    {
        if (!document.CopyTextureFiles)
        {
            return new RayTextureExportResult(materialText, commonText, [], []);
        }

        var copied = new List<string>();
        var warnings = new List<string>();
        var textureDir = Path.Combine(exportRoot, "Materials", "textures");
        Directory.CreateDirectory(textureDir);

        foreach (var node in document.Graph.Nodes.Where(node => node.Kind == NodeKind.RayTextureSlot))
        {
            var source = Read(node, "Source", "File");
            if (!RayCompatibilityChecker.IsFileSource(source))
            {
                continue;
            }

            var originalPath = Read(node, "File", string.Empty);
            var absolutePath = RayCompatibilityChecker.ResolveTexturePath(document, originalPath);
            if (absolutePath is null || !File.Exists(absolutePath))
            {
                warnings.Add($"Missing texture skipped: {originalPath}");
                continue;
            }

            var destinationFileName = MakeUniqueFileName(textureDir, Path.GetFileName(absolutePath));
            var destinationPath = Path.Combine(textureDir, destinationFileName);
            File.Copy(absolutePath, destinationPath, overwrite: true);
            copied.Add(destinationPath);

            var relativeFxPath = $"textures/{destinationFileName}".Replace('\\', '/');
            materialText = ReplaceMapFile(materialText, originalPath, relativeFxPath);
            commonText = ReplaceMapFile(commonText, originalPath, relativeFxPath);
        }

        return new RayTextureExportResult(materialText, commonText, copied, warnings);
    }

    private static string ReplaceMapFile(string materialText, string originalPath, string relativeFxPath)
    {
        var normalizedOriginal = originalPath.Replace('\\', '/');
        return Regex.Replace(
            materialText,
            $@"""{Regex.Escape(normalizedOriginal)}""",
            $"\"{relativeFxPath}\"");
    }

    private static string MakeUniqueFileName(string directory, string fileName)
    {
        var candidate = fileName;
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var index = 1;
        while (File.Exists(Path.Combine(directory, candidate)))
        {
            candidate = $"{stem}_{index}{extension}";
            index++;
        }

        return candidate;
    }

    private static string Read(GraphNode node, string propertyName, string fallback)
    {
        return node.Properties.TryGetValue(propertyName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }
}

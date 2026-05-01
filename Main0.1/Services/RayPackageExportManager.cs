namespace RayMmdNodeEditor.Services;

public sealed record RayPackageExportResult(
    bool Enabled,
    int CopiedFiles,
    IReadOnlyList<string> CopiedRoots,
    IReadOnlyList<string> Warnings);

public static class RayPackageExportManager
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".svn",
        ".hg",
        "__pycache__",
    };

    private static readonly HashSet<string> ExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "RayMmdNodeEditor_Report.txt",
    };

    public static RayPackageExportResult Export(RayDocument document)
    {
        if (!document.ExportFullRayPackage)
        {
            return new RayPackageExportResult(false, 0, [], []);
        }

        var sourceRoot = Path.GetFullPath(document.RayRootPath);
        var targetRoot = Path.GetFullPath(document.ExportDirectory);
        var warnings = new List<string>();
        var roots = new List<string>();
        var copiedFiles = 0;

        if (!Directory.Exists(sourceRoot))
        {
            return new RayPackageExportResult(true, 0, [], [$"Missing Ray root directory: {sourceRoot}"]);
        }

        if (IsSameOrChildPath(sourceRoot, targetRoot))
        {
            return new RayPackageExportResult(
                true,
                0,
                [],
                [$"Export directory must not be inside the source Ray root when full package export is enabled: {targetRoot}"]);
        }

        Directory.CreateDirectory(targetRoot);
        roots.AddRange(Directory.EnumerateDirectories(sourceRoot).Select(Path.GetFileName).OfType<string>().Where(name => !string.IsNullOrWhiteSpace(name)));
        roots.AddRange(Directory.EnumerateFiles(sourceRoot).Select(Path.GetFileName).OfType<string>().Where(name => !string.IsNullOrWhiteSpace(name)));
        CopyDirectory(sourceRoot, targetRoot, targetRoot, warnings, ref copiedFiles);
        return new RayPackageExportResult(true, copiedFiles, roots.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), warnings);
    }

    private static void CopyDirectory(
        string sourceDirectory,
        string targetDirectory,
        string targetRoot,
        List<string> warnings,
        ref int copiedFiles)
    {
        Directory.CreateDirectory(targetDirectory);
        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            if (ExcludedFileNames.Contains(Path.GetFileName(file)))
            {
                continue;
            }

            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
            try
            {
                File.Copy(file, targetFile, overwrite: true);
                copiedFiles++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add($"Skipped file: {file} ({ex.Message})");
            }
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directory);
            if (ExcludedDirectoryNames.Contains(directoryName))
            {
                continue;
            }

            var targetDirectoryPath = Path.Combine(targetDirectory, directoryName);
            if (IsSameOrChildPath(directory, targetRoot))
            {
                continue;
            }

            CopyDirectory(directory, targetDirectoryPath, targetRoot, warnings, ref copiedFiles);
        }
    }

    private static bool IsSameOrChildPath(string parentPath, string childPath)
    {
        var parent = EnsureTrailingSeparator(Path.GetFullPath(parentPath));
        var child = EnsureTrailingSeparator(Path.GetFullPath(childPath));
        return child.StartsWith(parent, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}

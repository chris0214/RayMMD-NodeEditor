using System.Reflection;

namespace RayMmdNodeEditor;

public static class AppResourceLoader
{
    private const string ResourcePrefix = "RayMmdNodeEditor.Resources.";

    public static bool TryReadText(string relativePath, out string text)
    {
        text = string.Empty;

        var diskPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(diskPath))
        {
            text = File.ReadAllText(diskPath);
            return true;
        }

        using var stream = OpenEmbedded(relativePath);
        if (stream is null)
        {
            return false;
        }

        using var reader = new StreamReader(stream);
        text = reader.ReadToEnd();
        return true;
    }

    public static bool TryWriteFile(string relativePath, string destinationPath)
    {
        var diskPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(diskPath))
        {
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(diskPath, destinationPath, overwrite: true);
            return true;
        }

        using var stream = OpenEmbedded(relativePath);
        if (stream is null)
        {
            return false;
        }

        var outputDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var fileStream = File.Create(destinationPath);
        stream.CopyTo(fileStream);
        return true;
    }

    private static Stream? OpenEmbedded(string relativePath)
    {
        var resourceName = ResourcePrefix + relativePath
            .Replace('\\', '.')
            .Replace('/', '.');

        return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    }
}

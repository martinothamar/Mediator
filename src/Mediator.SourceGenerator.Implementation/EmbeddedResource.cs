// Copyright @kzu
// License MIT
// copied from https://github.com/devlooped/ThisAssembly/blob/main/src/EmbeddedResource.cs

using System.Reflection;

internal static class EmbeddedResource
{
    internal static readonly string BaseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static string GetContent(string relativePath)
    {
        var filePath = Path.Combine(BaseDir, Path.GetFileName(relativePath));
        if (File.Exists(filePath))
            return File.ReadAllText(filePath);

        var baseName = Assembly.GetExecutingAssembly().GetName().Name;
        var resourceName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        var manifestResourceName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .FirstOrDefault(x => x!.EndsWith(resourceName, StringComparison.InvariantCulture));

        if (string.IsNullOrEmpty(manifestResourceName))
            throw new InvalidOperationException(
                $"Did not find required resource ending in '{resourceName}' in assembly '{baseName}'."
            );

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName);

        if (stream == null)
            throw new InvalidOperationException(
                $"Did not find required resource '{manifestResourceName}' in assembly '{baseName}'."
            );

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

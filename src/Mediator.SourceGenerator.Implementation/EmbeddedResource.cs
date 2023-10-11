// Copyright @kzu
// License MIT
// copied from https://github.com/devlooped/ThisAssembly/blob/main/src/EmbeddedResource.cs

using System.Reflection;

internal static class EmbeddedResource
{
    public static string GetContent(string relativePath)
    {
        string baseName = Assembly.GetExecutingAssembly().GetName().Name;
        string resourceName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        string manifestResourceName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .FirstOrDefault(x => x!.EndsWith(resourceName, StringComparison.InvariantCulture));

        if (string.IsNullOrEmpty(manifestResourceName))
            throw new InvalidOperationException(
                $"Did not find required resource ending in '{resourceName}' in assembly '{baseName}'."
            );

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName);

        if (stream is null)
            throw new InvalidOperationException(
                $"Did not find required resource '{manifestResourceName}' in assembly '{baseName}'."
            );

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}

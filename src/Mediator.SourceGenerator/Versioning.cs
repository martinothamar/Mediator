namespace Mediator.SourceGenerator;

public static class Versioning
{
    public static string GetVersion()
    {
        var generatorAssembly = typeof(Versioning).Assembly;
        var generatorVersion = generatorAssembly.GetName().Version.ToString();
        return generatorVersion;
    }
}

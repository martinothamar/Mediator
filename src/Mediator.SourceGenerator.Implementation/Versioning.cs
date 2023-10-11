using System.Reflection;

namespace Mediator.SourceGenerator;

public static class Versioning
{
    public static string GetVersion()
    {
        Assembly generatorAssembly = typeof(Versioning).Assembly;
        string generatorVersion = generatorAssembly.GetName().Version.ToString();
        return generatorVersion;
    }
}

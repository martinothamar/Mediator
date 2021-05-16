using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests
{
    public static class Fixture
    {
        public static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ISender).GetTypeInfo().Assembly.Location),
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        public static Task<string> SourceFromResourceFile(string file) => File.ReadAllTextAsync(Path.Combine("resources", file));
    }
}

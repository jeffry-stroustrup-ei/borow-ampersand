using System.Collections.Immutable;
using System.Text;
using BorowAmpersand.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BorowAmpersand.Generator.Tests;

public sealed class BorowSourceGeneratorTests
{
    [Fact]
    public void RewritesBorowAndDereferenceOperators()
    {
        const string source = """
using BorowAmpersand.Runtime;
int hp = 100;
var soul = &borow hp;
*soul -= 10;
Console.WriteLine(&borow hp);
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Empty(diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));

        var generated = result.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("Borow.Ampersand(ref ", generated);
        Assert.Contains("conceptId:", generated);
        Assert.Contains(".Value -= 10;", generated);
        Assert.Contains("Console.WriteLine", generated);
        Assert.Contains(".ToString()", generated);
    }

    [Fact]
    public void RewritesBorowWithContextAndConceptId()
    {
        const string source = """
using BorowAmpersand.Runtime;
var context = borowcontext("demo");
int hp = 100;
var soul = &borow(hp, context, "shared-concept");
_ = soul.Value;
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Empty(diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));

        var generated = result.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("new global::BorowAmpersand.Runtime.BorowContext(\"demo\")", generated);
        Assert.Contains("context:", generated);
        Assert.Contains("conceptId:", generated);
        Assert.Contains("\"shared-concept\"", generated);
        Assert.DoesNotContain("conceptId: \"C:/tests", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void RewritesBorowContextSugarWithNamedArguments()
    {
        const string source = """
using BorowAmpersand.Runtime;
var context = borowcontext(
    Name: "combat",
    FutureUsage: "hp updates",
    Source: "Battle",
    RespectLevel: 9);
_ = context.Name;
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Empty(diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));

        var generated = result.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("new global::BorowAmpersand.Runtime.BorowContext(", generated);
        Assert.Contains("Name: \"combat\"", generated);
        Assert.Contains("FutureUsage: \"hp updates\"", generated);
        Assert.Contains("Source: \"Battle\"", generated);
        Assert.Contains("RespectLevel: 9", generated);
    }

    [Fact]
    public void RewritesBorowTypeSugar()
    {
        const string source = """
public static class Demo
{
    public static borow<int> Echo(borow<int> value)
    {
        borow<int> local = value;
        return local;
    }
}
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Empty(diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));

        var generated = result.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("global::BorowAmpersand.Runtime.BorowRef<int> Echo", generated);
        Assert.Contains("global::BorowAmpersand.Runtime.BorowRef<int> value", generated);
        Assert.Contains("global::BorowAmpersand.Runtime.BorowRef<int> local", generated);
    }

    [Fact]
    public void ReportsBor001ForTemporaryOperands()
    {
        const string source = """
var broken = &borow (1 + 2);
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Contains(diagnostics, static d => d.Id == "BOR001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsBor404ForUnusedVariable()
    {
        const string source = """
int lonely = 5;
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.Contains(diagnostics, static d => d.Id == "BOR404" && d.GetMessage().Contains("lonely", StringComparison.Ordinal));
    }

    [Fact]
    public void DoesNotReportBor404ForUsingDeclarations()
    {
        const string source = """
using BorowAmpersand.Runtime;
using var scope = BorowExecutionContext.Use(borowcontext("demo"));
int hp = 1;
var soul = &borow hp;
_ = soul.Value;
""";

        var result = RunGenerator(source);
        var diagnostics = result.Results[0].Diagnostics;

        Assert.DoesNotContain(diagnostics, static d => d.Id == "BOR404" && d.GetMessage().Contains("scope", StringComparison.Ordinal));
    }

    private static GeneratorDriverRunResult RunGenerator(string borSource)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "BorowGeneratorTests",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText("internal static class Placeholder { }") },
            references: GetDefaultReferences(),
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var additionalText = new InMemoryAdditionalText(@"C:\tests\Program.bor.cs", borSource);
        var generator = new BorowSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { generator.AsSourceGenerator() },
            additionalTexts: ImmutableArray.Create<AdditionalText>(additionalText),
            parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }

    private static ImmutableArray<MetadataReference> GetDefaultReferences()
    {
        var trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedAssemblies))
        {
            throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES is unavailable.");
        }

        var references = trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(BorowAmpersand.Runtime.Borow).Assembly.Location));
        return references.ToImmutableArray();
    }

    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public InMemoryAdditionalText(string path, string source)
        {
            Path = path;
            _text = SourceText.From(source, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }
}

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tinkoff.Visor.Gen
{
    [Generator]
    public class LensGenerator : IIncrementalGenerator
    {
        static string Indent(int steps) => new string(' ', steps * 4);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<RecordDeclarationSyntax> recordDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(m => m != null);

            IncrementalValueProvider<(Compilation, ImmutableArray<RecordDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(recordDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            => node is RecordDeclarationSyntax m && m.AttributeLists.Count > 0;

        static RecordDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var rec = (RecordDeclarationSyntax)context.Node;
            var attributeMetadata = context.SemanticModel.Compilation.GetTypeByMetadataName("Tinkoff.Visor.OpticsAttribute");

            var model = context.SemanticModel.Compilation.GetSemanticModel(rec.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(rec) as ITypeSymbol;

            var opticsAttrData = symbol?.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass?.Equals(attributeMetadata, SymbolEqualityComparer.Default) == true);

            if (opticsAttrData is null) return null;

            return rec;
        }

        private void Execute(Compilation compilation, ImmutableArray<RecordDeclarationSyntax> records, SourceProductionContext context)
        {
            INamedTypeSymbol attributeMetadata = compilation.GetTypeByMetadataName("Tinkoff.Visor.OpticsAttribute");

            foreach (var rec in records.Distinct())
            {
                var model = compilation.GetSemanticModel(rec.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(rec, context.CancellationToken) as ITypeSymbol;

                var opticsAttrData = symbol?.GetAttributes().FirstOrDefault(x =>
                    x.AttributeClass?.Equals(attributeMetadata, SymbolEqualityComparer.Default) == true);

                if (opticsAttrData is null) continue;

                var withNested = !opticsAttrData.ConstructorArguments.IsEmpty && (opticsAttrData.ConstructorArguments.First().Value as bool? ?? false);

                var filename = $"{EscapeFileName(symbol.ToDisplayString())}.Optics.g.cs";

                var code = ContainingTypesBuilder.Build(symbol, withNested);

                context.AddSource(filename, code);
            }
        }

        static string EscapeFileName(string fileName) => new[] { '<', '>', ',' }
            .Aggregate(new StringBuilder(fileName), (s, c) => s.Replace(c, '_')).ToString();
    }
}
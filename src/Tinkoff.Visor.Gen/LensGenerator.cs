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
        static string Indent(int Steps) => new string(' ', Steps * 4);

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

                var filename = $"{EscapeFileName(symbol.ToDisplayString())}.Optics.g.cs";

                var code = ContainingTypesBuilder.Build(symbol, content =>
                {
                    foreach (var propertySymbol in symbol.GetProperties())
                    {
                        var propertyName = propertySymbol.ToFQF();

                        if (propertyName == "EqualityContract") continue;

                        var propertyType = propertySymbol.Type.ToNullableFQF();

                        if (propertyType.EndsWith("?"))
                            content.AppendLine("#nullable enable");

                        content.AppendLine($"{Indent(2)}public static global::Tinkoff.Visor.ILens<{symbol.ToDisplayString()}, {propertyType}> {propertyName}Lens =>");
                        content.AppendLine($"{Indent(3)}global::Tinkoff.Visor.Lens<{symbol.ToDisplayString()}, {propertyType}>.New(");
                        content.AppendLine($"{Indent(4)}p => p.{propertyName},");
                        content.AppendLine($"{Indent(4)}f => p => p with {{{propertyName} = f(p.{propertyName})}}");
                        content.AppendLine($"{Indent(3)});");

                        if (propertyType.EndsWith("?"))
                            content.AppendLine("#nullable disable");
                    }
                }, true);

                context.AddSource(filename, code);
            }
        }

        static string EscapeFileName(string fileName) => new[] { '<', '>', ',' }
            .Aggregate(new StringBuilder(fileName), (s, c) => s.Replace(c, '_')).ToString();
    }
}
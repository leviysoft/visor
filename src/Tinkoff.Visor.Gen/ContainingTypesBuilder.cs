using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tinkoff.Visor.Gen
{
    internal static class ContainingTypesBuilder
    {
        static string Indent(int steps) => new string(' ', steps * 4);

        static INamespaceSymbol ContainingNamespace(ISymbol symbol)
        {
            if (symbol is INamespaceSymbol) return symbol as INamespaceSymbol;
            if (symbol is ITypeSymbol) return ContainingNamespace(symbol.ContainingSymbol);
            return null;
        }

        public static string Build(ITypeSymbol symbol, bool buildNested)
        {
            var sb = new StringBuilder();
            var ns = ContainingNamespace(symbol);

            BuildNamespace(ns, sb, (out1) => BuildType(symbol, out1, buildNested));

            return sb.ToString();
        }

        static void BuildNamespace(ISymbol symbol, StringBuilder output, Action<StringBuilder> content)
        {
            var namespaceName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
            output.AppendLine($"namespace {namespaceName} {{");

            content(output);

            output.AppendLine("}");
        }

        static void BuildType(ITypeSymbol symbol, StringBuilder output, bool buildNested, int initialIndent = 0)
        {
            var keyword = symbol.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .First()
                .Keyword
                .ValueText;

            var typeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            output.AppendLine($"{Indent(initialIndent + 1)}partial {keyword} {typeName} {{");

            foreach (var propertySymbol in symbol.GetProperties())
            {
                var propertyName = propertySymbol.ToFQF();

                if (propertyName == "EqualityContract") continue;

                var propertyType = propertySymbol.Type.ToNullableFQF();

                if (propertyType.EndsWith("?"))
                    output.AppendLine("#nullable enable");

                output.AppendLine($"{Indent(initialIndent + 2)}public static global::Tinkoff.Visor.ILens<{symbol.ToFQF()}, {propertyType}> {propertyName}Lens =>");
                output.AppendLine($"{Indent(initialIndent + 3)}global::Tinkoff.Visor.Lens<{symbol.ToFQF()}, {propertyType}>.New(");
                output.AppendLine($"{Indent(initialIndent + 4)}p => p.{propertyName},");
                output.AppendLine($"{Indent(initialIndent + 4)}f => p => p with {{{propertyName} = f(p.{propertyName})}}");
                output.AppendLine($"{Indent(initialIndent + 3)});");

                if (propertyType.EndsWith("?"))
                    output.AppendLine("#nullable disable");

                if (propertySymbol.Type.IsRecord && propertySymbol.NullableAnnotation != NullableAnnotation.Annotated && buildNested)
                    BuildNested(symbol, string.Empty, propertySymbol, output, initialIndent + 1);
            }

            output.AppendLine($"{Indent(initialIndent + 1)}}}");
        }

        static void BuildNested(ITypeSymbol rootType, string suffix, IPropertySymbol symbol, StringBuilder output, int initialIndent)
        {
            output.AppendLine($"{Indent(initialIndent + 1)}public static class Focus{symbol.ToFQF()} {{");

            var basePropertyName = symbol.ToFQF();
            var basePropertyType = symbol.Type.ToNullableFQF();

            foreach (var propertySymbol in symbol.Type.GetProperties())
            {
                var propertyName = propertySymbol.ToFQF();

                if (propertyName == "EqualityContract") continue;

                var propertyType = propertySymbol.Type.ToNullableFQF();

                if (propertyType.EndsWith("?"))
                    output.AppendLine("#nullable enable");

                output.AppendLine($"{Indent(initialIndent + 2)}public static global::Tinkoff.Visor.ILens<{rootType.ToFQF()}, {propertyType}> {propertyName}Lens =>");
                output.AppendLine($"{Indent(initialIndent + 3)}{rootType.ToFQF()}.{suffix}{basePropertyName}Lens.Compose({basePropertyType}.{propertyName}Lens);");

                if (propertyType.EndsWith("?"))
                    output.AppendLine("#nullable disable");

                if (propertySymbol.Type.IsRecord && propertySymbol.NullableAnnotation != NullableAnnotation.Annotated)
                    BuildNested(rootType, $"{suffix}Focus{symbol.ToFQF()}.", propertySymbol, output, initialIndent + 1);
            }

            output.AppendLine($"{Indent(initialIndent + 1)}}}");
        }
    }
}
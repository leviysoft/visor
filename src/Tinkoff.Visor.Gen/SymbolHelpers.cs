using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Tinkoff.Visor.Gen
{
    public static class SymbolHelpers
    {
        public static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol symbol)
        {
            var properties = symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => !x.IsStatic && !x.IsIndexer);
            
            foreach (var property in properties)
                yield return property;
        }

        // ReSharper disable once InconsistentNaming
        public static string ToNullableFQF(this ISymbol symbol) =>
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
                )
            );

        // ReSharper disable once InconsistentNaming
        public static string ToFQF(this ISymbol symbol) =>
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
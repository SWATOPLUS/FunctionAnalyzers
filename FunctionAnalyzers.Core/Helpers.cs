#nullable enable
using FunctionAnalyzers.Core.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FunctionAnalyzers.Core
{
    public static class Helpers
    {
        public static OutcomeList<(ImmutableArray<string>, Location)> GetRules(MethodDeclarationSyntax node, IMethodSymbol symbol, string typeName)
        {
            return node.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => (x.Name as SimpleNameSyntax)?.Identifier.Text == typeName)
                .Select(x => ExtractRule(symbol, x))
                .ToOutcomeList();
        }

        private static Outcome<(ImmutableArray<string>, Location)> ExtractRule(IMethodSymbol method, AttributeSyntax attribute)
        {
            var attributeArgs = attribute.ArgumentList?.Arguments;

            if (attributeArgs == null || attributeArgs.Value.Count < 2)
            {
                return new Outcome<(ImmutableArray<string>, Location)>(Diagnostic.Create(
                    Descriptors.LessThenTwoArguments,
                    attribute.GetLocation()));
            }

            var allParameters = method.Parameters.ToImmutableDictionary(x => x.Name);

            var result = new List<string>();
            var errors = new List<Diagnostic>();

            foreach (var arg in attributeArgs.Value)
            {
                var name = arg.Expression.ToFullString().Trim('"');

                if (allParameters.ContainsKey(name))
                {
                    result.Add(name);
                }
                else
                {
                    errors.Add(Diagnostic.Create(Descriptors.ArgumentShouldReferAParameter, arg.GetLocation()));
                }
            }

            if (errors.Any())
            {
                return new Outcome<(ImmutableArray<string>, Location)>(errors);
            }

            return new Outcome<(ImmutableArray<string>, Location)>((result.ToImmutableArray(), attribute.GetLocation()));
        }
    }
}

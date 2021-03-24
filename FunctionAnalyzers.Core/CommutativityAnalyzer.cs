using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using FunctionAnalyzers.Core.Data;

namespace FunctionAnalyzers.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]

    public class CommutativityAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.ExpressionIsNotCommutative,
            Descriptors.LessThenTwoArguments,
            Descriptors.UnsupportedError,
            Descriptors.ArgumentShouldReferAParameter,
            Descriptors.InternalError);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(node);

            if (symbol == null)
            {
                return;
            }

            var rules = Helpers.GetRules(node, symbol, "Commutative");

            if (rules.Errors.Any())
            {
                foreach (var error in rules.Errors)
                {
                    context.ReportDiagnostic(error);
                }

                return;
            }

            var lambda = ExpressionHelpers.Build(context.Compilation, symbol);
            
            try
            {
                var source = ExpressionHelpers.BuildExpressionTree(lambda?.Body);
                var tree = ExpressionHelpers.SimplifyTree(source);

                foreach (var rule in rules.Results)
                {
                    var result = CheckCommutativity(tree, rule.ElementAt(0), rule.ElementAt(1));

                    if (!result)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.ExpressionIsNotCommutative,
                            node.Body?.GetLocation()));
                    }
                }
            }
            catch (NotSupportedException)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.UnsupportedError,
                    node.GetLocation(),
                    "Method has unsupported constructions"));
            }
        }

        private static bool CheckCommutativity(ExpressionNode node, string parameterA, string parameterB)
        {
            var tree2 = ExpressionHelpers.ReplaceVars(node, parameterA, parameterB);
            var s1 = ExpressionHelpers.StringifyTree(node);
            var s2 = ExpressionHelpers.StringifyTree(tree2);

            return string.Equals(s1, s2, StringComparison.Ordinal);
        }
    }
}

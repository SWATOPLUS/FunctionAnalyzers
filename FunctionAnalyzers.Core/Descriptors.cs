using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace FunctionAnalyzers.Core
{
    public static class Descriptors
    {
        public static DiagnosticDescriptor InternalError { get; } = new(
            "FA000",
            "Internal Error",
            "Internal Analyzer Error! Please, contact with developers!",
            "FunctionAnalysis",
            DiagnosticSeverity.Warning,
            true);

        public static DiagnosticDescriptor UnsupportedError { get; } = new(
            "FA001",
            "Unsupported Error",
            "{0}",
            "FunctionAnalysis",
            DiagnosticSeverity.Warning,
            true);

        public static DiagnosticDescriptor LessThenTwoArguments { get; } = new(
            "FA002",
            "Property should have at least two arguments",
            "Property should have at least two arguments",
            "FunctionAnalysis",
            DiagnosticSeverity.Error,
            true);

        public static DiagnosticDescriptor ArgumentShouldReferAParameter { get; } = new(
            "FA003",
            "Argument should refer a parameter",
            "Argument should refer a parameter",
            "FunctionAnalysis",
            DiagnosticSeverity.Error,
            true);

        public static DiagnosticDescriptor ExpressionIsNotCommutative { get; } = new(
            "FA004",
            "Expression Is Not Commutative",
            "Expression Is Not Commutative",
            "FunctionAnalysis",
            DiagnosticSeverity.Error,
            true);

        public static DiagnosticDescriptor ByName(string descriptorName)
        {
            var prop = typeof(Descriptors).GetProperty(descriptorName, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);

            return prop?.GetValue(null) as DiagnosticDescriptor ?? throw new InvalidOperationException($"Name {descriptorName} not found");
        }
    }
}

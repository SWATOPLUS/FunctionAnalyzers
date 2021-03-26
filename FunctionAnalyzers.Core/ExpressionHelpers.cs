using DelegateDecompiler;
using FunctionAnalyzers.Core.Data;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis.CSharp;

namespace FunctionAnalyzers.Core
{
    public static class ExpressionHelpers
    {
        private static readonly string[] CaOperations = { "Add", "Multiply" };

        public static ExpressionNode BuildExpressionTree(Expression? expression)
        {

            return expression switch
            {
                ParameterExpression pe => new ExpressionNode(pe.Name),
                BinaryExpression be => new ExpressionNode(be.Method?.Name ?? be.NodeType.ToString(), BuildExpressionTree(be.Left), BuildExpressionTree(be.Right)),

                _ => throw new NotSupportedException($"Not supported {expression} of {expression?.GetType().Name}"),
            };
        }

        public static ExpressionNode SimplifyTree(ExpressionNode node)
        {
            if (node.Operands is null)
            {
                return node;
            }

            var children = new List<ExpressionNode>();

            foreach (var operand in node.Operands.Select(SimplifyTree))
            {
                if (node.OperationName == operand.OperationName && CaOperations.Contains(node.OperationName))
                {
                    children.AddRange(operand.Operands!);
                }
                else
                {
                    children.Add(operand);
                }
            }

            return new ExpressionNode(node.OperationName!, children.ToArray());
        }

        public static ExpressionNode ReplaceVars(ExpressionNode node, string a, string b)
        {
            if (node.VariableName == a)
            {
                return new ExpressionNode(b);
            }

            if (node.VariableName == b)
            {
                return new ExpressionNode(a);
            }

            if (node.Operands is null)
            {
                return node;
            }

            return new ExpressionNode(node.OperationName!, node.Operands.Select(x => ReplaceVars(x, a, b)).ToArray());
        }

        public static string StringifyTree(ExpressionNode node)
        {
            if (node.IsVariable)
            {
                return node.VariableName!;
            }

            var builder = new StringBuilder();
            builder.Append(node.OperationName);
            builder.Append("(");

            var ops = node.Operands!.Select(StringifyTree).ToList();

            if (CaOperations.Contains(node.OperationName))
            {
                ops.Sort();
            }

            builder.Append(string.Join(", ", ops));
            builder.Append(")");

            return builder.ToString();
        }

        public static LambdaExpression? Build(Compilation sourceCompilation, IMethodSymbol method)
        {
            var references = new List<MetadataReference>();
            var currentDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in currentDomainAssemblies)
            {
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location) || !File.Exists(assembly.Location))
                    continue;
                
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            // https://stackoverflow.com/questions/35741219/how-to-get-il-of-one-method-body-with-roslyn
            // https://stackoverflow.com/questions/3619386/convert-methodbody-to-expression-tree

            using var assemblyStream = new MemoryStream();
            var result = sourceCompilation
                .RemoveAllReferences()
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .Emit(assemblyStream);

            if (!result.Success)
            {
                return null;
            }

            var asm = Assembly.Load(assemblyStream.ToArray());
            
            var methodBase = asm.GetType(method.ContainingType.FullName()).GetMethod(method.Name);
            var lambda = MethodBodyDecompiler.Decompile(methodBase);

            return lambda;
        }
    }
}

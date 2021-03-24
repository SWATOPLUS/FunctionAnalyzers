using System;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Xunit;

namespace FunctionAnalyzers.Core.UnitTests
{
    public class CommutativityAnalyzerTests
    {
        [Theory]
        [InlineData("[↓Commutative()]", nameof(Descriptors.LessThenTwoArguments))]
        [InlineData("[↓Commutative(\"a\")]", nameof(Descriptors.LessThenTwoArguments))]
        [InlineData("[Commutative(\"b\", ↓6)]", nameof(Descriptors.ArgumentShouldReferAParameter))]
        [InlineData("[Commutative(↓\"c\", \"b\")]", nameof(Descriptors.ArgumentShouldReferAParameter))]
        [InlineData("[Commutative(\"a\", \"b\")]")]
        [InlineData("[Commutative(\"b\", \"a\")]")]
        public void ValidateAttributeTest(string attribute, string descriptor = default)
        {
            var code = @"
                public class TestClass
                {
                    " + attribute + @"
                    public int TestMethod(int a, int b)
                    {
                        return a + b;
                    }
                }
            ";

            Assert(code, descriptor);
        }

        [Fact]
        public void ValidCommutativityTest()
        {
            var code = @"
                public class TestClass
                {
                    [Commutative(""b"", ""a"")]
                    public int TestMethod(int a, int b)
                    {
                        var x = a * b;
                        x *= a + b;

                        return x + a + b;
                    }
                }
            ";

            Assert(code, null);
        }

        [Fact]
        public void InvalidCommutativityTest()
        {
            var code = @"
                public class TestClass
                {
                    [Commutative(""b"", ""a"")]
                    public int TestMethod(int a, int b)
                    ↓{
                        var x = a + b;
                        x *= a / b;

                        return x + a + b;
                    }
                }
            ";

            Assert(code, nameof(Descriptors.ExpressionIsNotCommutative));
        }

        private static void Assert(string code, string descriptorName)
        {
            var analyzer = new CommutativityAnalyzer();
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            };
            var allCode = CommutativeAttribute + code;

            if (descriptorName != null)
            {
                var descriptor = Descriptors.ByName(descriptorName);

                RoslynAssert.Diagnostics(
                    analyzer,
                    ExpectedDiagnostic.Create(descriptor),
                    allCode,
                    metadataReferences: references);
            }
            else
            {
                RoslynAssert.NoAnalyzerDiagnostics(analyzer, allCode);
            }
        }

        private const string CommutativeAttribute = @"
            using System;

            public class CommutativeAttribute : Attribute
            {
                public CommutativeAttribute(params object[] a)
                {
                }
            }
        ";
    }
}

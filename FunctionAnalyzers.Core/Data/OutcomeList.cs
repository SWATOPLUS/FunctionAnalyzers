using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FunctionAnalyzers.Core.Data
{
    public readonly struct OutcomeList<T>
    {
        public OutcomeList(ImmutableArray<T> results, ImmutableArray<Diagnostic> errors)
        {
            Results = results;
            Errors = errors;
        }

        public OutcomeList(IEnumerable<T> results, IEnumerable<Diagnostic> errors)
        {
            Results = results.ToImmutableArray();
            Errors = errors.ToImmutableArray();
        }

        public ImmutableArray<T> Results { get; }

        public ImmutableArray<Diagnostic> Errors { get; }
    }
}
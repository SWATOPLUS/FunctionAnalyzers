using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FunctionAnalyzers.Core.Data
{
    public readonly struct Outcome<T>
    {
        public Outcome(T result)
        {
            Result = result;
            Errors = default;
        }

        public Outcome(params Diagnostic[] errors)
        {
            Result = default;
            Errors = errors.ToImmutableArray();
        }

        public Outcome(IEnumerable<Diagnostic> errors)
        {
            Result = default;
            Errors = errors.ToImmutableArray();
        }

        public T Result { get; }

        public ImmutableArray<Diagnostic> Errors { get; }

        public bool HasResult => Errors.IsDefaultOrEmpty;
    }
}

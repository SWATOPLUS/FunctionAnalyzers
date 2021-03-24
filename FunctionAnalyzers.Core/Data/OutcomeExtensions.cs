using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FunctionAnalyzers.Core.Data
{
    public static class OutcomeExtensions
    {
        public static OutcomeList<TOut> Apply<TIn, TOut>(this OutcomeList<TIn> list, Func<TIn, Outcome<TOut>> func)
        {
            var results = new List<TOut>();
            var errors = new List<Diagnostic>();

            foreach (var item in list.Results)
            {
                var outcome = func.Invoke(item);

                if (outcome.HasResult)
                {
                    results.Add(outcome.Result);
                }
                else
                {
                    errors.AddRange(outcome.Errors);
                }
            }

            return new OutcomeList<TOut>(
                results.ToImmutableArray(),
                list.Errors.Concat(errors).ToImmutableArray());
        }

        public static OutcomeList<T> ToOutcomeList<T>(this IEnumerable<Outcome<T>> source)
        {
            var results = new List<T>();
            var errors = new List<Diagnostic>();

            foreach (var item in source)
            {
                if (item.HasResult)
                {
                    results.Add(item.Result);
                }
                else
                {
                    errors.AddRange(item.Errors);
                }
            }

            return new OutcomeList<T>(results, errors);
        }
    }
}
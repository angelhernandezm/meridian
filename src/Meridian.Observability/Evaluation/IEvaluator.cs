// ============================================================================
// Meridian
// File: IEvaluator.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The evaluation abstraction: scores a model/agent response against a rubric,
// returning a pass/fail verdict with an explanatory reason. Meridian ships a
// couple of trivial built-ins; hosts plug in their own (regex, LLM-as-judge,
// a third-party eval SDK) by implementing IEvaluator<TInput, TOutput>.
//
// License: MIT
// ============================================================================
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ============================================================================

namespace Meridian.Observability.Evaluation;

/// <summary>
/// The outcome of running an <see cref="IEvaluator{TInput, TOutput}" />.
/// </summary>
/// <param name="EvaluatorName">The name of the evaluator that produced this result.</param>
/// <param name="Passed">Whether the output passed the evaluator's rubric.</param>
/// <param name="Score">A numeric score for the output; conventionally in [0, 1], but evaluator-defined.</param>
/// <param name="Reason">An explanation for the verdict, or <see langword="null" /> when none is given (typically on pass).</param>
public sealed record EvaluationResult(string EvaluatorName, bool Passed, double Score, string? Reason = null);

/// <summary>
/// Scores a model/agent output, optionally in light of the input that produced it.
/// </summary>
/// <typeparam name="TInput">The type of the input that produced the output, when relevant to scoring.</typeparam>
/// <typeparam name="TOutput">The type of the output being scored.</typeparam>
public interface IEvaluator<in TInput, in TOutput> {
    /// <summary>
    /// Gets the evaluator's name, surfaced in <see cref="EvaluationResult.EvaluatorName" />.
    /// </summary>
    /// <value>The evaluator's name.</value>
    string Name { get; }

    /// <summary>
    /// Scores <paramref name="output" />, optionally in light of <paramref name="input" />.
    /// </summary>
    /// <param name="input">The input that produced <paramref name="output" />.</param>
    /// <param name="output">The output to score.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evaluation result.</returns>
    Task<EvaluationResult> EvaluateAsync(TInput input, TOutput output, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience alias for evaluators that only need the text response, not the originating input.
/// </summary>
public interface ITextEvaluator : IEvaluator<string?, string> {
    /// <inheritdoc />
    Task<EvaluationResult> IEvaluator<string?, string>.EvaluateAsync(string? input, string output, CancellationToken cancellationToken) =>
        EvaluateAsync(output, cancellationToken);

    /// <summary>
    /// Scores <paramref name="output" />.
    /// </summary>
    /// <param name="output">The output to score.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evaluation result.</returns>
    Task<EvaluationResult> EvaluateAsync(string output, CancellationToken cancellationToken = default);
}
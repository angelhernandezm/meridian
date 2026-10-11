// ============================================================================
// Meridian
// File: KeywordPresenceEvaluator.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Built-in evaluator that passes when a response contains (or, inverted, omits)
// at least one of a configured set of keywords. Useful for cheap grounding or
// policy checks (e.g. "the refusal phrase must appear" / "must not mention X").
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
/// Built-in evaluator that passes when a response contains (or, inverted, omits) at least one of a
/// configured set of keywords.
/// </summary>
/// <param name="keywords">The keywords to check for. Must contain at least one entry.</param>
/// <param name="requireAbsence">When <see langword="true" />, the evaluator passes only when none of <paramref name="keywords" /> are present; otherwise it passes when at least one is.</param>
public sealed class KeywordPresenceEvaluator(IReadOnlyList<string> keywords, bool requireAbsence = false) : ITextEvaluator {
    /// <summary>
    /// The keywords
    /// </summary>
    private readonly IReadOnlyList<string> _keywords = keywords is { Count: > 0 }
        ? keywords
        : throw new ArgumentException("At least one keyword is required.", nameof(keywords));

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name => "KeywordPresence";

    /// <summary>
    /// Evaluates the asynchronous.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>System.Threading.Tasks.Task{Meridian.Observability.Evaluation.EvaluationResult}.</returns>
    public Task<EvaluationResult> EvaluateAsync(string output, CancellationToken cancellationToken = default) {
        var found = _keywords.FirstOrDefault(k => (output ?? string.Empty).Contains(k, StringComparison.OrdinalIgnoreCase));
        var passed = requireAbsence ? found is null : found is not null;

        var reason = passed
            ? null
            : requireAbsence
                ? $"Forbidden keyword \"{found}\" was present."
                : $"None of the required keywords [{string.Join(", ", _keywords)}] were present.";

        return Task.FromResult(new EvaluationResult(Name, passed, Score: passed ? 1 : 0, reason));
    }
}
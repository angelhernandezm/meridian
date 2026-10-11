// ============================================================================
// Meridian
// File: TextLengthEvaluator.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Built-in evaluator that flags responses outside a configured character-count
// range. Useful as a cheap guardrail against empty or runaway completions.
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
/// Built-in evaluator that flags responses outside a configured character-count range.
/// </summary>
/// <param name="minLength">The minimum acceptable length, in characters.</param>
/// <param name="maxLength">The maximum acceptable length, in characters.</param>
public sealed class TextLengthEvaluator(int minLength = 1, int maxLength = 8000) : ITextEvaluator {
    public string Name => "TextLength";

    /// <summary>
    /// Scores <paramref name="output" />.
    /// </summary>
    /// <param name="output">The output to score.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evaluation result.</returns>
    public Task<EvaluationResult> EvaluateAsync(string output, CancellationToken cancellationToken = default) {
        var length = output?.Length ?? 0;
        var passed = length >= minLength && length <= maxLength;
        var reason = passed ? null : $"Length {length} is outside the allowed range [{minLength}, {maxLength}].";
        return Task.FromResult(new EvaluationResult(Name, passed, Score: passed ? 1 : 0, reason));
    }
}

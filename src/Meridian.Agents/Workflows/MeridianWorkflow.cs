// ============================================================================
// Meridian
// File: MeridianWorkflow.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A sequential pipeline of IWorkflowStep instances: each step's output becomes
// the next step's input. Returns the final output plus a per-step trace so hosts
// can inspect or log what each stage produced.
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

namespace Meridian.Agents.Workflows;

/// <summary>
/// One step's recorded output within a completed workflow run.
/// </summary>
/// <param name="StepName">The step's name.</param>
/// <param name="Output">The step's output.</param>
public sealed record WorkflowStepTrace(string StepName, string Output);

/// <summary>
/// The result of running a <see cref="MeridianWorkflow" />: the final output plus the full step trace.
/// </summary>
/// <param name="Output">The final step's output.</param>
/// <param name="Trace">Every step's output, in execution order.</param>
public sealed record WorkflowResult(string Output, IReadOnlyList<WorkflowStepTrace> Trace);

/// <summary>
/// Runs a fixed, ordered sequence of <see cref="IWorkflowStep" /> instances.
/// </summary>
/// <param name="steps">The ordered steps to run. Must contain at least one step.</param>
public sealed class MeridianWorkflow(IReadOnlyList<IWorkflowStep> steps) {
    /// <summary>
    /// The steps
    /// </summary>
    private readonly IReadOnlyList<IWorkflowStep> _steps = steps is { Count: > 0 }
        ? steps
        : throw new ArgumentException("A workflow needs at least one step.", nameof(steps));

    /// <summary>
    /// Runs every step in order, feeding each step's output to the next.
    /// </summary>
    /// <param name="input">The workflow's initial input.</param>
    /// <param name="cancellationToken">A token to cancel the operation; checked between steps.</param>
    /// <returns>The final output and the full per-step trace.</returns>
    public async Task<WorkflowResult> RunAsync(string input, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(input);

        var trace = new List<WorkflowStepTrace>(_steps.Count);
        var current = input;

        foreach (var step in _steps) {
            cancellationToken.ThrowIfCancellationRequested();
            current = await step.ExecuteAsync(current, cancellationToken).ConfigureAwait(false);
            trace.Add(new WorkflowStepTrace(step.Name, current));
        }

        return new WorkflowResult(current, trace);
    }
}
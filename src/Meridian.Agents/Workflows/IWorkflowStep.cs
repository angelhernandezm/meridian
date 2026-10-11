// ============================================================================
// Meridian
// File: IWorkflowStep.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A single unit of work in a MeridianWorkflow: takes the previous step's text
// output (or the workflow's initial input) and produces the next one.
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
/// A named, executable step in a <see cref="MeridianWorkflow" />.
/// </summary>
public interface IWorkflowStep {
    /// <summary>
    /// Gets the step's name, recorded in <see cref="WorkflowStepTrace.StepName" />.
    /// </summary>
    /// <value>The step's name.</value>
    string Name { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="input">The previous step's output, or the workflow's initial input for the first step.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>This step's output, fed to the next step (or returned as the workflow's final output).</returns>
    Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}
// ============================================================================
// Meridian
// File: AgentWorkflowStep.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A workflow step backed by a MAF AIAgent: runs the agent once per execution in
// a fresh session, feeding it the previous step's output and returning its
// response text as this step's output.
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

using Microsoft.Agents.AI;

namespace Meridian.Agents.Workflows;

/// <summary>
/// A workflow step backed by a MAF <see cref="AIAgent" />.
/// </summary>
/// <param name="name">The step's name.</param>
/// <param name="agent">The agent to run for this step.</param>
public sealed class AgentWorkflowStep(string name, AIAgent agent) : IWorkflowStep {
    /// <summary>
    /// The agent
    /// </summary>
    private readonly AIAgent _agent = agent ?? throw new ArgumentNullException(nameof(agent));

    /// <summary>
    /// Gets the step's name, recorded in <see cref="WorkflowStepTrace.StepName" />.
    /// </summary>
    /// <value>The step's name.</value>
    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("A step name is required.", nameof(name)) : name;

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="input">The previous step's output, or the workflow's initial input for the first step.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;System.String&gt; representing the asynchronous operation.</returns>
    public async Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default) {
        var session = await _agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false);
        var response = await _agent.RunAsync(input, session, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Text;
    }
}
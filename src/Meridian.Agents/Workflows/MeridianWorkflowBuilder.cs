// ============================================================================
// Meridian
// File: MeridianWorkflowBuilder.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Fluent builder for assembling a MeridianWorkflow out of agent steps and
// arbitrary transform steps.
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
/// A workflow step that applies a synchronous or asynchronous transform instead of calling an agent.
/// </summary>
/// <param name="name">The step's name.</param>
/// <param name="execute">The transform to run for this step.</param>
public sealed class DelegateWorkflowStep(string name, Func<string, CancellationToken, Task<string>> execute) : IWorkflowStep {
    /// <summary>
    /// The execute
    /// </summary>
    private readonly Func<string, CancellationToken, Task<string>> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    
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
    /// <returns>This step's output, fed to the next step (or returned as the workflow's final output).</returns>
    public Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default) => _execute(input, cancellationToken);
}

/// <summary>Fluent builder for a <see cref="MeridianWorkflow"/>.</summary>
public sealed class MeridianWorkflowBuilder {
    private readonly List<IWorkflowStep> _steps = [];

    /// <summary>
    /// Appends a step to the workflow.
    /// </summary>
    /// <param name="step">The step to append.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianWorkflowBuilder AddStep(IWorkflowStep step) {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Appends a step backed by a MAF agent.
    /// </summary>
    /// <param name="name">The step's name.</param>
    /// <param name="agent">The agent to run for this step.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianWorkflowBuilder AddAgentStep(string name, AIAgent agent) => AddStep(new AgentWorkflowStep(name, agent));

    /// <summary>
    /// Appends a step that applies an asynchronous transform.
    /// </summary>
    /// <param name="name">The step's name.</param>
    /// <param name="execute">The asynchronous transform to run for this step.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianWorkflowBuilder AddTransformStep(string name, Func<string, CancellationToken, Task<string>> execute) =>
        AddStep(new DelegateWorkflowStep(name, execute));

    /// <summary>
    /// Appends a step that applies a synchronous transform.
    /// </summary>
    /// <param name="name">The step's name.</param>
    /// <param name="execute">The synchronous transform to run for this step.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianWorkflowBuilder AddTransformStep(string name, Func<string, string> execute) =>
        AddStep(new DelegateWorkflowStep(name, (input, _) => Task.FromResult(execute(input))));

    /// <summary>
    /// Builds the <see cref="MeridianWorkflow"/> from the steps added so far.
    /// </summary>
    /// <returns>The built <see cref="MeridianWorkflow"/>.</returns>
    public MeridianWorkflow Build() => new(_steps);
}
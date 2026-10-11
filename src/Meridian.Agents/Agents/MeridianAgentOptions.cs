// ============================================================================
// Meridian
// File: MeridianAgentOptions.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Configuration for a single named agent: its instructions, which model
// provider backs it, which tools it can call, and which pipeline opt-ins
// (tracing/logging) MeridianAgentFactory applies when building it.
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

using Meridian.Models.Providers;
using Microsoft.Extensions.AI;

namespace Meridian.Agents.Agents;

/// <summary>
/// Configures a single named Meridian agent.
/// </summary>
public sealed class MeridianAgentOptions {
    /// <summary>
    /// Gets or sets the agent's name, surfaced to the model and used as its registration key.
    /// </summary>
    /// <value>The agent's name. Required.</value>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the system instructions (persona, rules, guardrails) given to the agent.
    /// </summary>
    /// <value>The instructions, or <see langword="null" /> for none.</value>
    public string? Instructions { get; set; }

    /// <summary>
    /// Gets or sets a human-readable description of what the agent is for.
    /// </summary>
    /// <value>The description, or <see langword="null" /> for none.</value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets which configured model provider backs this agent. <see cref="ModelProvider.Auto" /> uses the configured default.
    /// </summary>
    /// <value>The provider. Defaults to <see cref="ModelProvider.Auto" />.</value>
    public ModelProvider Provider { get; set; } = ModelProvider.Auto;

    /// <summary>
    /// Gets the tools (by name, resolved against the <c>IMeridianToolCatalog</c>) this agent can call.
    /// </summary>
    /// <value>The tool names. Empty by default.</value>
    public IList<string> ToolNames { get; } = new List<string>();

    /// <summary>
    /// Gets additional tools supplied directly, bypassing the catalog.
    /// </summary>
    /// <value>The directly-supplied tools. Empty by default.</value>
    public IList<AITool> Tools { get; } = new List<AITool>();

    /// <summary>
    /// Gets or sets whether a "search" tool over the registered <c>IRetriever</c> (semantic or hybrid,
    /// whichever <c>options.AddRetrieval()</c>/<c>AddHybridRetrieval()</c> registered) is attached to this agent.
    /// </summary>
    /// <value><see langword="true" /> to attach the search tool; otherwise <see langword="false" /> (the default).</value>
    public bool EnableSearchTool { get; set; }

    /// <summary>
    /// Gets or sets the name the model sees for the search tool when <see cref="EnableSearchTool" /> is set.
    /// </summary>
    /// <value>The tool name. Defaults to <c>"search"</c>.</value>
    public string SearchToolName { get; set; } = "search";

    /// <summary>
    /// Gets or sets the chat options (temperature, max tokens, ...) applied to every run.
    /// </summary>
    /// <value>The chat options, or <see langword="null" /> to use provider defaults.</value>
    public ChatOptions? ChatOptions { get; set; }

    /// <summary>
    /// Gets or sets whether MAF's OpenTelemetry agent middleware is applied.
    /// </summary>
    /// <value><see langword="true" /> to emit OpenTelemetry spans for agent runs; otherwise <see langword="false" /> (the default).</value>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// Gets or sets whether MAF's logging agent middleware is applied (requires an <c>ILoggerFactory</c> in DI).
    /// </summary>
    /// <value><see langword="true" /> to log agent runs; otherwise <see langword="false" /> (the default).</value>
    public bool EnableLogging { get; set; }
}

// ============================================================================
// Meridian
// File: MeridianAgentFactory.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Builds a MAF AIAgent (ChatClientAgent) from a MeridianAgentOptions: resolves
// the configured model provider, attaches tools, and layers on MAF's own
// OpenTelemetry/logging agent middleware via AIAgentBuilder when enabled.
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

using Meridian.Agents.Tools;
using Meridian.Knowledge.Retrieval;
using Meridian.Models.Providers;
using Meridian.Observability.Tracing;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meridian.Agents.Agents;

/// <summary>Builds <see cref="AIAgent"/> instances from <see cref="MeridianAgentOptions"/>.</summary>
/// <param name="clientResolver">Resolves the model client each agent is built against.</param>
/// <param name="toolCatalog">Resolves tools referenced by name in <see cref="MeridianAgentOptions.ToolNames"/>.</param>
/// <param name="services">The service provider passed to <see cref="ChatClientAgent"/> and the MAF pipeline builder.</param>
/// <param name="loggerFactory">The logger factory used when <see cref="MeridianAgentOptions.EnableLogging"/> is set, or <see langword="null"/> to disable logging regardless.</param>
public sealed class MeridianAgentFactory(
    IModelClientResolver clientResolver,
    IMeridianToolCatalog toolCatalog,
    IServiceProvider services,
    ILoggerFactory? loggerFactory = null) {
    /// <summary>
    /// The client resolver
    /// </summary>
    private readonly IModelClientResolver _clientResolver = clientResolver ?? throw new ArgumentNullException(nameof(clientResolver));
    /// <summary>
    /// The tool catalog
    /// </summary>
    private readonly IMeridianToolCatalog _toolCatalog = toolCatalog ?? throw new ArgumentNullException(nameof(toolCatalog));
    /// <summary>
    /// The services
    /// </summary>
    private readonly IServiceProvider _services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Builds an <see cref="AIAgent"/> from <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The agent configuration to build from.</param>
    /// <returns>The built agent, with tools attached and the configured pipeline (tracing/logging) applied.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="options"/> references an unregistered tool name, or <see cref="MeridianAgentOptions.EnableSearchTool"/>
    /// is set but no <c>IRetriever</c> is registered.
    /// </exception>
    public AIAgent Create(MeridianAgentOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        var clients = _clientResolver.Resolve(options.Provider);

        var tools = new List<AITool>(options.Tools);
        foreach (var toolName in options.ToolNames) {
            if (_toolCatalog.TryGet(toolName, out var tool))
                tools.Add(tool);
            else
                throw new InvalidOperationException($"Agent \"{options.Name}\" references unknown tool \"{toolName}\". Register it first with options.AddTool(...).");
        }

        if (options.EnableSearchTool) {
            var retriever = _services.GetService<IRetriever>()
                ?? throw new InvalidOperationException($"Agent \"{options.Name}\" has EnableSearchTool set, but no IRetriever is registered. Call options.AddRetrieval() or options.AddHybridRetrieval() first.");
            tools.Add(KnowledgeTools.Search(retriever, options.SearchToolName));
        }

        AIAgent agent = new ChatClientAgent(
            clients.Chat,
            instructions: options.Instructions,
            name: options.Name,
            description: options.Description,
            tools: tools.Count > 0 ? tools : null,
            loggerFactory: loggerFactory,
            services: _services);

        if (!options.EnableTracing && !options.EnableLogging)
            return agent;

        var builder = agent.AsBuilder();

        if (options.EnableTracing)
            builder.UseOpenTelemetry(sourceName: MeridianTelemetry.SourceName);

        if (options.EnableLogging && loggerFactory is not null)
            builder.UseLogging(loggerFactory);

        return builder.Build(_services);
    }
}

// ============================================================================
// Meridian
// File: MeridianBuilder.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The fluent configuration surface passed to services.AddMeridian(options => ...).
// Each Add* method wires one subsystem (Models, Knowledge, Agents, Observability)
// into the underlying IServiceCollection, mirroring the Add*-per-concern shape
// of a typical ASP.NET Core / Azure Functions startup.
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

using Meridian.Agents.Agents;
using Meridian.Agents.Memory;
using Meridian.Agents.Tools;
using Meridian.Agents.Workflows;
using Meridian.Context;
using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.Retrieval;
using Meridian.Knowledge.SemanticStore;
using Meridian.Models.Providers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meridian.Hosting;

/// <summary>
/// Fluent builder passed to <c>services.AddMeridian(options =&gt; ...)</c>. Each method registers one
/// subsystem's services; registrations resolve their dependencies lazily, so Add* calls may appear
/// in any order as long as everything a given subsystem needs is added before the host resolves it.
/// </summary>
public sealed class MeridianBuilder {
    /// <summary>
    /// The tool catalog
    /// </summary>
    private readonly MeridianToolCatalog _toolCatalog = new();
    /// <summary>
    /// The agent options by name
    /// </summary>
    private readonly Dictionary<string, MeridianAgentOptions> _agentOptionsByName = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// The workflow factories by name
    /// </summary>
    private readonly Dictionary<string, Func<IServiceProvider, MeridianWorkflow>> _workflowFactoriesByName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the underlying service collection every Add* method registers into.
    /// </summary>
    /// <value>The service collection passed to <c>AddMeridian</c>.</value>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MeridianBuilder" /> class, registering the services
    /// every Meridian host needs regardless of which optional subsystems are opted into.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    internal MeridianBuilder(IServiceCollection services) {
        Services = services;
        Services.AddSingleton<IMeridianContextAccessor, MeridianContextAccessor>();
        Services.AddSingleton<IMeridianToolCatalog>(_toolCatalog);

        Services.AddSingleton<IMeridianAgentRegistry>(sp => new MeridianAgentRegistry(
            new MeridianAgentFactory(
                sp.GetRequiredService<IModelClientResolver>(),
                sp.GetRequiredService<IMeridianToolCatalog>(),
                sp,
                sp.GetService<ILoggerFactory>()),
            _agentOptionsByName));

        Services.AddSingleton<IMeridianWorkflowRegistry>(sp => new MeridianWorkflowRegistry(sp, _workflowFactoriesByName));
    }

    /// <summary>
    /// Registers Meridian.Models: the configured <see cref="ModelOptions" />, the resulting
    /// <see cref="IChatClient" /> (fallback + pipeline applied) and <see cref="IModelClientResolver" />.
    /// </summary>
    /// <param name="configure">A callback to configure <see cref="ModelOptions" />, or <see langword="null" /> to use defaults.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddModel(Action<ModelOptions>? configure = null) {
        var options = new ModelOptions();
        configure?.Invoke(options);
        Services.AddSingleton(options);

        Services.AddSingleton<IChatClient>(sp => {
            var (chat, _) = ModelClientBuilder.BuildChatWithFallback(options);
            return ModelClientBuilder.ApplyChatPipeline(chat, options, sp);
        });

        Services.AddSingleton<IModelClientResolver>(sp => new ModelClientResolver(options, sp));
        return this;
    }

    /// <summary>
    /// Registers Meridian.Knowledge's <see cref="IEmbeddingService" />, sourced from the provider
    /// configured in <see cref="EmbeddingsOptions.Provider" /> (defaults to the chat default provider).
    /// Requires <see cref="AddModel" /> to have been called.
    /// </summary>
    /// <param name="configure">A callback to configure <see cref="EmbeddingsOptions" />, or <see langword="null" /> to use defaults.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddEmbeddings(Action<EmbeddingsOptions>? configure = null) {
        var options = new EmbeddingsOptions();
        configure?.Invoke(options);

        Services.AddSingleton<IEmbeddingService>(sp => {
            var resolver = sp.GetRequiredService<IModelClientResolver>();
            var clients = resolver.Resolve(options.Provider);
            var generator = clients.Embeddings
                ?? throw new InvalidOperationException($"Provider \"{clients.ProviderName}\" has no embedding generator configured. Set EmbeddingsOptions.Provider to one that does (OpenAI, Gemini, AzureOpenAI, AzureAIInference).");
            return new EmbeddingService(generator);
        });
        return this;
    }

    /// <summary>
    /// Registers Meridian.Models' <see cref="IMeridianImageService"/> (generation, editing, vision analysis),
    /// sourced from the provider configured in <see cref="EmbeddingsOptions.Provider"/>-style selection via
    /// <paramref name="provider"/>. Only AzureOpenAI and OpenAI support image generation/editing today; set
    /// the corresponding <c>ImageDeploymentName</c> / <c>ImageModel</c> on <see cref="ModelOptions"/>.
    /// Requires <see cref="AddModel"/> to have been called.
    /// </summary>
    /// <param name="provider">The provider to source image generation/editing from. <see cref="ModelProvider.Auto"/> uses the chat default provider.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddImages(ModelProvider provider = ModelProvider.Auto) {
        Services.AddSingleton<IMeridianImageService>(sp => {
            var resolver = sp.GetRequiredService<IModelClientResolver>();
            var clients = resolver.Resolve(provider);
            var imageGenerator = clients.Images
                ?? throw new InvalidOperationException($"Provider \"{clients.ProviderName}\" has no image generator configured. Image generation/editing is supported for AzureOpenAI (set AzureOpenAI.ImageDeploymentName) and OpenAI (set OpenAI.ImageModel).");
            return new MeridianImageService(clients.Chat, imageGenerator);
        });
        return this;
    }

    /// <summary>
    /// Registers the default, process-local <see cref="InMemorySemanticStore" /> as <see cref="ISemanticStore" />.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddSemanticStore() => AddSemanticStore(_ => new InMemorySemanticStore());

    /// <summary>
    /// Registers <typeparamref name="TStore" /> as <see cref="ISemanticStore" />, resolved through DI.
    /// </summary>
    /// <typeparam name="TStore">The concrete <see cref="ISemanticStore" /> implementation to register.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddSemanticStore<TStore>() where TStore : class, ISemanticStore {
        Services.AddSingleton<ISemanticStore, TStore>();
        return this;
    }

    /// <summary>
    /// Registers a custom <see cref="ISemanticStore" /> built by <paramref name="factory" />.
    /// </summary>
    /// <param name="factory">A factory that builds the store from the resolved <see cref="IServiceProvider" />.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public MeridianBuilder AddSemanticStore(Func<IServiceProvider, ISemanticStore> factory) {
        ArgumentNullException.ThrowIfNull(factory);
        Services.AddSingleton(factory);
        return this;
    }

    /// <summary>
    /// Registers Meridian.Knowledge's <see cref="IRetriever" /> as pure vector similarity search.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddRetrieval() {
        Services.AddSingleton<IRetriever>(sp => new SemanticRetriever(
            sp.GetRequiredService<IEmbeddingService>(),
            sp.GetRequiredService<ISemanticStore>()));
        return this;
    }

    /// <summary>
    /// Registers Meridian.Knowledge's <see cref="IRetriever" /> as a <see cref="HybridRetriever" />: vector
    /// similarity fused with keyword overlap, weighted by <paramref name="semanticWeight" />/<paramref name="keywordWeight" />.
    /// Use this instead of <see cref="AddRetrieval" />, not alongside it — the last one registered wins.
    /// </summary>
    /// <param name="semanticWeight">The weight given to each candidate's semantic (vector similarity) score during fusion.</param>
    /// <param name="keywordWeight">The weight given to each candidate's keyword (lexical overlap) score during fusion.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddHybridRetrieval(double semanticWeight = 0.6, double keywordWeight = 0.4) {
        Services.AddSingleton<IRetriever>(sp => new HybridRetriever(
            sp.GetRequiredService<IEmbeddingService>(),
            sp.GetRequiredService<ISemanticStore>(),
            semanticWeight,
            keywordWeight));
        return this;
    }

    /// <summary>
    /// Registers Meridian.Knowledge's <see cref="RagContextBuilder" />: the batteries-included
    /// retrieval → numbered context → prompt-template pipeline, built from the registered <see cref="IRetriever" />.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddRagContextBuilder() {
        Services.AddSingleton(sp => new RagContextBuilder(sp.GetRequiredService<IRetriever>()));
        return this;
    }

    /// <summary>
    /// Registers Meridian.Agents' long-term <see cref="IMeridianMemory" />, built from the registered embeddings + semantic store.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddMemory() {
        Services.AddSingleton<IMeridianMemory>(sp => new SemanticMemory(
            sp.GetRequiredService<IEmbeddingService>(),
            sp.GetRequiredService<ISemanticStore>()));
        return this;
    }

    /// <summary>
    /// Registers a tool, callable by name from any <see cref="MeridianAgentOptions.ToolNames" />.
    /// </summary>
    /// <param name="name">The name to register the tool under, and the name the model sees.</param>
    /// <param name="method">The delegate to wrap as the tool's implementation.</param>
    /// <param name="description">The tool description surfaced to the model, or <see langword="null" /> to use the method's XML doc summary, if any.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddTool(string name, Delegate method, string? description = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _toolCatalog.Register(name, MeridianToolBuilder.FromDelegate(method, name, description));
        return this;
    }

    /// <summary>
    /// Registers a named agent, resolvable later via <see cref="IMeridianAgentRegistry" />.
    /// </summary>
    /// <param name="name">The name to register the agent under.</param>
    /// <param name="configure">A callback to configure the agent's <see cref="MeridianAgentOptions" />.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public MeridianBuilder AddAgent(string name, Action<MeridianAgentOptions> configure) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new MeridianAgentOptions { Name = name };
        configure(options);
        _agentOptionsByName[name] = options;
        return this;
    }

    /// <summary>
    /// Registers a named workflow, resolvable later via <see cref="IMeridianWorkflowRegistry" />.
    /// </summary>
    /// <param name="name">The name to register the workflow under.</param>
    /// <param name="factory">A factory that builds the workflow from the resolved <see cref="IServiceProvider" /> (typically using <see cref="IMeridianAgentRegistry" /> and <see cref="MeridianWorkflowBuilder" />).</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public MeridianBuilder AddWorkflow(string name, Func<IServiceProvider, MeridianWorkflow> factory) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(factory);
        _workflowFactoriesByName[name] = factory;
        return this;
    }

    /// <summary>
    /// Registers the evaluators Observability runs against agent/model responses.
    /// </summary>
    /// <param name="configure">A callback to configure <see cref="ObservabilityOptions" />, or <see langword="null" /> to register no evaluators.</param>
    /// <returns>This builder, for chaining.</returns>
    public MeridianBuilder AddObservability(Action<ObservabilityOptions>? configure = null) {
        var options = new ObservabilityOptions();
        configure?.Invoke(options);
        Services.AddSingleton<IReadOnlyList<Meridian.Observability.Evaluation.ITextEvaluator>>(options.Evaluators.ToArray());
        return this;
    }
}
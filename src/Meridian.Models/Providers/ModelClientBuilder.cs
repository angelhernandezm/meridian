// ============================================================================
// Meridian
// File: ModelClientBuilder.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Builds Microsoft.Extensions.AI chat/embedding/image clients from ModelOptions
// for each supported provider, composes multi-provider fallback, and applies
// the opt-in middleware pipeline (retry/cache/OpenTelemetry/logging) around the
// resulting chat client.
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

using Azure;
using Azure.AI.Inference;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Meridian.Models.Providers;

/// <summary>
/// Builds the Microsoft.Extensions.AI clients backing Meridian.Models from <see cref="ModelOptions" />.
/// Provider selection is config-driven via <see cref="ModelOptions.DefaultProvider" />.
/// </summary>
public static class ModelClientBuilder {
    /// <summary>
    /// Builds the clients for <see cref="ModelOptions.DefaultProvider" />.
    /// </summary>
    /// <param name="options">The provider options to build clients from.</param>
    /// <returns>The <see cref="ModelClients" /> bundle for the default provider.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"><paramref name="options" /> is <see langword="null" />.</exception>
    public static ModelClients Build(ModelOptions options) {
        ArgumentNullException.ThrowIfNull(options);
        return BuildFor(options.DefaultProvider, options);
    }

    /// <summary>
    /// Builds the clients for a specific <paramref name="provider" />, used for both the default and each fallback provider.
    /// </summary>
    /// <param name="provider">The provider to build clients for.</param>
    /// <param name="options">The provider options to build clients from.</param>
    /// <returns>The <see cref="ModelClients" /> bundle for <paramref name="provider" />.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"><paramref name="options" /> is <see langword="null" />.</exception>
    public static ModelClients BuildFor(ModelProvider provider, ModelOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        return provider switch {
            ModelProvider.OpenAI => BuildOpenAI(options.OpenAI),
            ModelProvider.Anthropic => BuildAnthropic(options.Anthropic),
            ModelProvider.Gemini => BuildGemini(options.Gemini),
            ModelProvider.Perplexity => BuildPerplexity(options.Perplexity),
            ModelProvider.AzureAIInference => BuildAzureAIInference(options.AzureAIInference),
            // AzureOpenAI is the default; Auto and any unmapped provider also resolve to it.
            _ => BuildAzureOpenAI(options.AzureOpenAI),
        };
    }

    /// <summary>
    /// Builds the chat client for <see cref="ModelOptions.DefaultProvider" />, wrapping it in a
    /// <see cref="FallbackChatClient" /> when <see cref="ModelOptions.FallbackProviders" /> is non-empty.
    /// Does not apply the middleware pipeline; see <see cref="ApplyChatPipeline" />.
    /// </summary>
    /// <param name="options">The provider options to build the chat client from.</param>
    /// <returns>A tuple of the (optionally fallback-wrapped) chat client and the default provider's label.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public static (IChatClient Chat, string ProviderName) BuildChatWithFallback(ModelOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        var primary = BuildFor(options.DefaultProvider, options);
        if (options.FallbackProviders.Count == 0)
            return (primary.Chat, primary.ProviderName);

        var ordered = new List<(IChatClient Chat, string Name)> { (primary.Chat, primary.ProviderName) };
        foreach (var fallback in options.FallbackProviders) {
            if (fallback == options.DefaultProvider)
                continue;
            var built = BuildFor(fallback, options);
            ordered.Add((built.Chat, built.ProviderName));
        }

        return (new FallbackChatClient(ordered), primary.ProviderName);
    }

    /// <summary>
    /// Applies the configured middleware pipeline around <paramref name="inner" />: retry, distributed
    /// caching, OpenTelemetry, then logging. Each stage is opt-in via <see cref="ModelOptions.Pipeline" />;
    /// a disabled stage adds no wrapper.
    /// </summary>
    /// <param name="inner">The chat client to wrap.</param>
    /// <param name="options">The provider options carrying the pipeline configuration.</param>
    /// <param name="services">The service provider used to resolve pipeline dependencies (<c>ILoggerFactory</c>, <c>IDistributedCache</c>), or <see langword="null" /> to skip stages that need them.</param>
    /// <returns>The pipeline-wrapped chat client.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public static IChatClient ApplyChatPipeline(IChatClient inner, ModelOptions options, IServiceProvider? services) {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(options);

        var pipeline = options.Pipeline;
        var builder = inner.AsBuilder();

        // Order matters: the outermost stage runs first. Retry wraps the inner client so a single attempt
        // is retried; caching short-circuits before retry; logging/OTel observe the whole call.
        if (pipeline.EnableRetry)
            builder.UseRetry(pipeline.MaxRetries, pipeline.RetryBaseDelay);

        if (pipeline.EnableDistributedCache && services?.GetService(typeof(IDistributedCache)) is IDistributedCache cache)
            builder.UseDistributedCache(cache);

        if (pipeline.EnableOpenTelemetry)
            builder.UseOpenTelemetry(sourceName: pipeline.OpenTelemetrySourceName);

        if (pipeline.EnableLogging && services?.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory)
            builder.UseLogging(loggerFactory);

        return builder.Build(services);
    }

    /// <summary>
    /// Builds Azure OpenAI clients (chat/embeddings/images) via the Azure OpenAI SDK.
    /// </summary>
    /// <param name="opts">The Azure OpenAI provider options.</param>
    /// <returns>The Azure OpenAI <see cref="ModelClients" /> bundle.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildAzureOpenAI(ModelOptions.AzureOpenAIOptions opts) {
        if (string.IsNullOrWhiteSpace(opts.ResourceName))
            throw new InvalidOperationException($"{nameof(ModelOptions.AzureOpenAI)}.{nameof(opts.ResourceName)} is required when DefaultProvider is AzureOpenAI.");
        if (string.IsNullOrWhiteSpace(opts.ChatDeploymentName))
            throw new InvalidOperationException($"{nameof(ModelOptions.AzureOpenAI)}.{nameof(opts.ChatDeploymentName)} is required when DefaultProvider is AzureOpenAI.");

        var endpoint = new Uri($"https://{opts.ResourceName}.openai.azure.com");
        var client = opts.UseAzureAD
            ? new AzureOpenAIClient(endpoint, new DefaultAzureCredential())
            : new AzureOpenAIClient(endpoint, new AzureKeyCredential(
                opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.AzureOpenAI)}.{nameof(opts.ApiKey)} is required when UseAzureAD is false.")));

        var chat = client.GetChatClient(opts.ChatDeploymentName).AsIChatClient();

        IEmbeddingGenerator<string, Embedding<float>>? embeddings = null;
        if (!string.IsNullOrWhiteSpace(opts.EmbeddingDeploymentName))
            embeddings = client.GetEmbeddingClient(opts.EmbeddingDeploymentName).AsIEmbeddingGenerator();

        IImageGenerator? images = null;
        if (!string.IsNullOrWhiteSpace(opts.ImageDeploymentName))
            images = client.GetImageClient(opts.ImageDeploymentName).AsIImageGenerator();

        return new ModelClients("AzureOpenAI", chat, embeddings, images);
    }

    /// <summary>
    /// Builds OpenAI clients (chat/embeddings/images) via the OpenAI SDK.
    /// </summary>
    /// <param name="opts">The OpenAI provider options.</param>
    /// <returns>The OpenAI <see cref="ModelClients" /> bundle.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildOpenAI(ModelOptions.OpenAIOptions opts) {
        var apiKey = opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.OpenAI)}.{nameof(opts.ApiKey)} is required when DefaultProvider is OpenAI.");
        var client = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey));

        var chat = client.GetChatClient(opts.ChatModel).AsIChatClient();
        var embeddings = client.GetEmbeddingClient(opts.EmbeddingModel).AsIEmbeddingGenerator();
        var images = client.GetImageClient(opts.ImageModel).AsIImageGenerator();

        return new ModelClients("OpenAI", chat, embeddings, images);
    }

    /// <summary>
    /// Builds Anthropic (Claude) clients via Anthropic's OpenAI-compatible endpoint, exposed through the
    /// OpenAI SDK. Anthropic has no first-party embeddings API; configure Meridian.Knowledge's embeddings
    /// to source from a different provider (e.g. OpenAI or Gemini) via <c>EmbeddingsOptions.Provider</c>.
    /// </summary>
    /// <param name="opts">The Anthropic provider options.</param>
    /// <returns>The Anthropic <see cref="ModelClients" /> bundle (embeddings always <see langword="null" />).</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildAnthropic(ModelOptions.AnthropicOptions opts) {
        var apiKey = opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.Anthropic)}.{nameof(opts.ApiKey)} is required when DefaultProvider is Anthropic.");
        var client = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = ToOpenAICompatEndpoint(opts.BaseUrl, appendOpenAISegment: false) });

        var chat = client.GetChatClient(opts.ChatModel).AsIChatClient();
        return new ModelClients("Anthropic", chat, Embeddings: null);
    }

    /// <summary>
    /// Builds Gemini clients (chat/embeddings) via Gemini's OpenAI-compatible endpoint, exposed through the OpenAI SDK.
    /// </summary>
    /// <param name="opts">The Gemini provider options.</param>
    /// <returns>The Gemini <see cref="ModelClients" /> bundle.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildGemini(ModelOptions.GeminiOptions opts) {
        var apiKey = opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.Gemini)}.{nameof(opts.ApiKey)} is required when DefaultProvider is Gemini.");
        var client = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = ToOpenAICompatEndpoint(opts.BaseUrl, appendOpenAISegment: true) });

        var chat = client.GetChatClient(opts.ChatModel).AsIChatClient();
        var embeddings = client.GetEmbeddingClient(opts.EmbeddingModel).AsIEmbeddingGenerator();

        return new ModelClients("Gemini", chat, embeddings);
    }

    /// <summary>
    /// Builds Perplexity (<c>sonar</c> family) clients via Perplexity's OpenAI-compatible endpoint. Perplexity
    /// has no embeddings API, so the embedding generator is always <see langword="null" />; pair it with another
    /// provider for Meridian.Knowledge embeddings when Perplexity is the default chat provider.
    /// </summary>
    /// <param name="opts">The Perplexity provider options.</param>
    /// <returns>The Perplexity <see cref="ModelClients" /> bundle (embeddings always <see langword="null" />).</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildPerplexity(ModelOptions.PerplexityOptions opts) {
        var apiKey = opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.Perplexity)}.{nameof(opts.ApiKey)} is required when DefaultProvider is Perplexity.");
        var client = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = ToOpenAICompatEndpoint(opts.BaseUrl, appendOpenAISegment: false) });

        var chat = client.GetChatClient(opts.ChatModel).AsIChatClient();
        return new ModelClients("Perplexity", chat, Embeddings: null);
    }

    /// <summary>
    /// Builds Azure AI Inference (Azure AI Foundry) clients (chat/embeddings) via the Azure AI Inference SDK.
    /// </summary>
    /// <param name="opts">The Azure AI Inference provider options.</param>
    /// <returns>The Azure AI Inference <see cref="ModelClients" /> bundle.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    private static ModelClients BuildAzureAIInference(ModelOptions.AzureAIInferenceOptions opts) {
        if (string.IsNullOrWhiteSpace(opts.Endpoint))
            throw new InvalidOperationException($"{nameof(ModelOptions.AzureAIInference)}.{nameof(opts.Endpoint)} is required when DefaultProvider is AzureAIInference.");
        if (string.IsNullOrWhiteSpace(opts.ChatModel))
            throw new InvalidOperationException($"{nameof(ModelOptions.AzureAIInference)}.{nameof(opts.ChatModel)} is required when DefaultProvider is AzureAIInference.");

        var endpoint = new Uri(opts.Endpoint);
        var credential = new AzureKeyCredential(
            opts.ApiKey ?? throw new InvalidOperationException($"{nameof(ModelOptions.AzureAIInference)}.{nameof(opts.ApiKey)} is required when DefaultProvider is AzureAIInference."));

        var chat = new ChatCompletionsClient(endpoint, credential).AsIChatClient(opts.ChatModel);

        IEmbeddingGenerator<string, Embedding<float>>? embeddings = null;
        if (!string.IsNullOrWhiteSpace(opts.EmbeddingModel))
            embeddings = new EmbeddingsClient(endpoint, credential).AsIEmbeddingGenerator(opts.EmbeddingModel);

        return new ModelClients("AzureAIInference", chat, embeddings);
    }

    /// <summary>
    /// Normalizes a provider base URL into an OpenAI-compatible endpoint (trailing slash required by the SDK).
    /// When <paramref name="appendOpenAISegment" /> is true, ensures the path ends in the provider's
    /// <c>/openai</c> compatibility segment (used by Gemini's <c>.../v1beta/openai</c> shim).
    /// </summary>
    /// <param name="baseUrl">The provider's configured base URL.</param>
    /// <param name="appendOpenAISegment">Whether to append the <c>/openai</c> compatibility segment.</param>
    /// <returns>A normalized, trailing-slash-terminated endpoint URI.</returns>
    private static Uri ToOpenAICompatEndpoint(string baseUrl, bool appendOpenAISegment) {
        var trimmed = (baseUrl ?? string.Empty).TrimEnd('/');
        if (appendOpenAISegment && !trimmed.EndsWith("/openai", StringComparison.OrdinalIgnoreCase))
            trimmed += "/openai";
        return new Uri(trimmed + "/");
    }
}
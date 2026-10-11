// ============================================================================
// Meridian
// File: ModelOptions.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Configuration surface for Meridian.Models: which provider is the default,
// which providers to fall back to in order, per-provider connection settings,
// and the opt-in Microsoft.Extensions.AI middleware pipeline (logging, caching,
// OpenTelemetry, retry) applied around the resulting chat client.
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

namespace Meridian.Models.Providers;

/// <summary>
/// Configures the model providers Meridian builds chat/embedding/image clients for.
/// </summary>
public sealed class ModelOptions {
    /// <summary>
    /// Gets or sets the provider used unless a call explicitly asks for another one.
    /// </summary>
    /// <value>The default provider. Defaults to <see cref="ModelProvider.AzureOpenAI" />.</value>
    public ModelProvider DefaultProvider { get; set; } = ModelProvider.AzureOpenAI;

    /// <summary>
    /// Gets the ordered list of providers to try, in order, if <see cref="DefaultProvider" /> fails.
    /// </summary>
    /// <value>The fallback providers, in try order. Empty by default, meaning no fallback.</value>
    public IList<ModelProvider> FallbackProviders { get; init; } = new List<ModelProvider>();

    /// <summary>
    /// Gets the opt-in Microsoft.Extensions.AI middleware pipeline options.
    /// </summary>
    /// <value>The pipeline configuration, with every stage disabled by default.</value>
    public PipelineOptions Pipeline { get; init; } = new();

    /// <summary>
    /// Gets the OpenAI provider options.
    /// </summary>
    /// <value>The OpenAI connection and model settings.</value>
    public OpenAIOptions OpenAI { get; init; } = new();

    /// <summary>
    /// Gets the Anthropic (Claude) provider options.
    /// </summary>
    /// <value>The Anthropic connection and model settings.</value>
    public AnthropicOptions Anthropic { get; init; } = new();

    /// <summary>
    /// Gets the Gemini provider options.
    /// </summary>
    /// <value>The Gemini connection and model settings.</value>
    public GeminiOptions Gemini { get; init; } = new();

    /// <summary>
    /// Gets the Perplexity provider options.
    /// </summary>
    /// <value>The Perplexity connection and model settings.</value>
    public PerplexityOptions Perplexity { get; init; } = new();

    /// <summary>
    /// Gets the Azure OpenAI provider options.
    /// </summary>
    /// <value>The Azure OpenAI connection and deployment settings.</value>
    public AzureOpenAIOptions AzureOpenAI { get; init; } = new();

    /// <summary>
    /// Gets the Azure AI Inference (Azure AI Foundry) provider options.
    /// </summary>
    /// <value>The Azure AI Inference connection and model settings.</value>
    public AzureAIInferenceOptions AzureAIInference { get; init; } = new();

    /// <summary>
    /// OpenAI connection and model settings.
    /// </summary>
    public sealed class OpenAIOptions {
        /// <summary>
        /// Gets or sets the OpenAI API key.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.OpenAIApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.OpenAIApiKey);

        /// <summary>
        /// Gets or sets the OpenAI API base URL.
        /// </summary>
        /// <value>The base URL. Defaults to <see cref="ModelDefaults.OpenAIBaseUrl" />.</value>
        public string BaseUrl { get; set; } = ModelDefaults.OpenAIBaseUrl;

        /// <summary>
        /// Gets or sets the chat model.
        /// </summary>
        /// <value>The chat model id. Defaults to <see cref="ModelDefaults.OpenAIDefaultChatModel" />.</value>
        public string ChatModel { get; set; } = ModelDefaults.OpenAIDefaultChatModel;

        /// <summary>
        /// Gets or sets the embedding model.
        /// </summary>
        /// <value>The embedding model id. Defaults to <see cref="ModelDefaults.OpenAIDefaultEmbeddingModel" />.</value>
        public string EmbeddingModel { get; set; } = ModelDefaults.OpenAIDefaultEmbeddingModel;

        /// <summary>
        /// Gets or sets the image model used for generation/editing (e.g. <c>gpt-image-1</c>, <c>dall-e-3</c>).
        /// </summary>
        /// <value>The image model id. Defaults to <see cref="ModelDefaults.OpenAIDefaultImageModel" />.</value>
        public string ImageModel { get; set; } = ModelDefaults.OpenAIDefaultImageModel;
    }

    /// <summary>
    /// Anthropic (Claude) connection and model settings. Anthropic speaks an OpenAI-compatible chat API
    /// and has no embeddings or image endpoint of its own; configure a separate provider for those when needed.
    /// </summary>
    public sealed class AnthropicOptions {
        /// <summary>
        /// Gets or sets the Anthropic API key.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.AnthropicApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.AnthropicApiKey);

        /// <summary>
        /// Gets or sets the Anthropic API base URL.
        /// </summary>
        /// <value>The base URL. Defaults to <see cref="ModelDefaults.AnthropicBaseUrl" />.</value>
        public string BaseUrl { get; set; } = ModelDefaults.AnthropicBaseUrl;

        /// <summary>
        /// Gets or sets the chat model.
        /// </summary>
        /// <value>The chat model id. Defaults to <see cref="ModelDefaults.AnthropicDefaultChatModel" />.</value>
        public string ChatModel { get; set; } = ModelDefaults.AnthropicDefaultChatModel;
    }

    /// <summary>
    /// Gemini connection and model settings, via Gemini's OpenAI-compatible endpoint.
    /// </summary>
    public sealed class GeminiOptions {
        /// <summary>
        /// Gets or sets the Gemini API key.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.GeminiApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.GeminiApiKey);

        /// <summary>
        /// Gets or sets the Gemini API base URL.
        /// </summary>
        /// <value>The base URL. Defaults to <see cref="ModelDefaults.GeminiBaseUrl" />.</value>
        public string BaseUrl { get; set; } = ModelDefaults.GeminiBaseUrl;

        /// <summary>
        /// Gets or sets the chat model.
        /// </summary>
        /// <value>The chat model id. Defaults to <see cref="ModelDefaults.GeminiDefaultChatModel" />.</value>
        public string ChatModel { get; set; } = ModelDefaults.GeminiDefaultChatModel;

        /// <summary>
        /// Gets or sets the embedding model.
        /// </summary>
        /// <value>The embedding model id. Defaults to <see cref="ModelDefaults.GeminiDefaultEmbeddingModel" />.</value>
        public string EmbeddingModel { get; set; } = ModelDefaults.GeminiDefaultEmbeddingModel;
    }

    /// <summary>
    /// Perplexity (<c>sonar</c> family) options. Perplexity speaks an OpenAI-compatible chat API and has
    /// no embeddings or image endpoint of its own; configure a separate provider for those when needed.
    /// </summary>
    public sealed class PerplexityOptions {
        /// <summary>
        /// Gets or sets the Perplexity API key.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.PerplexityApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.PerplexityApiKey);

        /// <summary>
        /// Gets or sets the Perplexity API base URL.
        /// </summary>
        /// <value>The base URL. Defaults to <see cref="ModelDefaults.PerplexityBaseUrl" />.</value>
        public string BaseUrl { get; set; } = ModelDefaults.PerplexityBaseUrl;

        /// <summary>
        /// Gets or sets the chat model.
        /// </summary>
        /// <value>The chat model id. Defaults to <see cref="ModelDefaults.PerplexityDefaultChatModel" />.</value>
        public string ChatModel { get; set; } = ModelDefaults.PerplexityDefaultChatModel;
    }

    /// <summary>
    /// Azure OpenAI connection and deployment settings.
    /// </summary>
    public sealed class AzureOpenAIOptions {
        /// <summary>
        /// Gets or sets the Azure OpenAI API key. Unused when <see cref="UseAzureAD" /> is <see langword="true" />.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.AzureOpenAIApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.AzureOpenAIApiKey);

        /// <summary>
        /// Gets or sets the Azure OpenAI resource name (the "xyz" in "https://xyz.openai.azure.com").
        /// </summary>
        /// <value>The resource name. Empty by default; required when this provider is used.</value>
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure OpenAI REST API version.
        /// </summary>
        /// <value>The API version. Defaults to <see cref="ModelDefaults.AzureOpenAIDefaultApiVersion" />.</value>
        public string ApiVersion { get; set; } = ModelDefaults.AzureOpenAIDefaultApiVersion;

        /// <summary>
        /// Gets or sets the chat deployment name.
        /// </summary>
        /// <value>The deployment name. Empty by default; required when this provider is used.</value>
        public string ChatDeploymentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the embedding deployment name.
        /// </summary>
        /// <value>The deployment name, or empty to leave embeddings unconfigured for this provider.</value>
        public string EmbeddingDeploymentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image generation deployment name.
        /// </summary>
        /// <value>The deployment name, or empty to disable image generation for this provider.</value>
        public string ImageDeploymentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether to authenticate via Azure AD (<see cref="Azure.Identity.DefaultAzureCredential" />) instead of an API key.
        /// </summary>
        /// <value><see langword="true" /> to use Azure AD; <see langword="false" /> (the default) to use <see cref="ApiKey" />.</value>
        public bool UseAzureAD { get; set; }
    }

    /// <summary>
    /// Azure AI Inference (Azure AI Foundry) connection and model settings.
    /// </summary>
    public sealed class AzureAIInferenceOptions {
        /// <summary>
        /// Gets or sets the Azure AI Inference API key.
        /// </summary>
        /// <value>The API key, read from <see cref="ModelDefaults.EnvironmentVariables.AzureAIInferenceApiKey" /> by default.</value>
        public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable(ModelDefaults.EnvironmentVariables.AzureAIInferenceApiKey);

        /// <summary>
        /// Gets or sets the inference endpoint, e.g. <c>https://your-resource.services.ai.azure.com/models</c>.
        /// </summary>
        /// <value>The endpoint URL. Empty by default; required when this provider is used.</value>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the chat model/deployment name.
        /// </summary>
        /// <value>The model id. Empty by default; required when this provider is used.</value>
        public string ChatModel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the embedding model/deployment name.
        /// </summary>
        /// <value>The model id, or empty to leave embeddings unconfigured for this provider.</value>
        public string EmbeddingModel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Microsoft.Extensions.AI chat pipeline options. Each stage is opt-in; a disabled stage adds no wrapper.
    /// </summary>
    public sealed class PipelineOptions {
        /// <summary>
        /// Gets or sets whether the logging middleware stage is added.
        /// </summary>
        /// <value><see langword="true" /> to log requests/responses via the resolved <c>ILoggerFactory</c>; otherwise <see langword="false" /> (the default).</value>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets whether the distributed-cache middleware stage is added.
        /// </summary>
        /// <value><see langword="true" /> to cache responses via the resolved <c>IDistributedCache</c>; otherwise <see langword="false" /> (the default).</value>
        public bool EnableDistributedCache { get; set; }

        /// <summary>
        /// Gets or sets whether the OpenTelemetry middleware stage is added.
        /// </summary>
        /// <value><see langword="true" /> to emit OpenTelemetry spans/metrics for chat calls; otherwise <see langword="false" /> (the default).</value>
        public bool EnableOpenTelemetry { get; set; }

        /// <summary>
        /// Gets or sets the OpenTelemetry source name used when <see cref="EnableOpenTelemetry" /> is set.
        /// </summary>
        /// <value>The source name. Defaults to <c>"Meridian"</c>.</value>
        public string OpenTelemetrySourceName { get; set; } = "Meridian";

        /// <summary>
        /// Gets or sets whether the retry middleware stage is added.
        /// </summary>
        /// <value><see langword="true" /> to retry transient failures with exponential backoff; otherwise <see langword="false" /> (the default).</value>
        public bool EnableRetry { get; set; }

        /// <summary>
        /// Gets or sets the maximum retry attempts when <see cref="EnableRetry" /> is set.
        /// </summary>
        /// <value>The attempt count. Defaults to <see cref="ModelDefaults.MaxRetryAttempts" />.</value>
        public int MaxRetries { get; set; } = ModelDefaults.MaxRetryAttempts;

        /// <summary>
        /// Gets or sets the base delay for retry exponential backoff when <see cref="EnableRetry" /> is set.
        /// </summary>
        /// <value>The base delay. Defaults to <see cref="ModelDefaults.RetryBaseDelayMilliseconds" /> milliseconds.</value>
        public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(ModelDefaults.RetryBaseDelayMilliseconds);
    }

    /// <summary>
    /// Gets or sets the default per-request timeout.
    /// </summary>
    /// <value>The timeout. Defaults to <see cref="ModelDefaults.DefaultTimeoutSeconds" /> seconds.</value>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(ModelDefaults.DefaultTimeoutSeconds);
}

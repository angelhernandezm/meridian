// ============================================================================
// Meridian
// File: ModelDefaults.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Default values (environment variable names, API versions, model ids, timeouts)
// shared by the provider options and client builder, kept out of line so none of
// it is a magic literal at the call site.
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
/// Default configuration values for the built-in model providers.
/// </summary>
public static class ModelDefaults {
    /// <summary>
    /// Names of the environment variables each provider's API key falls back to when not set in code.
    /// </summary>
    public static class EnvironmentVariables {
        /// <summary>
        /// Environment variable holding the OpenAI API key.
        /// </summary>
        public const string OpenAIApiKey = "MERIDIAN_OPENAI_API_KEY";

        /// <summary>
        /// Environment variable holding the Anthropic API key.
        /// </summary>
        public const string AnthropicApiKey = "MERIDIAN_ANTHROPIC_API_KEY";

        /// <summary>
        /// Environment variable holding the Gemini API key.
        /// </summary>
        public const string GeminiApiKey = "MERIDIAN_GEMINI_API_KEY";

        /// <summary>
        /// Environment variable holding the Azure OpenAI API key.
        /// </summary>
        public const string AzureOpenAIApiKey = "MERIDIAN_AZURE_OPENAI_API_KEY";

        /// <summary>
        /// Environment variable holding the Azure AI Inference API key.
        /// </summary>
        public const string AzureAIInferenceApiKey = "MERIDIAN_AZURE_AI_INFERENCE_API_KEY";

        /// <summary>
        /// Environment variable holding the Perplexity API key.
        /// </summary>
        public const string PerplexityApiKey = "MERIDIAN_PERPLEXITY_API_KEY";
    }

    /// <summary>
    /// The default OpenAI API base URL.
    /// </summary>
    public const string OpenAIBaseUrl = "https://api.openai.com/v1";

    /// <summary>
    /// The default OpenAI chat model.
    /// </summary>
    public const string OpenAIDefaultChatModel = "gpt-4o-mini";

    /// <summary>
    /// The default OpenAI embedding model.
    /// </summary>
    public const string OpenAIDefaultEmbeddingModel = "text-embedding-3-small";

    /// <summary>
    /// The default OpenAI image generation model.
    /// </summary>
    public const string OpenAIDefaultImageModel = "gpt-image-1";

    /// <summary>
    /// The default Anthropic API base URL.
    /// </summary>
    public const string AnthropicBaseUrl = "https://api.anthropic.com/v1";

    /// <summary>
    /// The default Anthropic chat model.
    /// </summary>
    public const string AnthropicDefaultChatModel = "claude-sonnet-4-5";

    /// <summary>
    /// The default Gemini (OpenAI-compatible) API base URL.
    /// </summary>
    public const string GeminiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai";

    /// <summary>
    /// The default Gemini chat model.
    /// </summary>
    public const string GeminiDefaultChatModel = "gemini-2.5-flash";

    /// <summary>
    /// The default Gemini embedding model.
    /// </summary>
    public const string GeminiDefaultEmbeddingModel = "text-embedding-004";

    /// <summary>
    /// The default Azure OpenAI REST API version.
    /// </summary>
    public const string AzureOpenAIDefaultApiVersion = "2024-10-21";

    /// <summary>
    /// The default Perplexity (OpenAI-compatible) API base URL.
    /// </summary>
    public const string PerplexityBaseUrl = "https://api.perplexity.ai";

    /// <summary>
    /// The default Perplexity chat model.
    /// </summary>
    public const string PerplexityDefaultChatModel = "sonar-pro";

    /// <summary>
    /// The default per-request timeout, in seconds.
    /// </summary>
    public const int DefaultTimeoutSeconds = 100;

    /// <summary>
    /// The default maximum number of retry attempts when the retry pipeline stage is enabled.
    /// </summary>
    public const int MaxRetryAttempts = 3;

    /// <summary>
    /// The default base delay, in milliseconds, for retry exponential backoff.
    /// </summary>
    public const int RetryBaseDelayMilliseconds = 500;
}
// ============================================================================
// Meridian
// File: ComInteropConfigurator.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Configures ModelOptions for the COM host entirely from environment variables,
// since a VB6/VFP/Delphi/VBA caller has no way to run the C# fluent builder
// itself. API keys already fall back to Meridian.Models' own MERIDIAN_* env vars
// (see ModelOptions); this adds the non-secret selection knobs (which provider,
// which deployment/model) a COM host still needs to set before anything works.
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

namespace Meridian.Interop.Com.Configuration;

/// <summary>
/// Applies environment-variable configuration to <see cref="ModelOptions" /> for the COM host. See
/// <c>COM-INTEROP.md</c> for the full variable list and how to set them (e.g. via <c>setx</c>).
/// </summary>
internal static class ComInteropConfigurator {
    /// <summary>
    /// Applies the <c>MERIDIAN_*</c> environment variables to <paramref name="options" />. Variables that
    /// aren't set are left at their existing (default) value.
    /// </summary>
    /// <param name="options">The options to apply environment-variable configuration to.</param>
    public static void Apply(ModelOptions options) {
        if (Enum.TryParse<ModelProvider>(Environment.GetEnvironmentVariable("MERIDIAN_DEFAULT_PROVIDER"), ignoreCase: true, out var provider))
            options.DefaultProvider = provider;

        ApplyIfSet("MERIDIAN_AZURE_OPENAI_RESOURCE_NAME", v => options.AzureOpenAI.ResourceName = v);
        ApplyIfSet("MERIDIAN_AZURE_OPENAI_CHAT_DEPLOYMENT", v => options.AzureOpenAI.ChatDeploymentName = v);
        ApplyIfSet("MERIDIAN_AZURE_OPENAI_EMBEDDING_DEPLOYMENT", v => options.AzureOpenAI.EmbeddingDeploymentName = v);
        ApplyIfSet("MERIDIAN_AZURE_OPENAI_IMAGE_DEPLOYMENT", v => options.AzureOpenAI.ImageDeploymentName = v);
        ApplyIfSet("MERIDIAN_AZURE_OPENAI_USE_AAD", v => options.AzureOpenAI.UseAzureAD = bool.TryParse(v, out var b) && b);

        ApplyIfSet("MERIDIAN_OPENAI_CHAT_MODEL", v => options.OpenAI.ChatModel = v);
        ApplyIfSet("MERIDIAN_OPENAI_EMBEDDING_MODEL", v => options.OpenAI.EmbeddingModel = v);
        ApplyIfSet("MERIDIAN_OPENAI_IMAGE_MODEL", v => options.OpenAI.ImageModel = v);

        ApplyIfSet("MERIDIAN_ANTHROPIC_CHAT_MODEL", v => options.Anthropic.ChatModel = v);

        ApplyIfSet("MERIDIAN_GEMINI_CHAT_MODEL", v => options.Gemini.ChatModel = v);
        ApplyIfSet("MERIDIAN_GEMINI_EMBEDDING_MODEL", v => options.Gemini.EmbeddingModel = v);

        ApplyIfSet("MERIDIAN_PERPLEXITY_CHAT_MODEL", v => options.Perplexity.ChatModel = v);

        ApplyIfSet("MERIDIAN_AZURE_AI_INFERENCE_ENDPOINT", v => options.AzureAIInference.Endpoint = v);
        ApplyIfSet("MERIDIAN_AZURE_AI_INFERENCE_CHAT_MODEL", v => options.AzureAIInference.ChatModel = v);
        ApplyIfSet("MERIDIAN_AZURE_AI_INFERENCE_EMBEDDING_MODEL", v => options.AzureAIInference.EmbeddingModel = v);
    }

    /// <summary>
    /// Invokes <paramref name="apply" /> with the value of <paramref name="variable" /> when it's set to a non-blank value.
    /// </summary>
    /// <param name="variable">The environment variable name to read.</param>
    /// <param name="apply">The callback to invoke with the variable's value, if set.</param>
    private static void ApplyIfSet(string variable, Action<string> apply) {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrWhiteSpace(value))
            apply(value);
    }
}
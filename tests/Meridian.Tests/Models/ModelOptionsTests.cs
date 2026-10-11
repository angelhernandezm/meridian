// ============================================================================
// Meridian
// File: ModelOptionsTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for ModelOptions: default values for each provider's non-secret
// settings, so a regression that silently changes a default (e.g. a model id)
// is caught at build time instead of in production.
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
using Xunit;

namespace Meridian.Tests.Models;

/// <summary>
/// Unit tests for <see cref="ModelOptions" /> defaults.
/// </summary>
public sealed class ModelOptionsTests {
    /// <summary>
    /// The default provider is Azure OpenAI, with no fallback providers configured.
    /// </summary>
    [Fact]
    public void Defaults_UseAzureOpenAIWithNoFallback() {
        var options = new ModelOptions();

        Assert.Equal(ModelProvider.AzureOpenAI, options.DefaultProvider);
        Assert.Empty(options.FallbackProviders);
    }

    /// <summary>
    /// Every pipeline stage is opt-in (disabled) by default.
    /// </summary>
    [Fact]
    public void Defaults_PipelineStagesAreDisabled() {
        var pipeline = new ModelOptions().Pipeline;

        Assert.False(pipeline.EnableLogging);
        Assert.False(pipeline.EnableDistributedCache);
        Assert.False(pipeline.EnableOpenTelemetry);
        Assert.False(pipeline.EnableRetry);
    }

    /// <summary>
    /// OpenAI's default chat/embedding/image models match the published Meridian defaults.
    /// </summary>
    [Fact]
    public void Defaults_OpenAIModels() {
        var openAI = new ModelOptions().OpenAI;

        Assert.Equal(ModelDefaults.OpenAIDefaultChatModel, openAI.ChatModel);
        Assert.Equal(ModelDefaults.OpenAIDefaultEmbeddingModel, openAI.EmbeddingModel);
        Assert.Equal(ModelDefaults.OpenAIDefaultImageModel, openAI.ImageModel);
    }

    /// <summary>
    /// Azure OpenAI defaults to API-key auth (not Azure AD) with empty deployment names.
    /// </summary>
    [Fact]
    public void Defaults_AzureOpenAI() {
        var azure = new ModelOptions().AzureOpenAI;

        Assert.False(azure.UseAzureAD);
        Assert.Equal(string.Empty, azure.ResourceName);
        Assert.Equal(string.Empty, azure.ChatDeploymentName);
        Assert.Equal(ModelDefaults.AzureOpenAIDefaultApiVersion, azure.ApiVersion);
    }

    /// <summary>
    /// Perplexity has no embeddings field at all — its options expose only chat settings.
    /// </summary>
    [Fact]
    public void Defaults_Perplexity() {
        var perplexity = new ModelOptions().Perplexity;
        Assert.Equal(ModelDefaults.PerplexityDefaultChatModel, perplexity.ChatModel);
    }
}
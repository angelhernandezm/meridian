// ============================================================================
// Meridian
// File: MainWindow.Setup.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Setup tab: configures ModelOptions (provider credentials/models, fallback
// order), opts into Meridian.Knowledge (embeddings, semantic store, retrieval,
// RAG) and Meridian.Agents (memory, images, a single "assistant" agent), then
// builds the IServiceProvider every other tab resolves services from. Mirrors
// services.AddMeridian(...) from the README, driven by the UI instead of code.
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

using Avalonia.Controls;
using Avalonia.Layout;
using Meridian.Agents.Agents;
using Meridian.Hosting;
using Meridian.Models.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

partial class MainWindow {
    // ---- Provider selection -------------------------------------------------
    private readonly ComboBox _defaultProviderCombo = new() {
        ItemsSource = new[] { ModelProvider.OpenAI, ModelProvider.AzureOpenAI, ModelProvider.Anthropic, ModelProvider.Gemini, ModelProvider.Perplexity, ModelProvider.AzureAIInference },
        SelectedIndex = 0
    };
    private readonly Dictionary<ModelProvider, CheckBox> _fallbackChecks = new();

    // ---- Per-provider credential fields --------------------------------------
    private readonly TextBox _openAiKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.OpenAIApiKey}" };
    private readonly TextBox _openAiChatModel = new() { Watermark = ModelDefaults.OpenAIDefaultChatModel };
    private readonly TextBox _openAiEmbeddingModel = new() { Watermark = ModelDefaults.OpenAIDefaultEmbeddingModel };
    private readonly TextBox _openAiImageModel = new() { Watermark = ModelDefaults.OpenAIDefaultImageModel };

    private readonly TextBox _anthropicKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.AnthropicApiKey}" };
    private readonly TextBox _anthropicChatModel = new() { Watermark = ModelDefaults.AnthropicDefaultChatModel };

    private readonly TextBox _geminiKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.GeminiApiKey}" };
    private readonly TextBox _geminiChatModel = new() { Watermark = ModelDefaults.GeminiDefaultChatModel };
    private readonly TextBox _geminiEmbeddingModel = new() { Watermark = ModelDefaults.GeminiDefaultEmbeddingModel };

    private readonly TextBox _perplexityKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.PerplexityApiKey}" };
    private readonly TextBox _perplexityChatModel = new() { Watermark = ModelDefaults.PerplexityDefaultChatModel };

    private readonly TextBox _azureOpenAiKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.AzureOpenAIApiKey}" };
    private readonly TextBox _azureOpenAiResource = new() { Watermark = "my-resource" };
    private readonly TextBox _azureOpenAiChatDeployment = new() { Watermark = "gpt-4o" };
    private readonly TextBox _azureOpenAiEmbeddingDeployment = new() { Watermark = "text-embedding-3-small" };
    private readonly TextBox _azureOpenAiImageDeployment = new() { Watermark = "gpt-image-1 (optional)" };
    private readonly CheckBox _azureOpenAiUseAad = new() { Content = "Use Azure AD instead of an API key" };

    private readonly TextBox _azureAiInferenceKey = new() { PasswordChar = '●', Watermark = $"blank = use {ModelDefaults.EnvironmentVariables.AzureAIInferenceApiKey}" };
    private readonly TextBox _azureAiInferenceEndpoint = new() { Watermark = "https://your-resource.services.ai.azure.com/models" };
    private readonly TextBox _azureAiInferenceChatModel = new();
    private readonly TextBox _azureAiInferenceEmbeddingModel = new();

    // ---- Pipeline -------------------------------------------------------------
    private readonly CheckBox _enableRetry = new() { Content = "Retry transient failures", IsChecked = true };
    private readonly CheckBox _enableOpenTelemetry = new() { Content = "OpenTelemetry tracing" };

    // ---- Knowledge --------------------------------------------------------------
    private readonly CheckBox _enableEmbeddings = new() { Content = "Embeddings (required by Retrieval, Memory, and the Embeddings tab)", IsChecked = true };
    private readonly RadioButton _retrievalNone = new() { Content = "None", GroupName = "retrieval" };
    private readonly RadioButton _retrievalSemantic = new() { Content = "Semantic only", GroupName = "retrieval" };
    private readonly RadioButton _retrievalHybrid = new() { Content = "Hybrid (semantic + keyword)", GroupName = "retrieval", IsChecked = true };
    private readonly CheckBox _enableRagBuilder = new() { Content = "RAG context builder (requires retrieval)", IsChecked = true };
    private readonly CheckBox _enableMemory = new() { Content = "Long-term memory (requires embeddings)", IsChecked = true };
    private readonly CheckBox _enableImages = new() { Content = "Images: generation, editing, vision analysis (OpenAI/Azure OpenAI only)", IsChecked = true };

    // ---- Assistant agent --------------------------------------------------------
    private readonly TextBox _agentInstructions = new() {
        AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 60,
        Text = "You are a concise, helpful assistant."
    };
    private readonly CheckBox _agentSearchTool = new() { Content = "Attach knowledge-search tool (requires retrieval)" };
    private readonly CheckBox _agentSampleTool = new() { Content = "Register sample \"getWeather\" tool and give it to the agent" };

    private readonly TextBox _setupStatus = Output(70);

    /// <summary>
    /// Builds the Setup tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildSetupTab() {
        foreach (var provider in new[] { ModelProvider.OpenAI, ModelProvider.AzureOpenAI, ModelProvider.Anthropic, ModelProvider.Gemini, ModelProvider.Perplexity, ModelProvider.AzureAIInference })
            _fallbackChecks[provider] = new CheckBox { Content = provider.ToString() };

        var root = Stack(14);
        root.Children.Add(Heading("1. Provider"));
        root.Children.Add(LabeledRow("Default provider", _defaultProviderCombo));
        root.Children.Add(Help("Fallback providers, tried in order if the default fails:"));

        var fallbackRow = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var check in _fallbackChecks.Values)
            fallbackRow.Children.Add(check);
        root.Children.Add(fallbackRow);

        root.Children.Add(new Expander { Header = "OpenAI", Content = OpenAiPanel() });
        root.Children.Add(new Expander { Header = "Anthropic (Claude)", Content = AnthropicPanel() });
        root.Children.Add(new Expander { Header = "Gemini", Content = GeminiPanel() });
        root.Children.Add(new Expander { Header = "Perplexity", Content = PerplexityPanel() });
        root.Children.Add(new Expander { Header = "Azure OpenAI", Content = AzureOpenAiPanel() });
        root.Children.Add(new Expander { Header = "Azure AI Inference (Foundry)", Content = AzureAiInferencePanel() });

        root.Children.Add(new Separator());
        root.Children.Add(Heading("2. Pipeline"));
        root.Children.Add(_enableRetry);
        root.Children.Add(_enableOpenTelemetry);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("3. Knowledge (embeddings, retrieval, RAG, memory, images)"));
        root.Children.Add(_enableEmbeddings);
        root.Children.Add(Help("Retrieval mode (feeds the Knowledge/RAG tab and the agent's search tool):"));
        var retrievalRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
        retrievalRow.Children.Add(_retrievalNone);
        retrievalRow.Children.Add(_retrievalSemantic);
        retrievalRow.Children.Add(_retrievalHybrid);
        root.Children.Add(retrievalRow);
        root.Children.Add(_enableRagBuilder);
        root.Children.Add(_enableMemory);
        root.Children.Add(_enableImages);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("4. Assistant agent (used by the Agent tab)"));
        root.Children.Add(Help("Instructions:"));
        root.Children.Add(_agentInstructions);
        root.Children.Add(_agentSearchTool);
        root.Children.Add(_agentSampleTool);

        root.Children.Add(new Separator());
        var initButton = new Button { Content = "Initialize Meridian", FontWeight = Avalonia.Media.FontWeight.Bold };
        initButton.Click += (_, _) => RunSafely(initButton, _setupStatus, "Initializing...", InitializeAsync);
        root.Children.Add(initButton);
        root.Children.Add(_setupStatus);

        return new TabItem { Header = "Setup", Content = Page(root) };
    }

    /// <summary>
    /// Builds a labeled field row: a fixed-width label followed by <paramref name="input" />.
    /// </summary>
    /// <param name="label">The field's label text.</param>
    /// <param name="input">The input control.</param>
    /// <returns>The assembled row.</returns>
    private static StackPanel LabeledRow(string label, Control input) {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(new TextBlock { Text = label, Width = 170, VerticalAlignment = VerticalAlignment.Center });
        input.Width = 320;
        row.Children.Add(input);
        return row;
    }

    private Control OpenAiPanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _openAiKey));
        p.Children.Add(LabeledRow("Chat model", _openAiChatModel));
        p.Children.Add(LabeledRow("Embedding model", _openAiEmbeddingModel));
        p.Children.Add(LabeledRow("Image model", _openAiImageModel));
        return p;
    }

    private Control AnthropicPanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _anthropicKey));
        p.Children.Add(LabeledRow("Chat model", _anthropicChatModel));
        p.Children.Add(Help("No embeddings or image endpoint — pair with OpenAI or Gemini for those."));
        return p;
    }

    private Control GeminiPanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _geminiKey));
        p.Children.Add(LabeledRow("Chat model", _geminiChatModel));
        p.Children.Add(LabeledRow("Embedding model", _geminiEmbeddingModel));
        p.Children.Add(Help("No image generation/editing endpoint."));
        return p;
    }

    private Control PerplexityPanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _perplexityKey));
        p.Children.Add(LabeledRow("Chat model", _perplexityChatModel));
        p.Children.Add(Help("No embeddings or image endpoint."));
        return p;
    }

    private Control AzureOpenAiPanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _azureOpenAiKey));
        p.Children.Add(_azureOpenAiUseAad);
        p.Children.Add(LabeledRow("Resource name", _azureOpenAiResource));
        p.Children.Add(LabeledRow("Chat deployment", _azureOpenAiChatDeployment));
        p.Children.Add(LabeledRow("Embedding deployment", _azureOpenAiEmbeddingDeployment));
        p.Children.Add(LabeledRow("Image deployment", _azureOpenAiImageDeployment));
        return p;
    }

    private Control AzureAiInferencePanel() {
        var p = Stack(6);
        p.Children.Add(LabeledRow("API key", _azureAiInferenceKey));
        p.Children.Add(LabeledRow("Endpoint", _azureAiInferenceEndpoint));
        p.Children.Add(LabeledRow("Chat model", _azureAiInferenceChatModel));
        p.Children.Add(LabeledRow("Embedding model", _azureAiInferenceEmbeddingModel));
        return p;
    }

    /// <summary>
    /// Builds the Meridian <see cref="IServiceProvider" /> from every Setup tab control and stores it in
    /// <see cref="MainWindow._services" />, resetting any previous agent session.
    /// </summary>
    private async Task InitializeAsync() {
        var services = new ServiceCollection();
        var defaultProvider = (ModelProvider)_defaultProviderCombo.SelectedItem!;

        services.AddMeridian(options => {
            options.AddModel(model => {
                model.DefaultProvider = defaultProvider;
                foreach (var (provider, check) in _fallbackChecks)
                    if (check.IsChecked == true && provider != defaultProvider)
                        model.FallbackProviders.Add(provider);

                ApplyIfSet(_openAiKey.Text, v => model.OpenAI.ApiKey = v);
                ApplyIfSet(_openAiChatModel.Text, v => model.OpenAI.ChatModel = v);
                ApplyIfSet(_openAiEmbeddingModel.Text, v => model.OpenAI.EmbeddingModel = v);
                ApplyIfSet(_openAiImageModel.Text, v => model.OpenAI.ImageModel = v);

                ApplyIfSet(_anthropicKey.Text, v => model.Anthropic.ApiKey = v);
                ApplyIfSet(_anthropicChatModel.Text, v => model.Anthropic.ChatModel = v);

                ApplyIfSet(_geminiKey.Text, v => model.Gemini.ApiKey = v);
                ApplyIfSet(_geminiChatModel.Text, v => model.Gemini.ChatModel = v);
                ApplyIfSet(_geminiEmbeddingModel.Text, v => model.Gemini.EmbeddingModel = v);

                ApplyIfSet(_perplexityKey.Text, v => model.Perplexity.ApiKey = v);
                ApplyIfSet(_perplexityChatModel.Text, v => model.Perplexity.ChatModel = v);

                ApplyIfSet(_azureOpenAiKey.Text, v => model.AzureOpenAI.ApiKey = v);
                ApplyIfSet(_azureOpenAiResource.Text, v => model.AzureOpenAI.ResourceName = v);
                ApplyIfSet(_azureOpenAiChatDeployment.Text, v => model.AzureOpenAI.ChatDeploymentName = v);
                ApplyIfSet(_azureOpenAiEmbeddingDeployment.Text, v => model.AzureOpenAI.EmbeddingDeploymentName = v);
                ApplyIfSet(_azureOpenAiImageDeployment.Text, v => model.AzureOpenAI.ImageDeploymentName = v);
                model.AzureOpenAI.UseAzureAD = _azureOpenAiUseAad.IsChecked == true;

                ApplyIfSet(_azureAiInferenceKey.Text, v => model.AzureAIInference.ApiKey = v);
                ApplyIfSet(_azureAiInferenceEndpoint.Text, v => model.AzureAIInference.Endpoint = v);
                ApplyIfSet(_azureAiInferenceChatModel.Text, v => model.AzureAIInference.ChatModel = v);
                ApplyIfSet(_azureAiInferenceEmbeddingModel.Text, v => model.AzureAIInference.EmbeddingModel = v);

                model.Pipeline.EnableRetry = _enableRetry.IsChecked == true;
                model.Pipeline.EnableOpenTelemetry = _enableOpenTelemetry.IsChecked == true;
            });

            if (_enableEmbeddings.IsChecked == true)
                options.AddEmbeddings();

            if (_retrievalSemantic.IsChecked == true || _retrievalHybrid.IsChecked == true) {
                options.AddSemanticStore();
                if (_retrievalHybrid.IsChecked == true)
                    options.AddHybridRetrieval();
                else
                    options.AddRetrieval();

                if (_enableRagBuilder.IsChecked == true)
                    options.AddRagContextBuilder();
            }

            if (_enableMemory.IsChecked == true) {
                if (_retrievalNone.IsChecked == true)
                    options.AddSemanticStore(); // Memory needs its own store when retrieval didn't already add one.
                options.AddMemory();
            }

            if (_enableImages.IsChecked == true)
                options.AddImages();

            if (_agentSampleTool.IsChecked == true)
                options.AddTool("getWeather", (string city) => $"It's sunny in {city}.", "Looks up the current weather for a city.");

            options.AddAgent("assistant", agent => {
                agent.Instructions = string.IsNullOrWhiteSpace(_agentInstructions.Text) ? null : _agentInstructions.Text;
                if (_agentSampleTool.IsChecked == true)
                    agent.ToolNames.Add("getWeather");
                agent.EnableSearchTool = _agentSearchTool.IsChecked == true;
                agent.EnableTracing = _enableOpenTelemetry.IsChecked == true;
            });
        });

        var provider = services.BuildServiceProvider();

        // Resolve the agent now (instead of lazily on the Agent tab) so a configuration mistake -- e.g.
        // EnableSearchTool without retrieval enabled -- surfaces here, on the tab where it's fixable,
        // rather than leaving _services set with no usable agent behind it.
        var registry = provider.GetRequiredService<IMeridianAgentRegistry>();
        var agent = registry.Get("assistant");

        _services = provider;
        _agent = agent;
        _agentSession = null;

        await Task.CompletedTask;
        _setupStatus.Text = $"Initialized. Default provider: {defaultProvider}. Switch to any other tab to try it out.";
    }

    /// <summary>
    /// Invokes <paramref name="apply" /> with <paramref name="value" /> only when it's non-blank, leaving
    /// the target property at its <see cref="ModelOptions" /> default (which already falls back to the
    /// matching <c>MERIDIAN_*_API_KEY</c> environment variable for API keys) otherwise.
    /// </summary>
    /// <param name="value">The UI field's current text.</param>
    /// <param name="apply">The callback to invoke with <paramref name="value" /> when it's set.</param>
    private static void ApplyIfSet(string? value, Action<string> apply) {
        if (!string.IsNullOrWhiteSpace(value))
            apply(value);
    }
}

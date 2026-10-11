// ============================================================================
// Meridian
// File: MainWindow.Knowledge.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Knowledge/RAG tab: ingest text chunks into the semantic store, retrieve
// the passages most relevant to a query, and assemble a ready-to-send RAG
// prompt with cited sources -- exercising Meridian.Knowledge end to end
// (ISemanticStore, IRetriever, RagContextBuilder). Not exposed over COM.
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
using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.Retrieval;
using Meridian.Knowledge.SemanticStore;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

partial class MainWindow {
    private readonly TextBox _ingestId = new() { Watermark = "Id (optional -- auto-generated if blank)" };
    private readonly TextBox _ingestText = new() { AcceptsReturn = true, Height = 70, Watermark = "Text to add to the knowledge base" };
    private readonly TextBox _knowledgeOutput = Output(220);

    private readonly TextBox _retrieveQuery = new() { Watermark = "Search query" };
    private readonly NumericUpDown _retrieveTopK = new() { Value = 5, Minimum = 1, Maximum = 50, Width = 90 };

    private readonly TextBox _ragQuery = new() { Watermark = "Question to answer with RAG" };
    private readonly NumericUpDown _ragTopK = new() { Value = 5, Minimum = 1, Maximum = 50, Width = 90 };

    /// <summary>
    /// Builds the Knowledge/RAG tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildKnowledgeTab() {
        var root = Stack();
        root.Children.Add(Heading("Knowledge base: ingest"));
        root.Children.Add(Help("Requires \"Embeddings\" and a retrieval mode (Semantic or Hybrid) enabled on the Setup tab."));
        root.Children.Add(_ingestId);
        root.Children.Add(_ingestText);
        var ingestButton = new Button { Content = "Add to knowledge base" };
        ingestButton.Click += (_, _) => RunSafely(ingestButton, _knowledgeOutput, "Embedding and storing...", IngestAsync);
        root.Children.Add(ingestButton);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("Retrieve"));
        var retrieveRow = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        retrieveRow.Children.Add(_retrieveQuery);
        retrieveRow.Children.Add(new TextBlock { Text = "Top K:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        retrieveRow.Children.Add(_retrieveTopK);
        root.Children.Add(retrieveRow);
        var retrieveButton = new Button { Content = "Search" };
        retrieveButton.Click += (_, _) => RunSafely(retrieveButton, _knowledgeOutput, "Searching...", RetrieveAsync);
        root.Children.Add(retrieveButton);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("Batteries-included RAG prompt"));
        root.Children.Add(Help("Requires \"RAG context builder\" enabled on the Setup tab."));
        var ragRow = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8 };
        ragRow.Children.Add(_ragQuery);
        ragRow.Children.Add(new TextBlock { Text = "Top K:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        ragRow.Children.Add(_ragTopK);
        root.Children.Add(ragRow);
        var ragButton = new Button { Content = "Build RAG prompt" };
        ragButton.Click += (_, _) => RunSafely(ragButton, _knowledgeOutput, "Retrieving and assembling...", BuildRagPromptAsync);
        root.Children.Add(ragButton);

        root.Children.Add(_knowledgeOutput);
        return new TabItem { Header = "Knowledge / RAG", Content = Page(root) };
    }

    private async Task IngestAsync() {
        if (!RequireServices(_knowledgeOutput))
            return;

        if (string.IsNullOrWhiteSpace(_ingestText.Text)) {
            _knowledgeOutput.Text = "Enter some text to add first.";
            return;
        }

        var embeddings = _services!.GetRequiredService<IEmbeddingService>();
        var store = _services!.GetRequiredService<ISemanticStore>();

        var id = string.IsNullOrWhiteSpace(_ingestId.Text) ? Guid.NewGuid().ToString("n") : _ingestId.Text;
        var vector = await embeddings.EmbedAsync(_ingestText.Text).ConfigureAwait(true);
        await store.UpsertAsync(new SemanticRecord(id, vector, _ingestText.Text)).ConfigureAwait(true);

        _knowledgeOutput.Text = $"Added record \"{id}\" ({vector.Length}-dim vector).";
        _ingestId.Text = string.Empty;
        _ingestText.Text = string.Empty;
    }

    private async Task RetrieveAsync() {
        if (!RequireServices(_knowledgeOutput))
            return;

        if (string.IsNullOrWhiteSpace(_retrieveQuery.Text)) {
            _knowledgeOutput.Text = "Enter a search query first.";
            return;
        }

        var retriever = _services!.GetRequiredService<IRetriever>();
        var topK = (int)(_retrieveTopK.Value ?? 5);
        var results = await retriever.RetrieveAsync(_retrieveQuery.Text, topK).ConfigureAwait(true);

        _knowledgeOutput.Text = results.Count == 0
            ? "No matches."
            : string.Join("\n\n", results.Select((r, i) => $"[{i + 1}] score={r.Score:F4} id={r.Id}\n{r.Text}"));
    }

    private async Task BuildRagPromptAsync() {
        if (!RequireServices(_knowledgeOutput))
            return;

        if (string.IsNullOrWhiteSpace(_ragQuery.Text)) {
            _knowledgeOutput.Text = "Enter a question first.";
            return;
        }

        var builder = _services!.GetRequiredService<RagContextBuilder>();
        var topK = (int)(_ragTopK.Value ?? 5);
        var result = await builder.BuildAsync(_ragQuery.Text, topK).ConfigureAwait(true);

        _knowledgeOutput.Text = $"~{result.EstimatedTokenCount} estimated tokens, {result.Sources.Count} source(s)\n\n----- Prompt -----\n{result.Prompt}";
    }
}

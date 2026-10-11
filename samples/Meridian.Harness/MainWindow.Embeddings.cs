// ============================================================================
// Meridian
// File: MainWindow.Embeddings.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Embeddings tab: embeds two independent pieces of text and computes their
// cosine similarity -- the direct-.NET equivalent of Meridian.Interop.Com's
// IMeridianEmbeddingService.
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
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

partial class MainWindow {
    private readonly TextBox _embedTextA = new() { AcceptsReturn = true, Height = 60, Watermark = "Text A" };
    private readonly TextBox _embedTextB = new() { AcceptsReturn = true, Height = 60, Watermark = "Text B" };
    private readonly TextBox _embedOutput = Output(160);

    private float[]? _lastVectorA;
    private float[]? _lastVectorB;

    /// <summary>
    /// Builds the Embeddings tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildEmbeddingsTab() {
        var root = Stack();
        root.Children.Add(Heading("Embeddings"));
        root.Children.Add(Help("Embed two pieces of text, then compare them by cosine similarity. Requires \"Embeddings\" enabled on the Setup tab."));

        root.Children.Add(_embedTextA);
        var embedAButton = new Button { Content = "Embed text A" };
        embedAButton.Click += (_, _) => RunSafely(embedAButton, _embedOutput, "Embedding A...", async () => _lastVectorA = await EmbedAsync(_embedTextA.Text, "A"));
        root.Children.Add(embedAButton);

        root.Children.Add(_embedTextB);
        var embedBButton = new Button { Content = "Embed text B" };
        embedBButton.Click += (_, _) => RunSafely(embedBButton, _embedOutput, "Embedding B...", async () => _lastVectorB = await EmbedAsync(_embedTextB.Text, "B"));
        root.Children.Add(embedBButton);

        var similarityButton = new Button { Content = "Cosine similarity (A, B)" };
        similarityButton.Click += (_, _) => RunSafely(similarityButton, _embedOutput, "Comparing...", CompareAsync);
        root.Children.Add(similarityButton);

        root.Children.Add(_embedOutput);
        return new TabItem { Header = "Embeddings", Content = Page(root) };
    }

    private async Task<float[]?> EmbedAsync(string? text, string label) {
        if (!RequireServices(_embedOutput))
            return null;

        if (string.IsNullOrWhiteSpace(text)) {
            _embedOutput.Text = $"Enter text {label} first.";
            return null;
        }

        var embeddings = _services!.GetRequiredService<IEmbeddingService>();
        var vector = await embeddings.EmbedAsync(text).ConfigureAwait(true);
        _embedOutput.Text = $"Text {label}: {vector.Length} dimensions. First 8 values: [{string.Join(", ", vector.Take(8).Select(v => v.ToString("F4")))}, ...]";
        return vector;
    }

    private async Task CompareAsync() {
        if (!RequireServices(_embedOutput))
            return;

        _lastVectorA ??= await EmbedAsync(_embedTextA.Text, "A").ConfigureAwait(true);
        _lastVectorB ??= await EmbedAsync(_embedTextB.Text, "B").ConfigureAwait(true);

        if (_lastVectorA is null || _lastVectorB is null)
            return;

        if (_lastVectorA.Length != _lastVectorB.Length) {
            _embedOutput.Text = $"Vectors have different lengths ({_lastVectorA.Length} vs {_lastVectorB.Length}) -- they must come from the same embedding model.";
            return;
        }

        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < _lastVectorA.Length; i++) {
            dot += _lastVectorA[i] * _lastVectorB[i];
            magA += _lastVectorA[i] * _lastVectorA[i];
            magB += _lastVectorB[i] * _lastVectorB[i];
        }
        var similarity = dot / (Math.Sqrt(magA) * Math.Sqrt(magB));

        _embedOutput.Text = $"Cosine similarity: {similarity:F4} (range -1..1, higher = more similar)";
    }
}

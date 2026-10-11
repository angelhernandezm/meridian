// ============================================================================
// Meridian
// File: HybridRetrieverTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for HybridRetriever: verifies that semanticWeight/keywordWeight
// actually control ranking, using a fake embedding service so the semantic and
// keyword signals can be pointed at different candidates independently.
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

using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.Retrieval;
using Meridian.Knowledge.SemanticStore;
using Xunit;

namespace Meridian.Tests.Knowledge;

/// <summary>
/// A fake <see cref="IEmbeddingService" /> that maps fixed strings to fixed vectors.
/// </summary>
/// <param name="vectorsByText">The vectors by text.</param>
internal sealed class FakeEmbeddingService(IReadOnlyDictionary<string, float[]> vectorsByText) : IEmbeddingService {
    /// <summary>
    /// Embeds a single piece of text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The embedding vector.</returns>
    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default) =>
        Task.FromResult(vectorsByText.TryGetValue(text, out var vector) ? vector : [0, 0]);

    /// <summary>
    /// Embeds a batch of texts in one provider call.
    /// </summary>
    /// <param name="texts">The texts to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The embedding vectors, in the same order as <paramref name="texts" />.</returns>
    public Task<IReadOnlyList<float[]>> EmbedManyAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<float[]>>(texts.Select(t => vectorsByText.TryGetValue(t, out var v) ? v : [0, 0]).ToArray());
}

/// <summary>
/// Unit tests for <see cref="HybridRetriever" />.
/// </summary>
public sealed class HybridRetrieverTests {
    /// <summary>
    /// Build store as an asynchronous operation.
    /// </summary>
    /// <returns>A Task&lt;ISemanticStore&gt; representing the asynchronous operation.</returns>
    private static async Task<ISemanticStore> BuildStoreAsync() {
        var store = new InMemorySemanticStore();
        // "semantic-match" has a vector identical to the query but no keyword overlap.
        await store.UpsertAsync(new SemanticRecord("semantic-match", [1, 0], "completely unrelated words"));
        // "keyword-match" has an orthogonal (unrelated) vector but exact keyword overlap.
        await store.UpsertAsync(new SemanticRecord("keyword-match", [0, 1], "apple banana"));
        return store;
    }

    /// <summary>
    /// With semanticWeight=1 and keywordWeight=0, the vector-similar candidate ranks first.
    /// </summary>
    [Fact]
    public async Task RetrieveAsync_PureSemanticWeight_RanksVectorMatchFirst() {
        var embeddings = new FakeEmbeddingService(new Dictionary<string, float[]> { ["apple banana"] = [1, 0] });
        var retriever = new HybridRetriever(embeddings, await BuildStoreAsync(), semanticWeight: 1.0, keywordWeight: 0.0);

        var results = await retriever.RetrieveAsync("apple banana", topK: 2);

        Assert.Equal("semantic-match", results[0].Id);
    }

    /// <summary>
    /// With semanticWeight=0 and keywordWeight=1, the keyword-overlapping candidate ranks first.
    /// </summary>
    [Fact]
    public async Task RetrieveAsync_PureKeywordWeight_RanksKeywordMatchFirst() {
        var embeddings = new FakeEmbeddingService(new Dictionary<string, float[]> { ["apple banana"] = [1, 0] });
        var retriever = new HybridRetriever(embeddings, await BuildStoreAsync(), semanticWeight: 0.0, keywordWeight: 1.0);

        var results = await retriever.RetrieveAsync("apple banana", topK: 2);

        Assert.Equal("keyword-match", results[0].Id);
    }

    /// <summary>
    /// The result count never exceeds the requested topK, even with more candidates available.
    /// </summary>
    [Fact]
    public async Task RetrieveAsync_RespectsTopK() {
        var embeddings = new FakeEmbeddingService(new Dictionary<string, float[]>());
        var retriever = new HybridRetriever(embeddings, await BuildStoreAsync());

        var results = await retriever.RetrieveAsync("apple banana", topK: 1);

        Assert.Single(results);
    }
}

// ============================================================================
// Meridian
// File: SemanticMemory.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Default IMeridianMemory implementation: embeds and stores facts in a
// Meridian.Knowledge ISemanticStore, recalling them by vector similarity.
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
using Meridian.Knowledge.SemanticStore;

namespace Meridian.Agents.Memory;

/// <summary>
/// Default <see cref="IMeridianMemory"/> implementation: embeds and stores facts in a Meridian.Knowledge
/// <see cref="ISemanticStore"/>, recalling them by vector similarity.
/// </summary>
/// <param name="embeddings">The embedding service used to embed remembered facts and recall queries.</param>
/// <param name="store">The semantic store facts are persisted to and recalled from.</param>
public sealed class SemanticMemory(IEmbeddingService embeddings, ISemanticStore store) : IMeridianMemory {
    /// <summary>
    /// The embeddings
    /// </summary>
    private readonly IEmbeddingService _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
    /// <summary>
    /// The store
    /// </summary>
    private readonly ISemanticStore _store = store ?? throw new ArgumentNullException(nameof(store));


    /// <summary>
    /// Saves a fact to long-term memory.
    /// </summary>
    /// <param name="text">The fact to remember.</param>
    /// <param name="metadata">An open-ended metadata bag to store alongside the fact, or <see langword="null" /> for none.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;System.String&gt; representing the asynchronous operation.</returns>
    public async Task<string> RememberAsync(string text, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var id = Guid.NewGuid().ToString("n");
        var vector = await _embeddings.EmbedAsync(text, cancellationToken).ConfigureAwait(false);
        await _store.UpsertAsync(new SemanticRecord(id, vector, text, metadata), cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Recalls the <paramref name="topK" /> facts most relevant to <paramref name="query" />.
    /// </summary>
    /// <param name="query">The natural-language recall query.</param>
    /// <param name="topK">The maximum number of memories to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;IReadOnlyList`1&gt; representing the asynchronous operation.</returns>
    public async Task<IReadOnlyList<MemoryEntry>> RecallAsync(string query, int topK = 5, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var queryVector = await _embeddings.EmbedAsync(query, cancellationToken).ConfigureAwait(false);
        var hits = await _store.SearchAsync(queryVector, topK, cancellationToken).ConfigureAwait(false);

        return hits.Select(hit => new MemoryEntry(hit.Record.Id, hit.Record.Text, hit.Score, hit.Record.Metadata)).ToArray();
    }

    /// <summary>
    /// Forgets a previously remembered fact by id.
    /// </summary>
    /// <param name="id">The id of the memory to forget.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the memory is removed (or confirmed absent).</returns>
    public Task ForgetAsync(string id, CancellationToken cancellationToken = default) => _store.DeleteAsync(id, cancellationToken);
}
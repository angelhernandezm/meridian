// ============================================================================
// Meridian
// File: SemanticRetriever.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Default IRetriever implementation: embeds the query via IEmbeddingService and
// performs a similarity search against an ISemanticStore.
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

using System.Diagnostics;
using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.SemanticStore;
using Meridian.Observability.Metrics;
using Meridian.Observability.Tracing;

namespace Meridian.Knowledge.Retrieval;

/// <summary>
/// Default <see cref="IRetriever" /> implementation: embeds the query via <see cref="IEmbeddingService" /> and
/// performs a similarity search against an <see cref="ISemanticStore" />.
/// </summary>
/// <param name="embeddings">The embedding service used to embed queries.</param>
/// <param name="store">The semantic store to search.</param>
public sealed class SemanticRetriever(IEmbeddingService embeddings, ISemanticStore store) : IRetriever {
    /// <summary>
    /// The embeddings
    /// </summary>
    private readonly IEmbeddingService _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
    /// <summary>
    /// The store
    /// </summary>
    private readonly ISemanticStore _store = store ?? throw new ArgumentNullException(nameof(store));

    /// <summary>
    /// Retrieves the passages most relevant to <paramref name="query" />.
    /// </summary>
    /// <param name="query">The natural-language query.</param>
    /// <param name="topK">The maximum number of passages to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;IReadOnlyList`1&gt; representing the asynchronous operation.</returns>
    public async Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        using var activity = MeridianTelemetry.ActivitySource.StartActivity("meridian.retrieval.semantic");
        var stopwatch = Stopwatch.StartNew();

        var queryVector = await _embeddings.EmbedAsync(query, cancellationToken).ConfigureAwait(false);
        var hits = await _store.SearchAsync(queryVector, topK, cancellationToken).ConfigureAwait(false);
        var passages = hits.Select(hit => new RetrievedPassage(hit.Record.Id, hit.Record.Text, hit.Score, hit.Record.Metadata)).ToArray();

        stopwatch.Stop();
        MeridianMetrics.Latency.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "retrieval.semantic"));
        MeridianMetrics.RetrievalHits.Record(passages.Length, new KeyValuePair<string, object?>("kind", "semantic"));
        activity?.SetTag("meridian.retrieval.hits", passages.Length);

        return passages;
    }
}
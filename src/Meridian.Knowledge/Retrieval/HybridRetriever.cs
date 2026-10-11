// ============================================================================
// Meridian
// File: HybridRetriever.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// IRetriever that fuses vector similarity with lexical overlap. Over-fetches a
// candidate set by vector similarity, re-scores each candidate as a weighted
// blend of its semantic score and its KeywordScorer score, then returns the
// top K by the fused score. Catches exact terms (ids, codes, rare words) that
// pure embedding similarity tends to under-rank.
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
/// Combines semantic (vector) and keyword (lexical) scoring via weighted fusion.
/// </summary>
/// <param name="embeddings">The embedding service used to embed queries.</param>
/// <param name="store">The semantic store to search for candidates.</param>
/// <param name="semanticWeight">The weight given to each candidate's semantic (vector similarity) score during fusion.</param>
/// <param name="keywordWeight">The weight given to each candidate's keyword (lexical overlap) score during fusion.</param>
public sealed class HybridRetriever(IEmbeddingService embeddings, ISemanticStore store, double semanticWeight = 0.6,
                                    double keywordWeight = 0.4) : IRetriever {
    /// <summary>
    /// The embeddings
    /// </summary>
    private readonly IEmbeddingService _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
    /// <summary>
    /// The store
    /// </summary>
    private readonly ISemanticStore _store = store ?? throw new ArgumentNullException(nameof(store));

    /// <summary>
    /// Gets or sets how many candidates to pull by vector similarity before fusion re-ranks and trims to <c>topK</c>.
    /// </summary>
    /// <value>The candidate multiplier. Defaults to <c>4</c>.</value>
    public int CandidateMultiplier { get; init; } = 4;

    /// <summary>
    /// Retrieves the passages most relevant to <paramref name="query" />.
    /// </summary>
    /// <param name="query">The natural-language query.</param>
    /// <param name="topK">The maximum number of passages to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;IReadOnlyList`1&gt; representing the asynchronous operation.</returns>
    public async Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        using var activity = MeridianTelemetry.ActivitySource.StartActivity("meridian.retrieval.hybrid");
        var stopwatch = Stopwatch.StartNew();

        var queryVector = await _embeddings.EmbedAsync(query, cancellationToken).ConfigureAwait(false);
        var candidateCount = Math.Max(topK * CandidateMultiplier, topK);
        var candidates = await _store.SearchAsync(queryVector, candidateCount, cancellationToken).ConfigureAwait(false);

        var fused = candidates
            .Select(hit => {
                var keywordScore = KeywordScorer.Score(query, hit.Record.Text);
                var fusedScore = (semanticWeight * hit.Score) + (keywordWeight * keywordScore);
                return new RetrievedPassage(hit.Record.Id, hit.Record.Text, fusedScore, hit.Record.Metadata);
            })
            .OrderByDescending(passage => passage.Score)
            .Take(topK)
            .ToArray();

        stopwatch.Stop();
        MeridianMetrics.Latency.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", "retrieval.hybrid"));
        MeridianMetrics.RetrievalHits.Record(fused.Length, new KeyValuePair<string, object?>("kind", "hybrid"));
        activity?.SetTag("meridian.retrieval.hits", fused.Length);

        return fused;
    }
}
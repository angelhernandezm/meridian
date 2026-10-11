// ============================================================================
// Meridian
// File: InMemorySemanticStore.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Default ISemanticStore implementation: a process-local, thread-safe vector
// store using brute-force cosine similarity. Fine for development, small
// corpora, and tests; swap for a durable ISemanticStore in production.
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

using System.Collections.Concurrent;

namespace Meridian.Knowledge.SemanticStore;

/// <summary>
/// Default <see cref="ISemanticStore" /> implementation: a process-local, thread-safe vector store using
/// brute-force cosine similarity. Fine for development, small corpora, and tests; swap for a durable
/// <see cref="ISemanticStore" /> in production.
/// </summary>
public sealed class InMemorySemanticStore : ISemanticStore {
    /// <summary>
    /// The records
    /// </summary>
    private readonly ConcurrentDictionary<string, SemanticRecord> _records = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task UpsertAsync(SemanticRecord record, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(record);
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Inserts or replaces a batch of records.
    /// </summary>
    /// <param name="records">The records to upsert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once every record is stored.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Task UpsertManyAsync(IReadOnlyList<SemanticRecord> records, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(records);
        foreach (var record in records)
            _records[record.Id] = record;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the <paramref name="topK" /> records most similar to <paramref name="queryVector" />, highest score first.
    /// </summary>
    /// <param name="queryVector">The query embedding vector to compare records against.</param>
    /// <param name="topK">The maximum number of hits to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching hits, highest score first.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Task<IReadOnlyList<SemanticSearchHit>> SearchAsync(float[] queryVector, int topK, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(queryVector);
        if (topK <= 0)
            return Task.FromResult<IReadOnlyList<SemanticSearchHit>>([]);

        IReadOnlyList<SemanticSearchHit> hits = _records.Values
            .Select(record => new SemanticSearchHit(record, CosineSimilarity(queryVector, record.Vector)))
            .OrderByDescending(hit => hit.Score)
            .Take(topK)
            .ToArray();

        return Task.FromResult(hits);
    }

    /// <summary>
    /// Removes a record by id. No-op when the id is not present.
    /// </summary>
    /// <param name="id">The id of the record to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the record is removed (or confirmed absent).</returns>
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) {
        _records.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Computes cosine similarity between two equal-length vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cosine similarity, in the range [-1, 1]; <c>0</c> when either vector is the zero vector.</returns>
    /// <exception cref="System.ArgumentException">Vectors must have the same dimensionality.</exception>
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b) {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same dimensionality.");

        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length; i++) {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        if (magA == 0 || magB == 0)
            return 0;

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
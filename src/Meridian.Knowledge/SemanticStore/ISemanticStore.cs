// ============================================================================
// Meridian
// File: ISemanticStore.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The pluggable vector-store abstraction Meridian.Knowledge's retrieval and
// Meridian.Agents' long-term memory are built on. Swap InMemorySemanticStore
// for a durable backend (Azure AI Search, Qdrant, Postgres/pgvector, ...) by
// implementing this interface and registering it via options.AddSemanticStore.
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

namespace Meridian.Knowledge.SemanticStore;

/// <summary>
/// A pluggable store of embedded text, searchable by vector similarity.
/// </summary>
public interface ISemanticStore {
    /// <summary>
    /// Inserts or replaces a record by <see cref="SemanticRecord.Id" />.
    /// </summary>
    /// <param name="record">The record to upsert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the record is stored.</returns>
    Task UpsertAsync(SemanticRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or replaces a batch of records.
    /// </summary>
    /// <param name="records">The records to upsert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once every record is stored.</returns>
    Task UpsertManyAsync(IReadOnlyList<SemanticRecord> records, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the <paramref name="topK" /> records most similar to <paramref name="queryVector" />, highest score first.
    /// </summary>
    /// <param name="queryVector">The query embedding vector to compare records against.</param>
    /// <param name="topK">The maximum number of hits to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching hits, highest score first.</returns>
    Task<IReadOnlyList<SemanticSearchHit>> SearchAsync(float[] queryVector, int topK, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a record by id. No-op when the id is not present.
    /// </summary>
    /// <param name="id">The id of the record to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the record is removed (or confirmed absent).</returns>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
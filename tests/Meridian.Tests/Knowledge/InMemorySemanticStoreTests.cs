// ============================================================================
// Meridian
// File: InMemorySemanticStoreTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for InMemorySemanticStore: cosine similarity edge cases, and
// upsert/search/delete round-tripping.
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

using Meridian.Knowledge.SemanticStore;
using Xunit;

namespace Meridian.Tests.Knowledge;

/// <summary>
/// Unit tests for <see cref="InMemorySemanticStore" />.
/// </summary>
public sealed class InMemorySemanticStoreTests {
    /// <summary>
    /// Identical vectors have a cosine similarity of 1.
    /// </summary>
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne() {
        float[] vector = [1, 2, 3];
        Assert.Equal(1.0, InMemorySemanticStore.CosineSimilarity(vector, vector), precision: 10);
    }

    /// <summary>
    /// Orthogonal vectors have a cosine similarity of 0.
    /// </summary>
    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero() {
        float[] a = [1, 0];
        float[] b = [0, 1];
        Assert.Equal(0.0, InMemorySemanticStore.CosineSimilarity(a, b), precision: 10);
    }

    /// <summary>
    /// Opposite vectors have a cosine similarity of -1.
    /// </summary>
    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne() {
        float[] a = [1, 0];
        float[] b = [-1, 0];
        Assert.Equal(-1.0, InMemorySemanticStore.CosineSimilarity(a, b), precision: 10);
    }

    /// <summary>
    /// A zero vector yields a similarity of 0 rather than dividing by zero.
    /// </summary>
    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZero() {
        float[] a = [0, 0];
        float[] b = [1, 1];
        Assert.Equal(0.0, InMemorySemanticStore.CosineSimilarity(a, b));
    }

    /// <summary>
    /// Vectors of different lengths are rejected.
    /// </summary>
    [Fact]
    public void CosineSimilarity_MismatchedLengths_Throws() {
        float[] a = [1, 2, 3];
        float[] b = [1, 2];
        Assert.Throws<ArgumentException>(() => InMemorySemanticStore.CosineSimilarity(a, b));
    }

    /// <summary>
    /// Search returns the closest records first, limited to topK.
    /// </summary>
    [Fact]
    public async Task SearchAsync_ReturnsClosestRecordsFirst() {
        var store = new InMemorySemanticStore();
        await store.UpsertAsync(new SemanticRecord("far", [0, 1], "far"));
        await store.UpsertAsync(new SemanticRecord("near", [1, 0], "near"));

        var hits = await store.SearchAsync([1, 0], topK: 1);

        var hit = Assert.Single(hits);
        Assert.Equal("near", hit.Record.Id);
    }

    /// <summary>
    /// A deleted record no longer appears in search results.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesRecordFromSearch() {
        var store = new InMemorySemanticStore();
        await store.UpsertAsync(new SemanticRecord("a", [1, 0], "a"));

        await store.DeleteAsync("a");
        var hits = await store.SearchAsync([1, 0], topK: 5);

        Assert.Empty(hits);
    }
}
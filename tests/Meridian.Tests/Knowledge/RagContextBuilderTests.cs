// ============================================================================
// Meridian
// File: RagContextBuilderTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for RagContextBuilder: numbered context formatting, source
// pass-through, instructions prepending, and the token-count estimate.
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

using Meridian.Knowledge.Retrieval;
using Xunit;

namespace Meridian.Tests.Knowledge;

/// <summary>
/// A fake <see cref="IRetriever" /> that returns a fixed set of passages regardless of query.
/// </summary>
/// <param name="passages">The passages to return from every <see cref="RetrieveAsync" /> call.</param>
internal sealed class FakeRetriever(IReadOnlyList<RetrievedPassage> passages) : IRetriever {
    /// <summary>
    /// Retrieves the passages most relevant to <paramref name="query" />.
    /// </summary>
    /// <param name="query">The natural-language query.</param>
    /// <param name="topK">The maximum number of passages to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching passages, highest relevance first.</returns>
    public Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default) =>
        Task.FromResult(passages.Take(topK).ToList() as IReadOnlyList<RetrievedPassage>);
}

/// <summary>
/// Unit tests for <see cref="RagContextBuilder" />.
/// </summary>
public sealed class RagContextBuilderTests {
    /// <summary>The context block numbers each passage as [1], [2], ... in retrieval order.</summary>
    [Fact]
    public async Task BuildAsync_NumbersPassagesInOrder() {
        var retriever = new FakeRetriever([
            new RetrievedPassage("1", "first passage", 0.9),
            new RetrievedPassage("2", "second passage", 0.8),
        ]);
        var builder = new RagContextBuilder(retriever);

        var result = await builder.BuildAsync("a question");

        Assert.Contains("[1] first passage", result.Context);
        Assert.Contains("[2] second passage", result.Context);
    }

    /// <summary>The sources list exactly mirrors what the retriever returned.</summary>
    [Fact]
    public async Task BuildAsync_SourcesMatchRetrievedPassages() {
        var passages = new RetrievedPassage[] { new("1", "text", 0.5) };
        var builder = new RagContextBuilder(new FakeRetriever(passages));

        var result = await builder.BuildAsync("q");

        Assert.Same(passages[0], result.Sources[0]);
    }

    /// <summary>When instructions are supplied, they appear before the context block in the prompt.</summary>
    [Fact]
    public async Task BuildAsync_PrependsInstructions() {
        var builder = new RagContextBuilder(new FakeRetriever([new RetrievedPassage("1", "text", 0.5)]));

        var result = await builder.BuildAsync("q", instructions: "Be concise.");

        Assert.StartsWith("Be concise.", result.Prompt);
    }

    /// <summary>The estimated token count is the prompt's character count divided by four.</summary>
    [Fact]
    public async Task BuildAsync_EstimatesTokenCountFromPromptLength() {
        var builder = new RagContextBuilder(new FakeRetriever([new RetrievedPassage("1", "text", 0.5)]));

        var result = await builder.BuildAsync("q");

        Assert.Equal(result.Prompt.Length / 4, result.EstimatedTokenCount);
    }

    /// <summary>A blank query is rejected.</summary>
    [Fact]
    public async Task BuildAsync_BlankQuery_Throws() {
        var builder = new RagContextBuilder(new FakeRetriever([]));
        await Assert.ThrowsAsync<ArgumentException>(() => builder.BuildAsync("   "));
    }
}

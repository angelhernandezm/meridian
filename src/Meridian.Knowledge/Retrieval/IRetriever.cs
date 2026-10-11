// ============================================================================
// Meridian
// File: IRetriever.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The RAG entry point: given a natural-language query, returns the most
// relevant passages from a Meridian.Knowledge semantic store. Consumed directly
// or wrapped as a MAF AIContextProvider by Meridian.Agents.
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

namespace Meridian.Knowledge.Retrieval;

/// <summary>
/// A single passage returned by an <see cref="IRetriever" />, with its relevance score.
/// </summary>
/// <param name="Id">The source record's unique identifier.</param>
/// <param name="Text">The passage text.</param>
/// <param name="Score">The relevance score (meaning depends on the retriever; higher is more relevant).</param>
/// <param name="Metadata">An open-ended metadata bag carried from the source record, or <see langword="null" /> when none is set.</param>
public sealed record RetrievedPassage(string Id, string Text, double Score, IReadOnlyDictionary<string, string>? Metadata = null);

/// <summary>
/// Retrieves the passages most relevant to a natural-language query.
/// </summary>
public interface IRetriever {
    /// <summary>
    /// Retrieves the passages most relevant to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The natural-language query.</param>
    /// <param name="topK">The maximum number of passages to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching passages, highest relevance first.</returns>
    Task<IReadOnlyList<RetrievedPassage>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default);
}
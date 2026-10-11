// ============================================================================
// Meridian
// File: KeywordScorer.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Lightweight lexical scorer: the fraction of a query's terms that appear in a
// candidate passage. Pure vector similarity misses exact identifiers, codes, and
// rare terms embeddings don't separate well; this gives HybridRetriever a cheap
// second signal to fuse with semantic similarity, without a search-engine
// dependency.
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

using System.Text.RegularExpressions;

namespace Meridian.Knowledge.Retrieval;

/// <summary>
/// A cheap, dependency-free lexical overlap score between a query and a passage.
/// </summary>
public static partial class KeywordScorer {
    /// <summary>
    /// Returns the fraction of <paramref name="query" />'s distinct terms found in <paramref name="text" />.
    /// </summary>
    /// <param name="query">The query to score terms from.</param>
    /// <param name="text">The candidate passage to search for those terms in.</param>
    /// <returns>A score in the range [0, 1]; <c>0</c> when <paramref name="query" /> has no terms.</returns>
    public static double Score(string query, string text) {
        var queryTerms = Tokenize(query);
        if (queryTerms.Count == 0)
            return 0;

        var textTerms = Tokenize(text);
        var matches = queryTerms.Count(textTerms.Contains);
        return (double)matches / queryTerms.Count;
    }

    /// <summary>
    /// Splits text into a case-insensitive set of alphanumeric tokens.
    /// </summary>
    /// <param name="text">The text to tokenize, or <see langword="null" /> (treated as empty).</param>
    /// <returns>The distinct tokens found in <paramref name="text" />.</returns>
    private static HashSet<string> Tokenize(string text) =>
        TokenPattern().Matches(text ?? string.Empty).Select(m => m.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tokens the pattern.
    /// </summary>
    /// <returns>Regex.</returns>
    [GeneratedRegex(@"[A-Za-z0-9]+")]
    private static partial Regex TokenPattern();
}
// ============================================================================
// Meridian
// File: KeywordScorerTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for KeywordScorer: term-overlap scoring behavior.
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
/// Unit tests for <see cref="KeywordScorer" />.
/// </summary>
public sealed class KeywordScorerTests {
    /// <summary>
    /// When every query term appears in the text, the score is 1.
    /// </summary>
    [Fact]
    public void Score_AllTermsPresent_ReturnsOne() {
        Assert.Equal(1.0, KeywordScorer.Score("hello world", "hello there, world!"));
    }

    /// <summary>
    /// When no query term appears in the text, the score is 0.
    /// </summary>
    [Fact]
    public void Score_NoTermsPresent_ReturnsZero() {
        Assert.Equal(0.0, KeywordScorer.Score("hello world", "goodbye moon"));
    }

    /// <summary>
    /// When half the query terms appear, the score is 0.5.
    /// </summary>
    [Fact]
    public void Score_HalfTermsPresent_ReturnsOneHalf() {
        Assert.Equal(0.5, KeywordScorer.Score("alpha beta", "alpha gamma"));
    }

    /// <summary>
    /// Matching is case-insensitive.
    /// </summary>
    [Fact]
    public void Score_IsCaseInsensitive() {
        Assert.Equal(1.0, KeywordScorer.Score("HELLO", "hello world"));
    }

    /// <summary>
    /// A blank query has no terms to match, so the score is 0 rather than dividing by zero.
    /// </summary>
    [Fact]
    public void Score_EmptyQuery_ReturnsZero() {
        Assert.Equal(0.0, KeywordScorer.Score("", "any text"));
    }
}
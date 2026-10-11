// ============================================================================
// Meridian
// File: ConversationIdTests.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Unit tests for ConversationId: generation uniqueness, string conversion.
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

using Meridian.Context;
using Xunit;

namespace Meridian.Tests.Context;

/// <summary>
/// Unit tests for <see cref="ConversationId" />.
/// </summary>
public sealed class ConversationIdTests {
    /// <summary>
    /// Two calls to <see cref="ConversationId.New" /> never return the same value.
    /// </summary>
    [Fact]
    public void New_GeneratesUniqueValues() {
        var a = ConversationId.New();
        var b = ConversationId.New();

        Assert.NotEqual(a, b);
        Assert.False(string.IsNullOrWhiteSpace(a.Value));
    }

    /// <summary>
    /// The implicit string conversion returns the underlying value.
    /// </summary>
    [Fact]
    public void ImplicitStringConversion_ReturnsValue() {
        var id = new ConversationId("abc123");
        string asString = id;

        Assert.Equal("abc123", asString);
    }

    /// <summary>
    /// <see cref="ConversationId.ToString" /> returns the underlying value.
    /// </summary>
    [Fact]
    public void ToString_ReturnsValue() {
        var id = new ConversationId("abc123");
        Assert.Equal("abc123", id.ToString());
    }
}

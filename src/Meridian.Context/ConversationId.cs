// ============================================================================
// Meridian
// File: ConversationId.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A lightweight, strongly-typed identifier for a conversation/session, used to
// correlate agent runs, memory records, and telemetry across Meridian subsystems.
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

namespace Meridian.Context;

/// <summary>
/// Strongly-typed conversation identifier. Wraps a string so call sites cannot
/// accidentally pass an arbitrary string where a conversation id is expected.
/// </summary>
/// <param name="Value">The underlying string identifier.</param>
public readonly record struct ConversationId(string Value) {
    /// <summary>
    /// Creates a new, random conversation id.
    /// </summary>
    /// <returns>A <see cref="ConversationId"/> wrapping a freshly generated GUID.</returns>
    public static ConversationId New() => new(Guid.NewGuid().ToString("n"));

    /// <summary>
    /// Returns the underlying string identifier.
    /// </summary>
    /// <returns>The value of <see cref="Value"/>.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="ConversationId"/> to its underlying string.
    /// </summary>
    /// <param name="id">The conversation id to convert.</param>
    /// <returns>The value of <paramref name="id"/>.</returns>
    public static implicit operator string(ConversationId id) => id.Value;
}

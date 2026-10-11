// ============================================================================
// Meridian
// File: SemanticRecord.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A single unit stored in an ISemanticStore: an id, its embedding vector, the
// source text, and an open-ended metadata bag for filtering/display.
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
/// A stored item plus its embedding vector.
/// </summary>
/// <param name="Id">The record's unique identifier.</param>
/// <param name="Vector">The record's embedding vector.</param>
/// <param name="Text">The record's source text.</param>
/// <param name="Metadata">An open-ended metadata bag for filtering/display, or <see langword="null" /> when none is set.</param>
public sealed record SemanticRecord(string Id, float[] Vector, string Text, IReadOnlyDictionary<string, string>? Metadata = null);

/// <summary>
/// A search hit: a stored record plus its similarity score against the query vector.
/// </summary>
/// <param name="Record">The matched record.</param>
/// <param name="Score">The similarity score against the query vector (higher is more similar).</param>
public sealed record SemanticSearchHit(SemanticRecord Record, double Score);
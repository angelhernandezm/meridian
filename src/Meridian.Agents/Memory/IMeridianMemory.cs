// ============================================================================
// Meridian
// File: IMeridianMemory.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Long-term agent memory: durable facts an agent can save for itself and recall
// later, independent of any single chat session's transcript. Distinct from MAF's
// own per-session ChatHistoryProvider, which only covers one conversation's turns.
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

namespace Meridian.Agents.Memory;

/// <summary>A single recalled memory, with its relevance score against the recall query.</summary>
/// <param name="Id">The memory's unique identifier.</param>
/// <param name="Text">The remembered text.</param>
/// <param name="Score">The relevance score against the recall query (higher is more relevant).</param>
/// <param name="Metadata">An open-ended metadata bag, or <see langword="null"/> when none was set.</param>
public sealed record MemoryEntry(string Id, string Text, double Score, IReadOnlyDictionary<string, string>? Metadata = null);

/// <summary>Durable, semantically-searchable long-term memory for agents.</summary>
public interface IMeridianMemory {
    /// <summary>
    /// Saves a fact to long-term memory.
    /// </summary>
    /// <param name="text">The fact to remember.</param>
    /// <param name="metadata">An open-ended metadata bag to store alongside the fact, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated id of the stored memory.</returns>
    Task<string> RememberAsync(string text, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalls the <paramref name="topK"/> facts most relevant to <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The natural-language recall query.</param>
    /// <param name="topK">The maximum number of memories to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching memories, highest relevance first.</returns>
    Task<IReadOnlyList<MemoryEntry>> RecallAsync(string query, int topK = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forgets a previously remembered fact by id.
    /// </summary>
    /// <param name="id">The id of the memory to forget.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the memory is removed (or confirmed absent).</returns>
    Task ForgetAsync(string id, CancellationToken cancellationToken = default);
}
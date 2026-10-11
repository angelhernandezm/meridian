// ============================================================================
// Meridian
// File: IMeridianMemoryService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-visible, dual-interface contract for Meridian's long-term semantic
// memory: save a fact, recall the facts most relevant to a query, forget one.
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

using System.Runtime.InteropServices;

namespace Meridian.Interop.Com.Interfaces;

/// <summary>
/// Long-term semantic memory, callable from VB6/VFP/Delphi/VBA.
/// </summary>
[ComVisible(true)]
[Guid("73FFD57D-8692-4605-B0CE-E72A8DAC4324")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IMeridianMemoryService {
    /// <summary>
    /// Saves <paramref name="text" /> to long-term memory.
    /// </summary>
    /// <param name="text">The fact to remember.</param>
    /// <returns>The generated id of the stored memory.</returns>
    string Remember(string text);

    /// <summary>
    /// Recalls the <paramref name="topK" /> facts most relevant to <paramref name="query" />, as a SAFEARRAY of strings.
    /// </summary>
    /// <param name="query">The natural-language recall query.</param>
    /// <param name="topK">The maximum number of memories to return.</param>
    /// <returns>The matching memory texts, highest relevance first.</returns>
    string[] Recall(string query, int topK);

    /// <summary>
    /// Forgets a previously remembered fact by id.
    /// </summary>
    /// <param name="id">The id of the memory to forget.</param>
    void Forget(string id);
}
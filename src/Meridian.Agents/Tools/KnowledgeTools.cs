// ============================================================================
// Meridian
// File: KnowledgeTools.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Built-in, agent-ready tools over Meridian.Knowledge: lets an agent search its
// own knowledge base mid-conversation instead of only receiving retrieved
// context up front. Works against whichever IRetriever is registered — a plain
// SemanticRetriever or a HybridRetriever — the agent doesn't know or care which.
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
using Microsoft.Extensions.AI;

namespace Meridian.Agents.Tools;

/// <summary>
/// Class KnowledgeTools.
/// </summary>
public static class KnowledgeTools {
    /// <summary>
    /// A "search" tool that retrieves the most relevant passages for a query from <paramref name="retriever"/>
    /// and returns them as a numbered list of text snippets.
    /// </summary>
    /// <param name="retriever">The retriever the tool searches against.</param>
    /// <param name="name">The tool name surfaced to the model.</param>
    /// <param name="description">The tool description surfaced to the model, or <see langword="null"/> for a default description.</param>
    /// <returns>An <see cref="AITool"/> the model can call to search <paramref name="retriever"/>.</returns>
    public static AITool Search(IRetriever retriever, string name = "search", string? description = null) {
        ArgumentNullException.ThrowIfNull(retriever);

        async Task<string> SearchAsync(string query, int topK) {
            var passages = await retriever.RetrieveAsync(query, topK <= 0 ? 5 : topK).ConfigureAwait(false);
            return passages.Count == 0
                ? "No relevant results found."
                : string.Join("\n\n", passages.Select((p, i) => $"[{i + 1}] {p.Text}"));
        }

        return MeridianToolBuilder.FromDelegate(
            SearchAsync,
            name,
            description ?? "Searches the knowledge base and returns the most relevant passages for a query, each prefixed with a [n] citation marker.");
    }
}
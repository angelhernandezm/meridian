// ============================================================================
// Meridian
// File: RagContextBuilder.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Batteries-included RAG: turns a raw IRetriever result into a ready-to-send
// prompt — retrieval, numbered source formatting, and prompt templating in one
// call — plus the source passages and a rough token-count estimate, so callers
// don't hand-assemble context blocks themselves.
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

using System.Text;

namespace Meridian.Knowledge.Retrieval;

/// <summary>
/// A retrieval-augmented prompt ready to send to a chat client, plus the sources it cites and a rough
/// token-count estimate (chars / 4 — a cheap approximation, not a tokenizer-exact count).
/// </summary>
/// <param name="Prompt">The full, ready-to-send prompt (instructions, numbered context, and question).</param>
/// <param name="Context">The numbered context block alone, without instructions or the question.</param>
/// <param name="Sources">The retrieved passages the context was built from, in citation order.</param>
/// <param name="EstimatedTokenCount">A rough token-count estimate for <see cref="Prompt" /> (characters / 4).</param>
public sealed record RagResult(string Prompt, string Context, IReadOnlyList<RetrievedPassage> Sources, int EstimatedTokenCount);

/// <summary>
/// Builds a RAG prompt (retrieval + numbered context + templating) from an <see cref="IRetriever" />.
/// </summary>
/// <param name="retriever">The retriever used to fetch source passages.</param>
public sealed class RagContextBuilder(IRetriever retriever) {
    /// <summary>
    /// The retriever
    /// </summary>
    private readonly IRetriever _retriever = retriever ?? throw new ArgumentNullException(nameof(retriever));

    /// <summary>
    /// Retrieves passages for <paramref name="query" /> and assembles them into a ready-to-send prompt.
    /// </summary>
    /// <param name="query">The natural-language question to answer.</param>
    /// <param name="topK">The maximum number of passages to retrieve and cite.</param>
    /// <param name="instructions">Optional system-style instructions to prepend to the prompt, or <see langword="null" /> for none.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The assembled <see cref="RagResult" />.</returns>
    public async Task<RagResult> BuildAsync(string query, int topK = 5, string? instructions = null, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var sources = await _retriever.RetrieveAsync(query, topK, cancellationToken).ConfigureAwait(false);

        var context = new StringBuilder();
        for (var i = 0; i < sources.Count; i++)
            context.Append('[').Append(i + 1).Append("] ").AppendLine(sources[i].Text).AppendLine();

        var prompt = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(instructions))
            prompt.AppendLine(instructions).AppendLine();
        prompt.AppendLine("Use the numbered context below to answer the question. Cite sources by their [n] marker; if the context doesn't contain the answer, say so.")
              .AppendLine()
              .AppendLine("Context:")
              .Append(context)
              .AppendLine("Question:")
              .Append(query);

        var promptText = prompt.ToString();
        var contextText = context.ToString();
        return new RagResult(promptText, contextText, sources, EstimatedTokenCount: promptText.Length / 4);
    }
}
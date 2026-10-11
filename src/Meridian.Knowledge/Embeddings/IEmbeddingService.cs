// ============================================================================
// Meridian
// File: IEmbeddingService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The opinionated embeddings entry point: turns text into vectors using the
// configured Meridian.Models embedding generator, without callers needing to
// touch Microsoft.Extensions.AI types directly.
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

namespace Meridian.Knowledge.Embeddings;

/// <summary>
/// Produces vector embeddings for text using the configured Meridian.Models embedding provider.
/// </summary>
public interface IEmbeddingService {
    /// <summary>
    /// Embeds a single piece of text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The embedding vector.</returns>
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Embeds a batch of texts in one provider call.
    /// </summary>
    /// <param name="texts">The texts to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The embedding vectors, in the same order as <paramref name="texts" />.</returns>
    Task<IReadOnlyList<float[]>> EmbedManyAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
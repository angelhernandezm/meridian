// ============================================================================
// Meridian
// File: EmbeddingService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Default IEmbeddingService implementation, a thin adapter over an
// IEmbeddingGenerator<string, Embedding<float>> resolved for the default
// (or a specific) Meridian.Models provider.
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

using Microsoft.Extensions.AI;

namespace Meridian.Knowledge.Embeddings;

/// <summary>
/// Default <see cref="IEmbeddingService" /> implementation, a thin adapter over an
/// <see cref="IEmbeddingGenerator{TInput, TEmbedding}" /> resolved for the default (or a specific) Meridian.Models provider.
/// </summary>
/// <param name="generator">The embedding generator to adapt.</param>
public sealed class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> generator) : IEmbeddingService {
    /// <summary>
    /// The generator
    /// </summary>
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator =
        generator ?? throw new ArgumentNullException(nameof(generator));

    /// <summary>
    /// Embeds a single piece of text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;System.Single[]&gt; representing the asynchronous operation.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(text);
        var result = await _generator.GenerateAsync([text], cancellationToken: cancellationToken).ConfigureAwait(false);
        return result[0].Vector.ToArray();
    }

    /// <summary>
    /// Embeds a batch of texts in one provider call.
    /// </summary>
    /// <param name="texts">The texts to embed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;IReadOnlyList`1&gt; representing the asynchronous operation.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public async Task<IReadOnlyList<float[]>> EmbedManyAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(texts);
        var result = await _generator.GenerateAsync(texts, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result.Select(e => e.Vector.ToArray()).ToArray();
    }
}
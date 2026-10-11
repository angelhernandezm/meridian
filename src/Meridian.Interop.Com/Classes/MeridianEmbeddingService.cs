// ============================================================================
// Meridian
// File: MeridianEmbeddingService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-creatable implementation of IMeridianEmbeddingService (ProgId
// "Meridian.EmbeddingService").
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
using Meridian.Interop.Com.Configuration;
using Meridian.Interop.Com.Interfaces;
using Meridian.Interop.Com.Support;
using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.SemanticStore;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Interop.Com.Classes;


/// <summary>
/// Class MeridianEmbeddingService. This class cannot be inherited.
/// Implements the <see cref="IMeridianEmbeddingService" />
/// </summary>
/// <seealso cref="IMeridianEmbeddingService" />
[ComVisible(true)]
[Guid("F116F102-BD61-410B-A1B7-E82E5B54E41C")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IMeridianEmbeddingService))]
[ProgId("Meridian.EmbeddingService")]
public sealed class MeridianEmbeddingService : IMeridianEmbeddingService {
    /// <summary>
    /// The embeddings
    /// </summary>
    private readonly IEmbeddingService _embeddings;

    /// <summary>
    /// COM clients create this type via <c>CreateObject("Meridian.EmbeddingService")</c>, which requires a public parameterless constructor.
    /// </summary>
    public MeridianEmbeddingService() {
        _embeddings = MeridianComHost.Services.GetRequiredService<IEmbeddingService>();
    }


    /// <summary>
    /// Embeds <paramref name="text" />, returning its vector as a SAFEARRAY of doubles.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>The embedding vector.</returns>
    public double[] GetEmbedding(string text) => ComSync.Run(async () => {
        var vector = await _embeddings.EmbedAsync(text).ConfigureAwait(false);
        return Array.ConvertAll(vector, v => (double)v);
    });


    /// <summary>
    /// Computes cosine similarity between two equal-length vectors.
    /// </summary>
    /// <param name="vectorA">The first vector.</param>
    /// <param name="vectorB">The second vector.</param>
    /// <returns>The cosine similarity, in the range [-1, 1].</returns>
    public double CosineSimilarity(double[] vectorA, double[] vectorB) =>
        InMemorySemanticStore.CosineSimilarity(Array.ConvertAll(vectorA, v => (float)v), Array.ConvertAll(vectorB, v => (float)v));
}
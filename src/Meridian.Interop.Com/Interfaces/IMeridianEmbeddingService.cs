// ============================================================================
// Meridian
// File: IMeridianEmbeddingService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-visible, dual-interface contract for embeddings: turns text into a vector
// a legacy COM host can store, compare, or index (e.g. a Visual FoxPro table
// column, a Delphi array), without touching Microsoft.Extensions.AI types.
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
/// Embeddings, callable from VB6/VFP/Delphi/VBA.
/// </summary>
[ComVisible(true)]
[Guid("DCBEE442-161B-46C1-9F78-FA9D3DB2165C")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IMeridianEmbeddingService {
    /// <summary>
    /// Embeds <paramref name="text" />, returning its vector as a SAFEARRAY of doubles.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>The embedding vector.</returns>
    double[] GetEmbedding(string text);

    /// <summary>
    /// Computes cosine similarity between two equal-length vectors.
    /// </summary>
    /// <param name="vectorA">The first vector.</param>
    /// <param name="vectorB">The second vector.</param>
    /// <returns>The cosine similarity, in the range [-1, 1].</returns>
    double CosineSimilarity(double[] vectorA, double[] vectorB);
}
// ============================================================================
// Meridian
// File: IMeridianImageService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-visible, dual-interface contract for image generation, editing, and
// vision analysis. Image bytes cross the COM boundary as a SAFEARRAY of bytes
// (byte[]), which VB6 (As Variant/Byte()), VFP, and Delphi all marshal natively.
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
/// Image generation, editing, and vision analysis, callable from VB6/VFP/Delphi/VBA.
/// </summary>
[ComVisible(true)]
[Guid("4EB4D912-BD44-486A-8E8F-D3DC11DF9B47")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IMeridianImageService {
    /// <summary>
    /// Generates a new image from <paramref name="prompt"/>, returning its raw bytes.
    /// </summary>
    /// <param name="prompt">The generation prompt.</param>
    /// <param name="width">The requested image width, in pixels.</param>
    /// <param name="height">The requested image height, in pixels.</param>
    /// <returns>The generated image's raw bytes.</returns>
    byte[] Generate(string prompt, int width, int height);

    /// <summary>
    /// Edits <paramref name="imageData"/> per <paramref name="prompt"/>, returning the result's raw bytes.
    /// </summary>
    /// <param name="imageData">The source image's raw bytes.</param>
    /// <param name="prompt">The edit instructions.</param>
    /// <param name="width">The requested output width, in pixels.</param>
    /// <param name="height">The requested output height, in pixels.</param>
    /// <returns>The edited image's raw bytes.</returns>
    byte[] Edit(byte[] imageData, string prompt, int width, int height);

    /// <summary>
    /// Describes/answers questions about <paramref name="imageData"/> via a vision-capable chat model.
    /// </summary>
    /// <param name="imageData">The image's raw bytes.</param>
    /// <param name="prompt">The question or instruction about the image.</param>
    /// <returns>The model's text response.</returns>
    string Analyze(byte[] imageData, string prompt);
}
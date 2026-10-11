// ============================================================================
// Meridian
// File: MeridianImageService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-creatable implementation of IMeridianImageService (ProgId
// "Meridian.ImageService").
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

using Meridian.Interop.Com.Configuration;
using Meridian.Interop.Com.Interfaces;
using Meridian.Interop.Com.Support;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace Meridian.Interop.Com.Classes;

/// <summary>
/// Class MeridianImageService. This class cannot be inherited.
/// Implements the <see cref="IMeridianImageService" />
/// </summary>
/// <seealso cref="IMeridianImageService" />
[ComVisible(true)]
[Guid("D5C450C6-CF7E-4C03-BC8F-3400259F8FE0")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(Interfaces.IMeridianImageService))]
[ProgId("Meridian.ImageService")]
public sealed class MeridianImageService :IMeridianImageService {
    private readonly Meridian.Models.Providers.IMeridianImageService _images;

    /// <summary>
    /// COM clients create this type via <c>CreateObject("Meridian.ImageService")</c>, which requires a public parameterless constructor.
    /// </summary>
    public MeridianImageService() {
        _images = MeridianComHost.Services.GetRequiredService<Meridian.Models.Providers.IMeridianImageService>();
    }


    /// <summary>
    /// Generates a new image from <paramref name="prompt" />, returning its raw bytes.
    /// </summary>
    /// <param name="prompt">The generation prompt.</param>
    /// <param name="width">The requested image width, in pixels.</param>
    /// <param name="height">The requested image height, in pixels.</param>
    /// <returns>The generated image's raw bytes.</returns>
    public byte[] Generate(string prompt, int width, int height) => ComSync.Run(async () => {
        var image = await _images.GenerateAsync(prompt, width <= 0 ? 1024 : width, height <= 0 ? 1024 : height).ConfigureAwait(false);
        return image.Data;
    });

    /// <summary>
    /// Edits <paramref name="imageData" /> per <paramref name="prompt" />, returning the result's raw bytes.
    /// </summary>
    /// <param name="imageData">The source image's raw bytes.</param>
    /// <param name="prompt">The edit instructions.</param>
    /// <param name="width">The requested output width, in pixels.</param>
    /// <param name="height">The requested output height, in pixels.</param>
    /// <returns>The edited image's raw bytes.</returns>
    public byte[] Edit(byte[] imageData, string prompt, int width, int height) => ComSync.Run(async () => {
        var image = await _images.EditAsync(imageData, prompt, maskData: null, width <= 0 ? 1024 : width, height <= 0 ? 1024 : height).ConfigureAwait(false);
        return image.Data;
    });


    /// <summary>
    /// Describes/answers questions about <paramref name="imageData" /> via a vision-capable chat model.
    /// </summary>
    /// <param name="imageData">The image's raw bytes.</param>
    /// <param name="prompt">The question or instruction about the image.</param>
    /// <returns>The model's text response.</returns>
    public string Analyze(byte[] imageData, string prompt) => ComSync.Run(() => _images.AnalyzeAsync(imageData, prompt));
}
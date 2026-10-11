// ============================================================================
// Meridian
// File: MeridianImageService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The opinionated image entry point: generation and editing via the configured
// Meridian.Models image generator, and analysis (vision) via the chat client —
// without callers touching Microsoft.Extensions.AI's experimental image types
// directly. Mirrors the three image operations an agent-facing SDK needs most;
// generation/editing are supported for AzureOpenAI and OpenAI today.
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

namespace Meridian.Models.Providers;

/// <summary>
/// A generated or edited image: its raw bytes and media type.
/// </summary>
/// <param name="Data">The raw image bytes.</param>
/// <param name="MediaType">The image's media type (e.g. <c>image/png</c>).</param>
public sealed record GeneratedImage(byte[] Data, string MediaType);

/// <summary>
/// Generates, edits, and analyzes (vision) images through the configured Meridian.Models providers.
/// </summary>
public interface IMeridianImageService {
    /// <summary>
    /// Generates a new image from <paramref name="prompt"/>.
    /// </summary>
    /// <param name="prompt">The generation prompt.</param>
    /// <param name="width">The requested image width, in pixels.</param>
    /// <param name="height">The requested image height, in pixels.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated image.</returns>
    Task<GeneratedImage> GenerateAsync(string prompt, int width = 1024, int height = 1024, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits <paramref name="imageData" /> per <paramref name="prompt" />, optionally constrained to the region <paramref name="maskData" /> marks.
    /// </summary>
    /// <param name="imageData">The source image's raw bytes.</param>
    /// <param name="prompt">The edit instructions.</param>
    /// <param name="maskData">An optional mask image's raw bytes marking the region to edit, or <see langword="null" /> to edit the whole image.</param>
    /// <param name="width">The requested output width, in pixels.</param>
    /// <param name="height">The requested output height, in pixels.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The edited image.</returns>
    Task<GeneratedImage> EditAsync(byte[] imageData, string prompt, byte[]? maskData = null, int width = 1024, int height = 1024, CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes/answers questions about <paramref name="imageData" /> via a vision-capable chat model.
    /// </summary>
    /// <param name="imageData">The image's raw bytes.</param>
    /// <param name="prompt">The question or instruction about the image.</param>
    /// <param name="mediaType">The image's media type.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The model's text response.</returns>
    Task<string> AnalyzeAsync(byte[] imageData, string prompt, string mediaType = "image/png", CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="IMeridianImageService" /> implementation: generation/editing via <paramref name="imageGenerator" />,
/// analysis via <paramref name="chatClient" />.
/// </summary>
/// <param name="chatClient">The chat client used for vision analysis.</param>
/// <param name="imageGenerator">The image generator used for generation and editing.</param>
public sealed class MeridianImageService(IChatClient chatClient, IImageGenerator imageGenerator) : IMeridianImageService {
    /// <summary>
    /// The s download client
    /// </summary>
    private static readonly HttpClient s_downloadClient = new();

    /// <summary>
    /// The chat client
    /// </summary>
    private readonly IChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    /// <summary>
    /// The image generator
    /// </summary>
    private readonly IImageGenerator _imageGenerator = imageGenerator ?? throw new ArgumentNullException(nameof(imageGenerator));


    /// <summary>
    /// Generates a new image from <paramref name="prompt" />.
    /// </summary>
    /// <param name="prompt">The generation prompt.</param>
    /// <param name="width">The requested image width, in pixels.</param>
    /// <param name="height">The requested image height, in pixels.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;GeneratedImage&gt; representing the asynchronous operation.</returns>
    public async Task<GeneratedImage> GenerateAsync(string prompt, int width = 1024, int height = 1024, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var response = await _imageGenerator.GenerateAsync(new ImageGenerationRequest(prompt), BuildOptions(width, height), cancellationToken).ConfigureAwait(false);
        return await MapResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Edits <paramref name="imageData" /> per <paramref name="prompt" />, optionally constrained to the region <paramref name="maskData" /> marks.
    /// </summary>
    /// <param name="imageData">The source image's raw bytes.</param>
    /// <param name="prompt">The edit instructions.</param>
    /// <param name="maskData">An optional mask image's raw bytes marking the region to edit, or <see langword="null" /> to edit the whole image.</param>
    /// <param name="width">The requested output width, in pixels.</param>
    /// <param name="height">The requested output height, in pixels.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;GeneratedImage&gt; representing the asynchronous operation.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public async Task<GeneratedImage> EditAsync(byte[] imageData, string prompt, byte[]? maskData = null, int width = 1024, int height = 1024, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(imageData);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var originals = new List<AIContent> { new DataContent(imageData, "image/png") };
        if (maskData is not null)
            originals.Add(new DataContent(maskData, "image/png"));

        var response = await _imageGenerator.GenerateAsync(new ImageGenerationRequest(prompt, originals), BuildOptions(width, height), cancellationToken).ConfigureAwait(false);
        return await MapResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Describes/answers questions about <paramref name="imageData" /> via a vision-capable chat model.
    /// </summary>
    /// <param name="imageData">The image's raw bytes.</param>
    /// <param name="prompt">The question or instruction about the image.</param>
    /// <param name="mediaType">The image's media type.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A Task&lt;System.String&gt; representing the asynchronous operation.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public async Task<string> AnalyzeAsync(byte[] imageData, string prompt, string mediaType = "image/png", CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(imageData);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        List<AIContent> content = [new TextContent(prompt), new DataContent(imageData, mediaType)];
        var messages = new List<ChatMessage> { new(ChatRole.User, content) };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Text;
    }

    /// <summary>
    /// Builds the image size options for a generation/edit call. Deliberately does not set a response
    /// format or output media type: OpenAI's images API rejects <c>response_format</c> and only accepts
    /// <c>output_format</c> for <c>gpt-image-1</c>, so forcing either 400s on DALL·E models. Providers
    /// therefore respond with inline bytes or a URL; both are handled on the way out by <see cref="MapResponseAsync" />.
    /// </summary>
    /// <param name="width">The requested width, in pixels.</param>
    /// <param name="height">The requested height, in pixels.</param>
    /// <returns>The image generation options.</returns>
    private static ImageGenerationOptions BuildOptions(int width, int height) => new() {
        ImageSize = new System.Drawing.Size(width, height)
    };

    /// <summary>
    /// Maps a provider's image generation response to a <see cref="GeneratedImage" />, downloading the
    /// image when the provider responds with a URL instead of inline bytes.
    /// </summary>
    /// <param name="response">The provider's image generation response.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The mapped <see cref="GeneratedImage" />.</returns>
    /// <exception cref="System.InvalidOperationException">The image provider returned no image content (neither inline bytes nor a URL).</exception>
    private static async Task<GeneratedImage> MapResponseAsync(ImageGenerationResponse response, CancellationToken cancellationToken) {
        var data = response.Contents.OfType<DataContent>().FirstOrDefault();
        if (data is not null)
            return new GeneratedImage(data.Data.ToArray(), data.MediaType ?? "image/png");

        if (response.Contents.OfType<UriContent>().FirstOrDefault() is { } uri) {
            var bytes = await s_downloadClient.GetByteArrayAsync(uri.Uri, cancellationToken).ConfigureAwait(false);
            return new GeneratedImage(bytes, string.IsNullOrWhiteSpace(uri.MediaType) ? "image/png" : uri.MediaType);
        }

        throw new InvalidOperationException("The image provider returned no image content (neither inline bytes nor a URL).");
    }
}
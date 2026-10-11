// ============================================================================
// Meridian
// File: RetryChatClient.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A DelegatingChatClient that retries transient failures with exponential
// backoff. Microsoft.Extensions.AI ships logging/caching/OpenTelemetry/function-
// invocation middleware but no retry stage, so Meridian supplies this one. Added
// to the pipeline via the UseRetry() builder extension below.
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

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace Meridian.Models.Providers;

/// <summary>
/// A <see cref="DelegatingChatClient" /> that retries transient failures with exponential backoff.
/// </summary>
public sealed class RetryChatClient : DelegatingChatClient {
    /// <summary>
    /// The maximum retries
    /// </summary>
    private readonly int _maxRetries;
    /// <summary>
    /// The base delay
    /// </summary>
    private readonly TimeSpan _baseDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryChatClient" /> class.
    /// </summary>
    /// <param name="innerClient">The chat client to wrap and retry calls against.</param>
    /// <param name="maxRetries">The maximum number of retry attempts after the initial call. Negative values are clamped to zero.</param>
    /// <param name="baseDelay">The base delay for exponential backoff between attempts. Negative values are clamped to zero.</param>
    public RetryChatClient(IChatClient innerClient, int maxRetries, TimeSpan baseDelay) : base(innerClient) {
        _maxRetries = Math.Max(0, maxRetries);
        _baseDelay = baseDelay < TimeSpan.Zero ? TimeSpan.Zero : baseDelay;
    }

    /// <summary>
    /// Gets a response, retrying transient failures with exponential backoff up to <see cref="_maxRetries" /> times.
    /// </summary>
    /// <param name="messages">The conversation messages to send.</param>
    /// <param name="options">The chat options to apply, or <see langword="null" /> for defaults.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response once a call succeeds.</returns>
    /// <inheritdoc />
    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) {
        for (var attempt = 0; ; attempt++) {
            try {
                return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            } catch (Exception ex) when (attempt < _maxRetries && IsTransient(ex) && !cancellationToken.IsCancellationRequested) {
                await Task.Delay(BackoffFor(attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Streams a response, retrying transient failures with exponential backoff, but only while
    /// establishing the stream — once updates start flowing, this method does not retry on a later failure.
    /// </summary>
    /// <param name="messages">The conversation messages to send.</param>
    /// <param name="options">The chat options to apply, or <see langword="null" /> for defaults.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A stream of response updates.</returns>
    /// <inheritdoc />
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        // Retry only covers establishing the stream; once updates start flowing we do not retry.
        for (var attempt = 0; ; attempt++) {
            var enumerator = base.GetStreamingResponseAsync(messages, options, cancellationToken).GetAsyncEnumerator(cancellationToken);
            var started = false;
            try {
                while (true) {
                    try {
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                            yield break;
                    } catch (Exception ex) when (!started && attempt < _maxRetries && IsTransient(ex) && !cancellationToken.IsCancellationRequested) {
                        break;
                    }
                    started = true;
                    yield return enumerator.Current;
                }
            } finally {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            if (started)
                yield break;
            await Task.Delay(BackoffFor(attempt), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Computes the exponential backoff delay for a given attempt number.
    /// </summary>
    /// <param name="attempt">The zero-based attempt number.</param>
    /// <returns>The delay to wait before the next attempt.</returns>
    private TimeSpan BackoffFor(int attempt) => TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt));

    /// <summary>
    /// Determines whether an exception is worth retrying. Cancellation and argument errors are never transient.
    /// </summary>
    /// <param name="ex">The exception to classify.</param>
    /// <returns><see langword="true"/> if the call should be retried; otherwise <see langword="false"/>.</returns>
    private static bool IsTransient(Exception ex) => ex is not OperationCanceledException and not ArgumentException and not NotSupportedException;
}

/// <summary>
/// Adds the <see cref="RetryChatClient" /> stage to a chat pipeline.
/// </summary>
public static class RetryChatClientBuilderExtensions {
    /// <summary>
    /// Wraps the pipeline's current client in a <see cref="RetryChatClient"/>.
    /// </summary>
    /// <param name="builder">The chat client builder to extend.</param>
    /// <param name="maxRetries">The maximum number of retry attempts after the initial call.</param>
    /// <param name="baseDelay">The base delay for exponential backoff between attempts.</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    public static ChatClientBuilder UseRetry(this ChatClientBuilder builder, int maxRetries, TimeSpan baseDelay) {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Use(inner => new RetryChatClient(inner, maxRetries, baseDelay));
    }
}

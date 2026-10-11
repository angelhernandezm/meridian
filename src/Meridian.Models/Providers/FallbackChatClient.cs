// ============================================================================
// Meridian
// File: FallbackChatClient.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// An IChatClient that tries each inner client in order, moving to the next one
// on failure. Used to back ModelOptions.FallbackProviders with an ordinary
// IChatClient so the rest of Meridian never has to know fallback is happening.
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
/// Chat client that tries each <paramref name="inner" /> client in order, falling through to the next
/// one when the current one throws. The last client's exception propagates.
/// </summary>
/// <param name="inner">The ordered list of chat clients to try, paired with a human-readable provider label.</param>
public sealed class FallbackChatClient(IReadOnlyList<(IChatClient Chat, string Name)> inner) : IChatClient {
    private readonly IReadOnlyList<(IChatClient Chat, string Name)> _inner = inner.Count > 0
        ? inner
        : throw new ArgumentException("At least one inner chat client is required.", nameof(inner));

    /// <summary>
    /// Gets a response, trying each inner client in order until one succeeds.
    /// </summary>
    /// <param name="messages">The conversation messages to send.</param>
    /// <param name="options">The chat options to apply, or <see langword="null" /> for defaults.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response from the first inner client that succeeds.</returns>
    /// <exception cref="System.AggregateException">All fallback providers failed.</exception>
    /// <exception cref="Exception">Every inner client failed; the last client's own exception propagates unwrapped.</exception>
    /// <related type="Article" href="https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai#request-a-chat-response">Request a chat response.</related>
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) {
        List<Exception>? failures = null;

        for (var i = 0; i < _inner.Count; i++) {
            try {
                return await _inner[i].Chat.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            } catch (Exception ex) when (i < _inner.Count - 1) {
                (failures ??= []).Add(ex);
            }
        }

        // Unreachable when _inner.Count > 0, kept for the compiler's benefit.
        throw new AggregateException("All fallback providers failed.", failures ?? []);
    }

    /// <summary>
    /// Streams a response, trying each inner client in order until one begins streaming successfully.
    /// Fallback only covers establishing the stream; once updates start flowing from one client, this
    /// method does not fall through on a later failure.
    /// </summary>
    /// <param name="messages">The conversation messages to send.</param>
    /// <param name="options">The chat options to apply, or <see langword="null" /> for defaults.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A stream of response updates from the first inner client that starts successfully.</returns>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) {
        for (var i = 0; i < _inner.Count; i++) {
            IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
            try {
                enumerator = _inner[i].Chat.GetStreamingResponseAsync(messages, options, cancellationToken).GetAsyncEnumerator(cancellationToken);
                // Prime the enumerator inside the try so a connection failure on the first item also triggers fallback.
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    yield break;
            } catch when (i < _inner.Count - 1) {
                if (enumerator is not null)
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                continue;
            }

            await using (enumerator) {
                yield return enumerator.Current;
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    yield return enumerator.Current;
            }
            yield break;
        }
    }

    /// <summary>
    /// Returns this instance when <paramref name="serviceType" /> matches it; otherwise delegates to the first inner client.
    /// </summary>
    /// <param name="serviceType">The service type being requested.</param>
    /// <param name="serviceKey">An optional key identifying the requested service, or <see langword="null" />.</param>
    /// <returns>The resolved service instance, or <see langword="null" /> if none matches.</returns>
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType.IsInstanceOfType(this) ? this : _inner[0].Chat.GetService(serviceType, serviceKey);

    /// <summary>
    /// Disposes every inner chat client.
    /// </summary>
    public void Dispose() {
        foreach (var (chat, _) in _inner)
            chat.Dispose();
    }
}

// ============================================================================
// Meridian
// File: ModelClientResolver.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Resolves the ModelClients bundle for a specific ModelProvider on demand, so a
// single caller can honor a per-call provider selection instead of being pinned
// to the configured default. Results are built once and cached for the lifetime
// of the (singleton) resolver.
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

using System.Collections.Concurrent;

namespace Meridian.Models.Providers;

/// <summary>
/// Resolves <see cref="ModelClients" /> for a specific <see cref="ModelProvider" /> on demand.
/// </summary>
public interface IModelClientResolver {
    /// <summary>
    /// Resolves the clients for <paramref name="provider" />. <see cref="ModelProvider.Auto" /> maps to the
    /// configured default provider.
    /// </summary>
    /// <param name="provider">The provider to resolve clients for.</param>
    /// <returns>The <see cref="ModelClients" /> bundle for <paramref name="provider" />.</returns>
    ModelClients Resolve(ModelProvider provider);
}

/// <summary>
/// Default <see cref="IModelClientResolver" /> implementation. Builds each provider's clients on first
/// request via <see cref="ModelClientBuilder.BuildFor" />, applies the configured chat pipeline, and
/// caches the result per provider for the lifetime of the (singleton) resolver.
/// </summary>
/// <param name="options">The provider options to resolve clients from.</param>
/// <param name="services">The service provider used to resolve pipeline dependencies (logging/cache), or <see langword="null" />.</param>
public sealed class ModelClientResolver(ModelOptions options, IServiceProvider? services = null) : IModelClientResolver {
    /// <summary>
    /// The options
    /// </summary>
    private readonly ModelOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    /// <summary>
    /// The cache
    /// </summary>
    private readonly ConcurrentDictionary<ModelProvider, ModelClients> _cache = new();

    /// <summary>
    /// Resolves the clients for <paramref name="provider" />. <see cref="ModelProvider.Auto" /> maps to the
    /// configured default provider.
    /// </summary>
    /// <param name="provider">The provider to resolve clients for.</param>
    /// <returns>The <see cref="ModelClients" /> bundle for <paramref name="provider" />.</returns>
    public ModelClients Resolve(ModelProvider provider) {
        var effective = provider == ModelProvider.Auto ? _options.DefaultProvider : provider;
        return _cache.GetOrAdd(effective, p => {
            var built = ModelClientBuilder.BuildFor(p, _options);
            var chat = ModelClientBuilder.ApplyChatPipeline(built.Chat, _options, services);
            return built with { Chat = chat };
        });
    }
}
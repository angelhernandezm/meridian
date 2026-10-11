// ============================================================================
// Meridian
// File: IMeridianAgentRegistry.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Resolves a named agent, built on first use from the MeridianAgentOptions
// registered via options.AddAgent(...) and cached thereafter. Lets application
// code ask for "the agent named X" without taking a dependency on how it was
// configured or which provider backs it.
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
using Microsoft.Agents.AI;

namespace Meridian.Agents.Agents;

/// <summary>Resolves named agents configured via <c>options.AddAgent(name, configure)</c>.</summary>
public interface IMeridianAgentRegistry {
    /// <summary>
    /// Resolves an agent by name, building it on first use.
    /// </summary>
    /// <param name="name">The agent name to look up.</param>
    /// <returns>The built <see cref="AIAgent"/>.</returns>
    /// <exception cref="KeyNotFoundException">No agent is registered under <paramref name="name"/>.</exception>
    AIAgent Get(string name);

    /// <summary>
    /// Attempts to resolve an agent by name, building it on first use.
    /// </summary>
    /// <param name="name">The agent name to look up.</param>
    /// <param name="agent">When this method returns <see langword="true"/>, the resolved agent.</param>
    /// <returns><see langword="true"/> if an agent named <paramref name="name"/> is registered; otherwise <see langword="false"/>.</returns>
    bool TryGet(string name, out AIAgent agent);

    /// <summary>Gets the names of every registered agent.</summary>
    /// <value>A snapshot of the registered agent names.</value>
    IReadOnlyCollection<string> Names { get; }
}

/// <summary>
/// Default <see cref="IMeridianAgentRegistry"/> implementation. Builds each agent on first request via
/// <paramref name="factory"/> and caches the result for the lifetime of the (singleton) registry.
/// </summary>
/// <param name="factory">The factory used to build each agent.</param>
/// <param name="optionsByName">The agent configurations, keyed by name.</param>
public sealed class MeridianAgentRegistry(MeridianAgentFactory factory, IReadOnlyDictionary<string, MeridianAgentOptions> optionsByName) : IMeridianAgentRegistry {
    /// <summary>
    /// The factory
    /// </summary>
    private readonly MeridianAgentFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    /// <summary>
    /// The options by name
    /// </summary>
    private readonly IReadOnlyDictionary<string, MeridianAgentOptions> _optionsByName = optionsByName ?? throw new ArgumentNullException(nameof(optionsByName));
    /// <summary>
    /// The cache
    /// </summary>
    private readonly ConcurrentDictionary<string, AIAgent> _cache = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Gets the names of every registered agent.
    /// </summary>
    /// <value>A snapshot of the registered agent names.</value>
    public IReadOnlyCollection<string> Names => _optionsByName.Keys.ToArray();


    /// <summary>
    /// Resolves an agent by name, building it on first use.
    /// </summary>
    /// <param name="name">The agent name to look up.</param>
    /// <returns>The built <see cref="AIAgent" />.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">No agent named \"{name}\" was registered. Register it with options.AddAgent(\"{name}\", ...).</exception>
    public AIAgent Get(string name) =>
        TryGet(name, out var agent) ? agent : throw new KeyNotFoundException($"No agent named \"{name}\" was registered. Register it with options.AddAgent(\"{name}\", ...).");


    /// <summary>
    /// Attempts to resolve an agent by name, building it on first use.
    /// </summary>
    /// <param name="name">The agent name to look up.</param>
    /// <param name="agent">When this method returns <see langword="true" />, the resolved agent.</param>
    /// <returns><see langword="true" /> if an agent named <paramref name="name" /> is registered; otherwise <see langword="false" />.</returns>
    public bool TryGet(string name, out AIAgent agent) {
        if (_cache.TryGetValue(name, out agent!))
            return true;

        if (!_optionsByName.TryGetValue(name, out var options)) {
            agent = null!;
            return false;
        }

        agent = _cache.GetOrAdd(name, _ => _factory.Create(options));
        return true;
    }
}

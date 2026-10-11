// ============================================================================
// Meridian
// File: IMeridianWorkflowRegistry.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Resolves a named MeridianWorkflow, built on first use from the factory
// registered via options.AddWorkflow(...) and cached thereafter.
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

namespace Meridian.Agents.Workflows;

/// <summary>
/// Resolves named workflows configured via <c>options.AddWorkflow(name, factory)</c>.
/// </summary>
public interface IMeridianWorkflowRegistry {
    /// <summary>
    /// Resolves a workflow by name, building it on first use.
    /// </summary>
    /// <param name="name">The workflow name to look up.</param>
    /// <returns>The built <see cref="MeridianWorkflow"/>.</returns>
    /// <exception cref="KeyNotFoundException">No workflow is registered under <paramref name="name"/>.</exception>
    MeridianWorkflow Get(string name);

    /// <summary>
    /// Attempts to resolve a workflow by name, building it on first use.
    /// </summary>
    /// <param name="name">The workflow name to look up.</param>
    /// <param name="workflow">When this method returns <see langword="true" />, the resolved workflow.</param>
    /// <returns><see langword="true" /> if a workflow named <paramref name="name" /> is registered; otherwise <see langword="false" />.</returns>
    bool TryGet(string name, out MeridianWorkflow workflow);

    /// <summary>
    /// Gets the names of every registered workflow.
    /// </summary>
    /// <value>A snapshot of the registered workflow names.</value>
    IReadOnlyCollection<string> Names { get; }
}

/// <summary>
/// Default <see cref="IMeridianWorkflowRegistry" /> implementation. Builds each workflow on first request
/// via its registered factory and caches the result for the lifetime of the (singleton) registry.
/// </summary>
/// <param name="services">The service provider passed to each workflow factory.</param>
/// <param name="factoriesByName">The workflow factories, keyed by name.</param>
public sealed class MeridianWorkflowRegistry(IServiceProvider services, IReadOnlyDictionary<string, Func<IServiceProvider, MeridianWorkflow>> factoriesByName) : IMeridianWorkflowRegistry {
    /// <summary>
    /// The services
    /// </summary>
    private readonly IServiceProvider _services = services ?? throw new ArgumentNullException(nameof(services));
    /// <summary>
    /// The factories by name
    /// </summary>
    private readonly IReadOnlyDictionary<string, Func<IServiceProvider, MeridianWorkflow>> _factoriesByName = factoriesByName ?? throw new ArgumentNullException(nameof(factoriesByName));
    /// <summary>
    /// The cache
    /// </summary>
    private readonly ConcurrentDictionary<string, MeridianWorkflow> _cache = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Gets the names of every registered workflow.
    /// </summary>
    /// <value>A snapshot of the registered workflow names.</value>
    public IReadOnlyCollection<string> Names => _factoriesByName.Keys.ToArray();


    /// <summary>
    /// Resolves a workflow by name, building it on first use.
    /// </summary>
    /// <param name="name">The workflow name to look up.</param>
    /// <returns>The built <see cref="MeridianWorkflow" />.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">No workflow named \"{name}\" was registered. Register it with options.AddWorkflow(\"{name}\", ...).</exception>
    public MeridianWorkflow Get(string name) =>
        TryGet(name, out var workflow) ? workflow : throw new KeyNotFoundException($"No workflow named \"{name}\" was registered. Register it with options.AddWorkflow(\"{name}\", ...).");


    /// <summary>
    /// Attempts to resolve a workflow by name, building it on first use.
    /// </summary>
    /// <param name="name">The workflow name to look up.</param>
    /// <param name="workflow">When this method returns <see langword="true" />, the resolved workflow.</param>
    /// <returns><see langword="true" /> if a workflow named <paramref name="name" /> is registered; otherwise <see langword="false" />.</returns>
    public bool TryGet(string name, out MeridianWorkflow workflow) {
        if (_cache.TryGetValue(name, out workflow!))
            return true;

        if (!_factoriesByName.TryGetValue(name, out var factory)) {
            workflow = null!;
            return false;
        }

        workflow = _cache.GetOrAdd(name, _ => factory(_services));
        return true;
    }
}
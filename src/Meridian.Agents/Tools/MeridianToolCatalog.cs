// ============================================================================
// Meridian
// File: MeridianToolCatalog.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A named registry of AITool instances populated via options.AddTool(...) at
// startup, so MeridianAgentOptions can reference tools by name instead of every
// call site rebuilding the same AIFunction.
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
using Microsoft.Extensions.AI;

namespace Meridian.Agents.Tools;

/// <summary>
/// A named registry of <see cref="AITool" /> instances available to agents.
/// </summary>
public interface IMeridianToolCatalog {
    /// <summary>
    /// Registers (or replaces) a tool under <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name to register the tool under.</param>
    /// <param name="tool">The tool to register.</param>
    void Register(string name, AITool tool);

    /// <summary>
    /// Attempts to resolve a tool by name.
    /// </summary>
    /// <param name="name">The tool name to look up.</param>
    /// <param name="tool">When this method returns <see langword="true" />, the resolved tool.</param>
    /// <returns><see langword="true" /> if a tool named <paramref name="name" /> is registered; otherwise <see langword="false" />.</returns>
    bool TryGet(string name, out AITool tool);

    /// <summary>
    /// Gets every registered tool.
    /// </summary>
    /// <value>A snapshot of all registered tools.</value>
    IReadOnlyCollection<AITool> All { get; }
}

/// <summary>
/// Class MeridianToolCatalog. This class cannot be inherited.
/// Implements the <see cref="Meridian.Agents.Tools.IMeridianToolCatalog" />
/// </summary>
/// <seealso cref="Meridian.Agents.Tools.IMeridianToolCatalog" />
/// <inheritdoc cref="IMeridianToolCatalog" />
public sealed class MeridianToolCatalog : IMeridianToolCatalog {
    /// <summary>
    /// The tools
    /// </summary>
    private readonly ConcurrentDictionary<string, AITool> _tools = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Registers (or replaces) a tool under <paramref name="name" />.
    /// </summary>
    /// <param name="name">The name to register the tool under.</param>
    /// <param name="tool">The tool to register.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public void Register(string name, AITool tool) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(tool);
        _tools[name] = tool;
    }
    
    /// <summary>
    /// Attempts to resolve a tool by name.
    /// </summary>
    /// <param name="name">The tool name to look up.</param>
    /// <param name="tool">When this method returns <see langword="true" />, the resolved tool.</param>
    /// <returns><see langword="true" /> if a tool named <paramref name="name" /> is registered; otherwise <see langword="false" />.</returns>
    public bool TryGet(string name, out AITool tool) => _tools.TryGetValue(name, out tool!);


    /// <summary>
    /// Gets every registered tool.
    /// </summary>
    /// <value>A snapshot of all registered tools.</value>
    public IReadOnlyCollection<AITool> All => _tools.Values.ToArray();
}
// ============================================================================
// Meridian
// File: MeridianToolBuilder.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Thin convenience wrapper over Microsoft.Extensions.AI's AIFunctionFactory so
// consumers can turn a plain delegate into an agent-callable AIFunction without
// importing Microsoft.Extensions.AI directly in application code.
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

namespace Meridian.Agents.Tools;

/// <summary>
/// Builds agent-callable <see cref="AIFunction" /> tools from plain .NET delegates.
/// </summary>
public static class MeridianToolBuilder {
    /// <summary>
    /// Wraps <paramref name="method"/> as an <see cref="AIFunction"/>. Parameter and return types are
    /// reflected into a JSON schema automatically; use <paramref name="name"/>/<paramref name="description"/>
    /// to give the model a clearer tool-selection signal than the method's own name/XML doc would.
    /// </summary>
    /// <param name="method">The delegate to wrap.</param>
    /// <param name="name">The tool name surfaced to the model, or <see langword="null"/> to use the method's own name.</param>
    /// <param name="description">The tool description surfaced to the model, or <see langword="null"/> to use the method's XML doc summary, if any.</param>
    /// <returns>An <see cref="AIFunction"/> wrapping <paramref name="method"/>.</returns>
    public static AIFunction FromDelegate(Delegate method, string? name = null, string? description = null) {
        ArgumentNullException.ThrowIfNull(method);
        return AIFunctionFactory.Create(method, name, description);
    }
}
// ============================================================================
// Meridian
// File: MeridianTelemetry.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The well-known ActivitySource name Meridian's Models/Agents pipelines pass to
// Microsoft.Extensions.AI's and Microsoft.Agents.AI's UseOpenTelemetry() stages,
// so every subsystem's spans land under one source a host can subscribe to once.
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

using System.Diagnostics;

namespace Meridian.Observability.Tracing;

/// <summary>
/// Well-known tracing identifiers shared across Meridian subsystems.
/// </summary>
public static class MeridianTelemetry {
    /// <summary>
    /// The name registered with OpenTelemetry's <c>TracerProviderBuilder.AddSource(...)</c> to see Meridian spans.
    /// </summary>
    /// <value>The constant <c>"Meridian"</c>.</value>
    public const string SourceName = "Meridian";

    /// <summary>
    /// A shared <see cref="ActivitySource" /> for spans Meridian itself emits outside the model/agent pipelines.
    /// </summary>
    /// <value>An <see cref="ActivitySource" /> named <see cref="SourceName" />.</value>
    public static readonly ActivitySource ActivitySource = new(SourceName, typeof(MeridianTelemetry).Assembly.GetName().Version?.ToString() ?? "1.0.0");
}

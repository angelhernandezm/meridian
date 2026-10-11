// ============================================================================
// Meridian
// File: MeridianMetrics.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// A shared System.Diagnostics.Metrics.Meter plus the counters/histograms Models,
// Agents and Knowledge record against (requests, tokens, latency, failures,
// retrieval hits), so a host wires one OpenTelemetry metrics exporter and gets
// domain-shaped data for every subsystem, not just generic request counts.
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

using System.Diagnostics.Metrics;

namespace Meridian.Observability.Metrics;

/// <summary>
/// Shared Meridian metrics instruments. Register <see cref="MeterName" /> with your OpenTelemetry MeterProvider.
/// </summary>
public static class MeridianMetrics {
    /// <summary>
    /// The name registered with OpenTelemetry's <c>MeterProviderBuilder.AddMeter(...)</c> to see Meridian metrics.
    /// </summary>
    /// <value>The constant <c>"Meridian"</c>.</value>
    public const string MeterName = "Meridian";

    /// <summary>
    /// The meter
    /// </summary>
    private static readonly Meter Meter = new(MeterName, typeof(MeridianMetrics).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>
    /// Count of model/agent requests, tagged with <c>provider</c> and <c>outcome</c> (success/failure).
    /// </summary>
    /// <value>A counter instrument named <c>meridian.requests</c>.</value>
    public static readonly Counter<long> Requests = Meter.CreateCounter<long>("meridian.requests", unit: "{request}");

    /// <summary>
    /// Total prompt + completion tokens consumed, tagged with <c>provider</c> and <c>kind</c> (prompt/completion).
    /// </summary>
    /// <value>A counter instrument named <c>meridian.tokens</c>.</value>
    public static readonly Counter<long> Tokens = Meter.CreateCounter<long>("meridian.tokens", unit: "{token}");

    /// <summary>
    /// End-to-end operation latency, tagged with <c>operation</c> (e.g. <c>chat</c>, <c>retrieval.semantic</c>, <c>retrieval.hybrid</c>).
    /// </summary>
    /// <value>A histogram instrument named <c>meridian.latency</c>, in milliseconds.</value>
    public static readonly Histogram<double> Latency = Meter.CreateHistogram<double>("meridian.latency", unit: "ms");

    /// <summary>
    /// Count of passages returned per retrieval call, tagged with <c>kind</c> (semantic/hybrid).
    /// </summary>
    /// <value>A histogram instrument named <c>meridian.retrieval.hits</c>.</value>
    public static readonly Histogram<long> RetrievalHits = Meter.CreateHistogram<long>("meridian.retrieval.hits", unit: "{passage}");
}
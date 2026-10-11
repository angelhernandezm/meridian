// ============================================================================
// Meridian
// File: ObservabilityOptions.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Configuration for options.AddObservability(...): the response evaluators run
// after an agent/model call. Tracing and metrics need no registration here —
// they're the always-on MeridianTelemetry ActivitySource/Meter, enabled per
// call via MeridianAgentOptions.EnableTracing / ModelOptions.Pipeline.
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

using Meridian.Observability.Evaluation;

namespace Meridian.Hosting;

/// <summary>
/// Configures Meridian's evaluation stage.
/// </summary>
public sealed class ObservabilityOptions {
    /// <summary>
    /// Gets the evaluators registered for application code to run against agent/model responses.
    /// </summary>
    /// <value>The registered evaluators. Empty by default.</value>
    public IList<ITextEvaluator> Evaluators { get; } = new List<ITextEvaluator>();
}

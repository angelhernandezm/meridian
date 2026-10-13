# Meridian.Observability

> [!NOTE]
> Independent open-source project; not affiliated with or endorsed by Microsoft.

Domain-shaped tracing, metrics, and evaluation for .NET AI applications — one `ActivitySource`
and one `Meter` shared across every Meridian subsystem, so a host wires one OpenTelemetry exporter
and gets request counts, token usage, latency, and retrieval hit counts out of the box, not just
generic HTTP metrics.

Part of [**Meridian**](https://github.com/angelhernandezm/meridian) — an opinionated .NET 10 AI
framework built on the Microsoft Agent Framework, reachable from modern .NET apps directly and
from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a COM Callable Wrapper.

## Install

```bash
dotnet add package Meridian.Observability
```

## Example

```csharp
using Meridian.Observability.Tracing;
using Meridian.Observability.Metrics;

// Register once, wherever you set up OpenTelemetry:
services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource(MeridianTelemetry.SourceName))
    .WithMetrics(m => m.AddMeter(MeridianMetrics.MeterName));

// Meridian.Models and Meridian.Agents emit spans/metrics against these automatically when
// ModelOptions.Pipeline.EnableOpenTelemetry / MeridianAgentOptions.EnableTracing are set --
// nothing further to call yourself.
```

```csharp
using Meridian.Observability.Evaluation;

public sealed class ContainsApologyEvaluator : ITextEvaluator {
    public string Name => "contains-apology";

    public Task<EvaluationResult> EvaluateAsync(string output, CancellationToken cancellationToken = default) {
        var passed = output.Contains("sorry", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(new EvaluationResult(Name, passed, passed ? 1 : 0));
    }
}
```

## What each piece does

| Type | Role |
|---|---|
| `MeridianTelemetry.SourceName` / `.ActivitySource` | The well-known `ActivitySource` name (`"Meridian"`) Models/Agents pass to `UseOpenTelemetry()`; register it once with `AddSource(...)` to see every subsystem's spans |
| `MeridianMetrics` | A shared `Meter` (`"Meridian"`) plus `Requests`, `Tokens`, `Latency`, and `RetrievalHits` instruments, tagged with provider/outcome/operation/kind |
| `IEvaluator<TInput, TOutput>` / `ITextEvaluator` | The evaluation abstraction — scores a model/agent output against a rubric, returning pass/fail plus a numeric score and explanation. Implement it for exact-match, keyword coverage, LLM-as-judge, or a third-party eval SDK |
| `KeywordPresenceEvaluator`, `TextLengthEvaluator` | Small built-in `ITextEvaluator`s, useful as examples or for simple checks |

## Full documentation

See the [Meridian repository](https://github.com/angelhernandezm/meridian) for the complete
framework — multi-provider models, hybrid RAG, agents and long-term memory, image generation, and
the COM bridge for VB6/Visual FoxPro/Delphi/VBA.

## License

MIT © Angel Hernandez

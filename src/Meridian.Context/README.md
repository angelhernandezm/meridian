# Meridian.Context

The ambient execution context shared across every Meridian package — a small, dependency-free
foundation package most apps won't reference directly (it comes in transitively via
[`Meridian.Hosting`](https://www.nuget.org/packages/Meridian.Hosting)).

Part of [**Meridian**](https://github.com/angelhernandezm/meridian) — an opinionated .NET 10 AI
framework built on the Microsoft Agent Framework, reachable from modern .NET apps directly and
from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a COM Callable Wrapper.

## What it provides

- **`MeridianContext`** — an immutable-by-convention record of a single logical operation (a chat
  turn, an agent run, a retrieval call): a `ConversationId`, a `CorrelationId` for stitching
  together logs/traces/metrics, optional `UserId`/`TenantId`, and an open-ended `Properties` bag
  for cross-cutting concerns that don't warrant a first-class field.
- **`IMeridianContextAccessor`** — an `AsyncLocal<T>`-backed ambient accessor for the current
  `MeridianContext`, mirroring ASP.NET Core's `IHttpContextAccessor` pattern. Deeply nested calls
  (tools, retrievers, model providers) can read the current context without it being threaded
  through every method signature, and it flows correctly across `async`/`await`.
- **`ConversationId`** — a strongly-typed wrapper around a conversation identifier, so call sites
  can't accidentally pass an arbitrary string where a conversation id is expected.

## Install

```bash
dotnet add package Meridian.Context
```

(Usually unnecessary directly — `Meridian.Hosting` and every other Meridian package already
depend on it.)

## Example

```csharp
using Meridian.Context;

services.AddSingleton<IMeridianContextAccessor, MeridianContextAccessor>();

// Somewhere at the start of a logical operation (a request, a queue message, ...):
contextAccessor.Current = MeridianContext.Create();
contextAccessor.Current.Properties["source"] = "scheduled-job";

// Anywhere deeper in the call chain, without the context being passed as a parameter:
var correlationId = contextAccessor.Current?.CorrelationId;
```

## Full documentation

See the [Meridian repository](https://github.com/angelhernandezm/meridian) for the complete
framework — multi-provider models, hybrid RAG, long-term agent memory, image generation, and the
COM bridge for VB6/Visual FoxPro/Delphi/VBA.

## License

MIT © Angel Hernandez

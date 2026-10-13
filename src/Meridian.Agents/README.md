# Meridian.Agents

Tools, long-term memory, sequential workflows, and named agent registration for .NET, built on
the [Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/) (MAF). Resolve an
agent by name without taking a dependency on how it's configured or which model provider backs
it.

Part of [**Meridian**](https://github.com/angelhernandezm/meridian) — an opinionated .NET 10 AI
framework built on the Microsoft Agent Framework, reachable from modern .NET apps directly and
from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a COM Callable Wrapper.

## Install

```bash
dotnet add package Meridian.Agents
```

## Example

```csharp
using Meridian.Agents.Agents;
using Meridian.Agents.Memory;

// Named agents, resolved later without knowing how they were built:
var agent = agentRegistry.Get("assistant");
var session = await agent.CreateSessionAsync();
var response = await agent.RunAsync("What's the weather in Seattle?", session);

// Long-term memory, independent of any single session's transcript:
await memory.RememberAsync("The customer prefers email over phone calls.");
IReadOnlyList<MemoryEntry> recalled = await memory.RecallAsync("How does the customer like to be contacted?");

// Sequential workflows mixing agent steps and plain transforms:
var workflow = new MeridianWorkflowBuilder()
    .AddAgentStep("research", researchAgent)
    .AddTransformStep("summarize", text => text.Length > 500 ? text[..500] + "..." : text)
    .Build();
WorkflowResult result = await workflow.RunAsync("Summarize recent news about .NET 10.");
```

Most applications won't construct these types directly — reference
[`Meridian.Hosting`](https://www.nuget.org/packages/Meridian.Hosting) and configure agents,
tools, and memory through `services.AddMeridian(options => { options.AddAgent(...); ... })`.

## What each piece does

| Type | Role |
|---|---|
| `IMeridianAgentRegistry` | Resolves a named agent (`Get`/`TryGet`), building it on first use from its registered `MeridianAgentOptions` and caching it thereafter |
| `MeridianAgentOptions` | Per-agent configuration: instructions, model provider, tool names, chat options, built-in knowledge-search tool, tracing/logging middleware |
| `IMeridianMemory` / `SemanticMemory` | Durable, semantically-searchable long-term memory — remember a fact, recall the most relevant facts for a query, forget by id. Distinct from a single chat session's transcript |
| `MeridianToolCatalog` | A process-wide registry of tools callable by name from `MeridianAgentOptions.ToolNames`, built from ordinary delegates via `MeridianToolBuilder.FromDelegate` |
| `MeridianWorkflow` / `MeridianWorkflowBuilder` | A fixed, ordered pipeline of `IWorkflowStep`s — agent steps and arbitrary sync/async transform steps — each step's output feeding the next, with a full per-step trace on completion |

## Full documentation

See the [Meridian repository](https://github.com/angelhernandezm/meridian) for the complete
framework — multi-provider models, hybrid RAG, image generation, and the COM bridge for
VB6/Visual FoxPro/Delphi/VBA.

## License

MIT © Angel Hernandez

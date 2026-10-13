# Meridian.Hosting

> [!NOTE]
> Independent open-source project; not affiliated with or endorsed by Microsoft.

The `services.AddMeridian(...)` front door — one fluent registration call that wires up
multi-provider models, embeddings/retrieval, agents/tools/memory/workflows, and observability.
Referencing this package alone pulls in the entire Meridian stack
([`Meridian.Context`](https://www.nuget.org/packages/Meridian.Context),
[`Meridian.Models`](https://www.nuget.org/packages/Meridian.Models),
[`Meridian.Knowledge`](https://www.nuget.org/packages/Meridian.Knowledge),
[`Meridian.Agents`](https://www.nuget.org/packages/Meridian.Agents),
[`Meridian.Observability`](https://www.nuget.org/packages/Meridian.Observability)) — this is the
package most applications should install.

**Meridian** is an opinionated .NET 10 AI framework built on the
[Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/) (MAF). It's a modern
.NET framework first — .NET applications reference it directly, no COM involved — and the same
engine is also reachable from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a
COM Callable Wrapper, so a legacy codebase doesn't have to stay frozen out of what's possible with
AI today. See the [repository](https://github.com/angelhernandezm/meridian) for the full story,
architecture diagrams, and the COM interop guide.

## Install

```bash
dotnet add package Meridian.Hosting
```

## Quick start

```csharp
using Meridian.Hosting;

services.AddMeridian(options => {
    options.AddModel(model => {
        model.DefaultProvider = ModelProvider.AzureOpenAI;
        model.AzureOpenAI.ResourceName = "my-resource";
        model.AzureOpenAI.ChatDeploymentName = "gpt-4o";
        model.AzureOpenAI.EmbeddingDeploymentName = "text-embedding-3-small";
        model.FallbackProviders.Add(ModelProvider.OpenAI);
        model.Pipeline.EnableOpenTelemetry = true;
        model.Pipeline.EnableRetry = true;
    });

    options.AddEmbeddings();
    options.AddSemanticStore();
    options.AddHybridRetrieval();      // vector + keyword fusion; use AddRetrieval() for pure semantic search
    options.AddRagContextBuilder();    // batteries-included retrieval -> numbered context -> prompt
    options.AddMemory();
    options.AddImages();               // generation, editing, vision analysis (AzureOpenAI / OpenAI)

    options.AddTool("getWeather", (string city) => $"It's sunny in {city}.");

    options.AddAgent("assistant", agent => {
        agent.Instructions = "You are a concise, helpful assistant.";
        agent.ToolNames.Add("getWeather");
        agent.EnableSearchTool = true;  // lets the agent search the knowledge base mid-conversation
        agent.EnableTracing = true;
    });
});
```

```csharp
var agent = serviceProvider.GetRequiredService<IMeridianAgentRegistry>().Get("assistant");
var session = await agent.CreateSessionAsync();
var response = await agent.RunAsync("What's the weather in Seattle?", session);
Console.WriteLine(response.Text);
```

## Feature surface

| Capability | How |
|---|---|
| Stateless chat | `IChatClient` (registered by `AddModel`) |
| Multi-turn chat with history | `IMeridianAgentRegistry` → `AIAgent` + session (MAF) |
| Embeddings | `IEmbeddingService` (`AddEmbeddings`) |
| Semantic (vector) search | `AddRetrieval()` → `IRetriever` |
| Hybrid search (vector + keyword, weighted fusion) | `AddHybridRetrieval()` → `IRetriever` |
| Batteries-included RAG (retrieval + context + prompt template + sources) | `AddRagContextBuilder()` → `RagContextBuilder` |
| Long-term agent memory (remember/recall/forget) | `AddMemory()` → `IMeridianMemory` |
| Image generation / editing / vision analysis | `AddImages()` → `IMeridianImageService` |
| Agent tools (custom + built-in knowledge search) | `AddTool(...)`, `MeridianAgentOptions.EnableSearchTool` |
| Sequential workflows (agent + transform steps) | `AddWorkflow(...)` → `MeridianWorkflowBuilder` |
| Tracing / metrics / evaluation | `Meridian.Observability` (`MeridianTelemetry`, `MeridianMetrics`, `IEvaluator`) |

Switch model providers — or add fallback providers tried in order — by changing `ModelOptions`
inside `AddModel(...)`; no other code in your application changes. Supported providers: Azure
OpenAI, OpenAI, Anthropic (Claude), Google Gemini, Perplexity, and Azure AI Inference (Foundry).

## Reaching it from COM (VB6, Visual FoxPro, Delphi, VBA)

The same engine configured via `AddMeridian(...)` is exposed through `Meridian.Interop.Com`'s five
COM-creatable classes — `Meridian.ChatService`, `Meridian.AgentService`,
`Meridian.EmbeddingService`, `Meridian.MemoryService`, `Meridian.ImageService` — callable from any
COM host in a few lines:

```vb
Dim chat As Object
Set chat = CreateObject("Meridian.ChatService")
Debug.Print chat.Chat("What's 2 + 2?")
```

`Meridian.Interop.Com` isn't published to NuGet (it's a RID-specific `dotnet publish` artifact,
not a library you reference) — see
[`COM-INTEROP.md`](https://github.com/angelhernandezm/meridian/blob/master/COM-INTEROP.md) in the
repository for the full registration and calling guide.

## License

MIT © Angel Hernandez

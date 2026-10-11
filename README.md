# Meridian

**Modern AI for Classic and Modern Applications.**

Meridian is an open-source framework that brings agents, memory, tools, and large language models to COM-compatible applications — including old but not forgotten tools such as Visual Basic 6, Visual FoxPro, VBA, classic ASP, .NET, and modern platforms.

*Dedicated to my wife Mery, and to the developer community that continues to build, learn, and share across generations of technology.*

---

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Meridian.Hosting?style=flat&logo=nuget)](https://www.nuget.org/packages/Meridian.Hosting)
[![Tests](https://img.shields.io/badge/tests-32%20passing-brightgreen?style=flat)](tests/Meridian.Tests)

An opinionated .NET 10 agent framework built on the [Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/) (MAF). Meridian wires up multi-provider models, embeddings/retrieval, agents/tools/memory/workflows, and observability behind one fluent, ASP.NET-style registration call — then exposes the same functionality to legacy COM clients (VB6, Visual FoxPro, Delphi, VBA) through a CCW.

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

## Supported model providers

Meridian.Models gives every provider the same `IChatClient` / `IEmbeddingGenerator` / `IImageGenerator` surface via Microsoft.Extensions.AI, selected through one strongly-typed `ModelOptions` object — application code never branches on which LLM is behind it.

| Provider | Chat | Embeddings | Images | Notes |
|---|---|---|---|---|
| Azure OpenAI | ✅ | ✅ | ✅ | API key or Azure AD (`UseAzureAD`) |
| OpenAI | ✅ | ✅ | ✅ | |
| Anthropic (Claude) | ✅ | — | — | OpenAI-compatible endpoint; pair with another provider for embeddings |
| Google Gemini | ✅ | ✅ | — | OpenAI-compatible endpoint |
| Perplexity (`sonar`) | ✅ | — | — | OpenAI-compatible endpoint |
| Azure AI Inference (Foundry) | ✅ | ✅ | — | |

Switch providers (or add fallback providers tried in order) by changing `ModelOptions` — no code elsewhere in the app changes.

## Feature surface

| Capability | How |
|---|---|
| Stateless chat | `IChatClient` (registered by `AddModel`) |
| Multi-turn chat with history | `IMeridianAgentRegistry` → `AIAgent` + `AgentSession` (MAF) |
| Embeddings | `IEmbeddingService` (`AddEmbeddings`) |
| Semantic (vector) search | `AddRetrieval()` → `IRetriever` |
| Hybrid search (vector + keyword, weighted fusion) | `AddHybridRetrieval()` → `IRetriever` |
| Batteries-included RAG (retrieval + context + prompt template + sources) | `AddRagContextBuilder()` → `RagContextBuilder` |
| Long-term agent memory (remember/recall/forget) | `AddMemory()` → `IMeridianMemory` |
| Image generation | `AddImages()` → `IMeridianImageService.GenerateAsync` |
| Image editing | `AddImages()` → `IMeridianImageService.EditAsync` |
| Image analysis (vision) | `AddImages()` → `IMeridianImageService.AnalyzeAsync` |
| Agent tools (custom + built-in knowledge search) | `AddTool(...)`, `MeridianAgentOptions.EnableSearchTool` |
| Sequential workflows (agent + transform steps) | `AddWorkflow(...)` → `MeridianWorkflowBuilder` |
| Tracing / metrics / evaluation | `Meridian.Observability` (`MeridianTelemetry`, `MeridianMetrics`, `IEvaluator`) |

## Project layout

```
src/
  Meridian.Context/        Ambient execution context (ConversationId, correlation, property bag)
  Meridian.Models/          Providers: multi-provider chat/embedding/image clients, fallback, retry, pipeline
  Meridian.Knowledge/        Embeddings, SemanticStore (pluggable), Retrieval (semantic + hybrid RAG)
  Meridian.Agents/           Tools, Memory, Workflows, Agents — built on Microsoft.Agents.AI (MAF)
  Meridian.Observability/    Tracing, Metrics, Evaluation
  Meridian.Hosting/          services.AddMeridian(...) — the front door
  Meridian.Interop.Com/      COM Callable Wrapper for VB6 / Visual FoxPro / Delphi / VBA — see COM-INTEROP.md
tests/
  Meridian.Tests/            xUnit tests for the pure-logic components (scoring, fusion, retry/fallback, context assembly)
```

## Requirements

- .NET 10 SDK
- Microsoft.Agents.AI 1.0.0 (Microsoft Agent Framework)

## Building, testing, and packaging

```bash
dotnet build Meridian.slnx
dotnet test tests/Meridian.Tests/Meridian.Tests.csproj
dotnet pack Meridian.slnx -c Release   # produces artifacts/packages/Meridian.*.nupkg (+ .snupkg)
```

Each library project (`Meridian.Context`, `.Models`, `.Knowledge`, `.Agents`, `.Observability`, `.Hosting`) packs as its own NuGet package, with inter-package dependencies resolved automatically — `Meridian.Hosting` depends on the others, so referencing it alone pulls in the full stack. `Meridian.Interop.Com` is not packed (it's published per-RID via `dotnet publish`, not referenced as a library) and `Meridian.Tests` is never packed.

> `Microsoft.Extensions.AI.AzureAIInference` is still a Microsoft preview package upstream (not yet GA), so `dotnet pack` emits an NU5104 warning about a stable package depending on a prerelease one. This is expected until Microsoft ships a stable release of that package — it doesn't affect functionality.

## COM interop

See [COM-INTEROP.md](COM-INTEROP.md) for registering `Meridian.Interop.Com` and calling it from VB6, Visual FoxPro, Delphi, or VBA — chat, embeddings, memory, multi-turn agent sessions, and image generation/editing/analysis are all exposed.

## License

MIT — see [LICENSE](LICENSE).

## 👨‍💻 Author

**Angel Hernandez**
📧 me@angelhernandezm.com
🐙 [github.com/angelhernandezm](https://github.com/angelhernandezm)

---

<p align="center"><strong>⭐ Star this repo if Meridian is useful to you — it helps others find it too.</strong></p>
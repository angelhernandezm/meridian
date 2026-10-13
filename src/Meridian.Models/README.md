# Meridian.Models

Multi-provider chat, embedding, and image model clients for .NET — one strongly-typed
`ModelOptions` configuration, the same `IChatClient`/`IEmbeddingGenerator`/`IImageGenerator`
surface from [Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI)
regardless of which provider is behind it, with opt-in fallback, retry, and OpenTelemetry.

Part of [**Meridian**](https://github.com/angelhernandezm/meridian) — an opinionated .NET 10 AI
framework built on the Microsoft Agent Framework, reachable from modern .NET apps directly and
from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a COM Callable Wrapper.

## Supported providers

| Provider | Chat | Embeddings | Images | Notes |
|---|---|---|---|---|
| Azure OpenAI | ✅ | ✅ | ✅ | API key or Azure AD (`UseAzureAD`) |
| OpenAI | ✅ | ✅ | ✅ | |
| Anthropic (Claude) | ✅ | — | — | OpenAI-compatible endpoint; pair with another provider for embeddings |
| Google Gemini | ✅ | ✅ | — | OpenAI-compatible endpoint |
| Perplexity (`sonar`) | ✅ | — | — | OpenAI-compatible endpoint |
| Azure AI Inference (Foundry) | ✅ | ✅ | — | |

Switch providers — or add an ordered list of fallback providers — by changing `ModelOptions`; no
code elsewhere in your application changes.

## Install

```bash
dotnet add package Meridian.Models
```

## Example

```csharp
using Meridian.Models.Providers;

services.AddSingleton(new ModelOptions {
    DefaultProvider = ModelProvider.OpenAI,
    FallbackProviders = { ModelProvider.Anthropic },
    OpenAI = { ApiKey = "sk-..." },
    Anthropic = { ApiKey = "sk-ant-..." },
    Pipeline = { EnableRetry = true, EnableOpenTelemetry = true }
});

services.AddSingleton<IChatClient>(sp => {
    var options = sp.GetRequiredService<ModelOptions>();
    var (chat, _) = ModelClientBuilder.BuildChatWithFallback(options);
    return ModelClientBuilder.ApplyChatPipeline(chat, options, sp);
});
```

Most applications won't call `ModelClientBuilder` directly — reference
[`Meridian.Hosting`](https://www.nuget.org/packages/Meridian.Hosting) instead, and configure
everything through `services.AddMeridian(options => options.AddModel(...))`, which wires this up
for you alongside embeddings, retrieval, agents, and memory.

## What each piece does

| Type | Role |
|---|---|
| `ModelOptions` | Per-provider connection/model settings, default + fallback provider selection, opt-in pipeline (logging, caching, retry, OpenTelemetry) |
| `ModelClientBuilder` | Builds the `IChatClient` for the configured default provider, with automatic fallback through `FallbackProviders` in order |
| `IModelClientResolver` / `ModelClientResolver` | Resolves a specific named provider's clients on demand (used internally by embeddings/images/agents to pick a provider independent of the chat default) |
| `FallbackChatClient` | The `IChatClient` decorator that implements the try-next-provider-on-failure behavior |
| `RetryChatClient` | The `IChatClient` decorator implementing `Pipeline.EnableRetry`'s exponential backoff |

## Full documentation

See the [Meridian repository](https://github.com/angelhernandezm/meridian) for the complete
framework — hybrid RAG, long-term agent memory, image generation, and the COM bridge for
VB6/Visual FoxPro/Delphi/VBA.

## License

MIT © Angel Hernandez

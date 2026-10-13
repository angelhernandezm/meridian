# Meridian.Knowledge

> [!NOTE]
> Independent open-source project; not affiliated with or endorsed by Microsoft.

Embeddings, a pluggable vector store, and retrieval-augmented generation for .NET — semantic
search, hybrid (vector + keyword) search, and a batteries-included RAG pipeline that turns a
query straight into a ready-to-send, cited prompt.

Part of [**Meridian**](https://github.com/angelhernandezm/meridian) — an opinionated .NET 10 AI
framework built on the Microsoft Agent Framework, reachable from modern .NET apps directly and
from COM-based applications (VB6, Visual FoxPro, Delphi, VBA) through a COM Callable Wrapper.

## Install

```bash
dotnet add package Meridian.Knowledge
```

Requires an `IEmbeddingService` (ships with this package) backed by an embedding-capable provider
— see [`Meridian.Models`](https://www.nuget.org/packages/Meridian.Models).

## Example

```csharp
using Meridian.Knowledge.Embeddings;
using Meridian.Knowledge.Retrieval;
using Meridian.Knowledge.SemanticStore;

// Ingest
var store = new InMemorySemanticStore();
var vector = await embeddingService.EmbedAsync("The customer prefers email over phone calls.");
await store.UpsertAsync(new SemanticRecord("fact-1", vector, "The customer prefers email over phone calls."));

// Hybrid retrieval: vector similarity fused with keyword overlap
var retriever = new HybridRetriever(embeddingService, store, semanticWeight: 0.6, keywordWeight: 0.4);
IReadOnlyList<RetrievedPassage> hits = await retriever.RetrieveAsync("How does the customer like to be contacted?", topK: 5);

// Batteries-included RAG: retrieval -> numbered context -> ready-to-send prompt
var ragBuilder = new RagContextBuilder(retriever);
RagResult result = await ragBuilder.BuildAsync("How should we contact this customer?");
var response = await chatClient.GetResponseAsync(result.Prompt);
```

## What each piece does

| Type | Role |
|---|---|
| `IEmbeddingService` | Turns text into vectors via the configured `Meridian.Models` provider; batch (`EmbedManyAsync`) and single (`EmbedAsync`) overloads |
| `ISemanticStore` | The pluggable vector-store abstraction — upsert, vector search, delete. `InMemorySemanticStore` ships as the default; implement the interface to back it with Azure AI Search, Qdrant, Postgres/pgvector, or anything else |
| `IRetriever` | Retrieves the passages most relevant to a natural-language query. Two implementations: `SemanticRetriever` (pure vector similarity) and `HybridRetriever` (vector + keyword, weighted fusion) |
| `RagContextBuilder` | Retrieves, assembles a numbered context block, and templates a ready-to-send prompt — with its sources and a rough token-count estimate attached to the result |

Swap `SemanticRetriever` for `HybridRetriever` (or vice versa) without touching any calling code —
both satisfy `IRetriever`.

## Full documentation

See the [Meridian repository](https://github.com/angelhernandezm/meridian) for the complete
framework — multi-provider models, agents, long-term memory, image generation, and the COM bridge
for VB6/Visual FoxPro/Delphi/VBA.

## License

MIT © Angel Hernandez

# Meridian.Harness

A cross-platform (Windows/macOS/Linux) test harness for every Meridian capability, built with
[Avalonia](https://avaloniaui.net/). It talks to `Meridian.Hosting` directly, the same way any
modern .NET application does -- **no COM is involved here**. For a demo of the COM bridge itself
(VB6/VFP/Delphi/VBA-style access via `Meridian.Interop.Com`), see the Windows-only COM demo app
alongside this one and [`COM-INTEROP.md`](../../COM-INTEROP.md) at the repo root.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- API keys for whichever provider(s) you want to test (see below)

No Node, no Docker, no database -- it's a single desktop app.

## Quick start

```bash
# From the repo root
dotnet run --project samples/Meridian.Harness
```

The window opens on the **Setup** tab. Nothing else works until you configure and initialize:

1. Pick a **Default provider** and expand its credentials panel to enter an API key (or leave it
   blank to use the matching environment variable -- see below).
2. Leave the **Knowledge** checkboxes at their defaults (Embeddings + Hybrid retrieval + RAG +
   Memory + Images all on) to exercise every tab, or narrow them down to just what you're testing.
3. Click **Initialize Meridian**. The status line confirms success or explains what's missing.
4. Switch to any other tab -- **Chat**, **Agent**, **Embeddings**, **Knowledge / RAG**, **Memory**,
   **Images** -- and try it. Each tab shows a reminder if you haven't initialized yet.

Re-running **Initialize Meridian** rebuilds everything from scratch (including a fresh agent
session), so it's safe to change settings and re-initialize as often as you like.

## Environment variables (optional)

Each provider's **API key** field falls back to an environment variable when left blank in the UI
-- the same variables `Meridian.Models` and the COM host read:

| Variable | Provider |
|---|---|
| `MERIDIAN_OPENAI_API_KEY` | OpenAI |
| `MERIDIAN_ANTHROPIC_API_KEY` | Anthropic (Claude) |
| `MERIDIAN_GEMINI_API_KEY` | Gemini |
| `MERIDIAN_PERPLEXITY_API_KEY` | Perplexity |
| `MERIDIAN_AZURE_OPENAI_API_KEY` | Azure OpenAI (unused if "Use Azure AD" is checked) |
| `MERIDIAN_AZURE_AI_INFERENCE_API_KEY` | Azure AI Inference (Foundry) |

Model/deployment names (chat, embedding, image) have working defaults and only need to be typed
into the UI to override them -- there's no separate environment variable for those in the harness
(unlike the COM host, which reads non-secret selection variables too, since a COM caller can't run
C# code to set them -- see `COM-INTEROP.md`).

## Provider coverage notes

- **Anthropic and Perplexity have no embeddings or image endpoint.** If your default provider is
  one of these, also fill in OpenAI or Gemini's credentials -- Embeddings/Memory/RAG source from
  whichever provider you configure independently of the chat provider.
- **Image generation and editing only work on OpenAI or Azure OpenAI** (set an image model/
  deployment). Vision *analysis* (the Images tab's "Analyze" section) works with any
  vision-capable chat model.

## What each tab exercises

| Tab | Meridian surface |
|---|---|
| Setup | `services.AddMeridian(...)` end to end |
| Chat | `IChatClient` (stateless, single-turn) |
| Agent | `IMeridianAgentRegistry` + MAF `AIAgent`/`AgentSession` (multi-turn) |
| Embeddings | `IEmbeddingService` + cosine similarity |
| Knowledge / RAG | `ISemanticStore`, `IRetriever` (hybrid/semantic), `RagContextBuilder` |
| Memory | `IMeridianMemory` (remember/recall/forget) |
| Images | `IMeridianImageService` (generate/edit/analyze) |

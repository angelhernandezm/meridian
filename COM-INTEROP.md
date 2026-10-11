# Calling Meridian from COM (VB6, Visual FoxPro, Delphi, VBA)

`Meridian.Interop.Com` exposes five COM-creatable classes backed by Meridian's normal pipeline (models, embeddings, memory, agents, images). It uses .NET's built-in [COM hosting](https://learn.microsoft.com/dotnet/core/native-interop/expose-components-to-com) feature (`EnableComHosting`), not the classic .NET Framework `regasm`/`tlbexp` toolchain — the project still produces an ordinary in-proc COM server your existing VB6/VFP/Delphi/VBA code calls the same way.

| ProgId | Interface | Purpose |
|---|---|---|
| `Meridian.ChatService` | `IMeridianChatService` | Stateless, single-turn chat |
| `Meridian.EmbeddingService` | `IMeridianEmbeddingService` | Text → vector, cosine similarity |
| `Meridian.MemoryService` | `IMeridianMemoryService` | Long-term semantic memory (remember/recall/forget) |
| `Meridian.AgentService` | `IMeridianAgentService` | Multi-turn sessions with a named agent |
| `Meridian.ImageService` | `IMeridianImageService` | Image generation, editing, and vision analysis |

All five are dual interfaces (`IDispatch` + vtable), so both early binding (with IntelliSense, once you've added a reference) and late binding (`CreateObject`) work.

## 1. Configure, then publish

Because a COM caller cannot run the C# fluent builder, `Meridian.Interop.Com` configures itself entirely from environment variables (see the table below) the first time any of its classes is created. Set what you need, then publish:

```bash
# 32-bit — required for VB6 and Visual FoxPro 9, which are 32-bit only
dotnet publish src/Meridian.Interop.Com -r win-x86 --self-contained false -c Release

# 64-bit — for Delphi/VBA hosts that are themselves 64-bit (match your host's bitness)
dotnet publish src/Meridian.Interop.Com -r win-x64 --self-contained false -c Release
```

Each publish produces `Meridian.Interop.Com.comhost.dll` alongside `Meridian.Interop.Com.dll` and its dependencies in `bin/Release/net10.0/<rid>/publish/`.

> **Bitness matters.** A COM client can only load a server of its own bitness. VB6 and Visual FoxPro 9 are always 32-bit, so they need the `win-x86` publish. Delphi and VBA (Office) match whatever bitness the host process is — check which Office/Delphi build you have before registering.

## 2. Register the server

From an elevated (Administrator) command prompt, in the publish folder:

```bash
regsvr32 Meridian.Interop.Com.comhost.dll
```

To unregister later: `regsvr32 /u Meridian.Interop.Com.comhost.dll`.

Register the matching bitness in the matching registry hive: a 32-bit `regsvr32` (run from `%WINDIR%\SysWOW64\regsvr32.exe` on a 64-bit OS) registers under `HKCR\WOW6432Node` for 32-bit clients; the native 64-bit `regsvr32` registers for 64-bit clients. If you need both VB6/VFP (32-bit) and a 64-bit Delphi app to use Meridian side by side, register both publishes — they coexist under different registry hives.

## 3. Environment variables

Secrets (API keys) use the **same** variables as the rest of Meridian (`Meridian.Models`), so if you already have Meridian configured for a .NET host on the same machine, nothing extra is needed there:

| Variable | Purpose |
|---|---|
| `MERIDIAN_AZURE_OPENAI_API_KEY`, `MERIDIAN_OPENAI_API_KEY`, `MERIDIAN_ANTHROPIC_API_KEY`, `MERIDIAN_GEMINI_API_KEY`, `MERIDIAN_PERPLEXITY_API_KEY`, `MERIDIAN_AZURE_AI_INFERENCE_API_KEY` | API keys |

The COM host additionally reads these **non-secret** selection variables (set only the ones for your chosen provider):

| Variable | Example |
|---|---|
| `MERIDIAN_DEFAULT_PROVIDER` | `AzureOpenAI`, `OpenAI`, `Anthropic`, `Gemini`, `Perplexity`, or `AzureAIInference` |
| `MERIDIAN_AZURE_OPENAI_RESOURCE_NAME` | `my-resource` |
| `MERIDIAN_AZURE_OPENAI_CHAT_DEPLOYMENT` | `gpt-4o` |
| `MERIDIAN_AZURE_OPENAI_EMBEDDING_DEPLOYMENT` | `text-embedding-3-small` |
| `MERIDIAN_AZURE_OPENAI_IMAGE_DEPLOYMENT` | `gpt-image-1` (leave unset to disable `Meridian.ImageService` generation/editing for Azure OpenAI) |
| `MERIDIAN_AZURE_OPENAI_USE_AAD` | `true` / `false` |
| `MERIDIAN_OPENAI_CHAT_MODEL` / `MERIDIAN_OPENAI_EMBEDDING_MODEL` / `MERIDIAN_OPENAI_IMAGE_MODEL` | `gpt-4o-mini` / `text-embedding-3-small` / `gpt-image-1` |
| `MERIDIAN_ANTHROPIC_CHAT_MODEL` | `claude-sonnet-4-5` |
| `MERIDIAN_GEMINI_CHAT_MODEL` / `MERIDIAN_GEMINI_EMBEDDING_MODEL` | `gemini-2.5-flash` / `text-embedding-004` |
| `MERIDIAN_PERPLEXITY_CHAT_MODEL` | `sonar-pro` |
| `MERIDIAN_AZURE_AI_INFERENCE_ENDPOINT` / `_CHAT_MODEL` / `_EMBEDDING_MODEL` | |

Set machine-wide variables with `setx /M NAME value` from an elevated prompt, then **restart** the COM client application (environment variables are read when the process starts).

Named agents (for `Meridian.AgentService`) are configured in code, not environment variables — `Meridian.Interop.Com`'s `MeridianComHost` is the place to add `options.AddAgent("assistant", ...)` calls for the agents you want COM callers to reach by name.

## 4. Calling it

### VB6 (late binding)

```vb
Dim chat As Object
Set chat = CreateObject("Meridian.ChatService")
Debug.Print chat.Chat("What's 2 + 2?")
```

### VB6 / VBA (early binding, with IntelliSense)

Add a reference to the registered `Meridian.Interop.Com` type library (Project > References), then:

```vb
Dim chat As New MeridianChatService
MsgBox chat.ChatWithInstructions("Reply in one short sentence.", "Capital of France?")
```

### Visual FoxPro

```foxpro
loChat = CREATEOBJECT("Meridian.ChatService")
? loChat.Chat("Summarize FoxPro in one sentence.")

loMemory = CREATEOBJECT("Meridian.MemoryService")
lcId = loMemory.Remember("The customer prefers email over phone calls.")
loFacts = loMemory.Recall("How does the customer like to be contacted?", 3)
FOR i = 1 TO ALEN(loFacts)
    ? loFacts(i)
ENDFOR
```

### Delphi

```pascal
uses ComObj;

var
  Chat: OleVariant;
begin
  Chat := CreateOleObject('Meridian.ChatService');
  ShowMessage(Chat.Chat('What''s the weather like in general in October?'));
end;
```

### Multi-turn agent session (any host, shown in VB6)

```vb
Dim agentSvc As Object, sessionId As String
Set agentSvc = CreateObject("Meridian.AgentService")
sessionId = agentSvc.CreateSession("assistant")

Debug.Print agentSvc.Run(sessionId, "My name is Angel.")
Debug.Print agentSvc.Run(sessionId, "What's my name?")   ' remembers the first turn

agentSvc.CloseSession sessionId
```

### Image generation (any host, shown in VB6)

```vb
Dim imageSvc As Object, imgBytes() As Byte
Set imageSvc = CreateObject("Meridian.ImageService")

imgBytes = imageSvc.Generate("a lighthouse at sunset, watercolor style", 1024, 1024)

' Write the bytes to a file
Dim fileNum As Integer
fileNum = FreeFile
Open "lighthouse.png" For Binary Access Write As #fileNum
Put #fileNum, 1, imgBytes
Close #fileNum
```

`Meridian.ImageService.Generate`/`.Edit` only work when the default (or explicitly configured) provider supports image generation — currently AzureOpenAI (`MERIDIAN_AZURE_OPENAI_IMAGE_DEPLOYMENT`) and OpenAI (`MERIDIAN_OPENAI_IMAGE_MODEL`). `.Analyze` (vision) works with any vision-capable chat model. Calling `.Generate`/`.Edit` against a provider without image support (e.g. Anthropic, Perplexity) raises a COM error explaining which deployment/model to set.

## Error handling

A .NET exception thrown from any of these methods is translated by the CCW into a COM error (`HRESULT` + rich error description), which VB6/VFP/Delphi surface as a normal runtime error (`Err.Description` in VB6/VBA, the exception message in Delphi). Wrap calls in your host language's usual error handling (`On Error`, `TRY...CATCH`, Delphi `try/except`).

## Troubleshooting

- **"Class not registered"** — `regsvr32` wasn't run, or was run with the wrong bitness's `regsvr32.exe`.
- **"ActiveX component can't create object"** — the comhost's dependent DLLs aren't next to it (always register from inside the `publish/` folder, don't copy just the `.comhost.dll`), or the wrong Visual C++/.NET runtime is installed for a self-contained publish.
- **Changes to environment variables don't take effect** — restart the calling application; environment variables are captured at process start.

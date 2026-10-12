# Setting Up the Meridian COM Bridge on a VB6 / Visual FoxPro VM

A checklist for getting `Meridian.Interop.Com` registered and callable on a Windows VM that
already has VB6 and/or Visual FoxPro installed. For the calling-code examples (VB6, VFP, Delphi,
VBA) and the full environment-variable reference, see [`COM-INTEROP.md`](COM-INTEROP.md) at the
repo root — this file only covers getting the bridge *installed and registered*.

---

## 1. Prerequisites

| On... | Needs |
|---|---|
| **Your build machine** (where you clone/build the repo) | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **The VM** (VB6 / Visual FoxPro) | [.NET 10 **Runtime**](https://dotnet.microsoft.com/download/dotnet/10.0) only — not the SDK |
| **The VM** | Administrator rights (COM registration requires elevation) |
| **The VM** | API key(s) for whichever model provider you're testing |

Two important distinctions:

- **The VM doesn't need the SDK.** `Meridian.Interop.Com` is published as a self-contained-false
  comhost artifact — you *build* it once on a machine with the SDK, then *copy the published
  output* to the VM. The VM only needs the shared **.NET Runtime** installed so the copied DLLs
  have something to run on. (You do **not** need the "ASP.NET Core Runtime" or "Desktop Runtime"
  variants — `Meridian.Interop.Com` is a plain class library, not a web app or WinForms/WPF app.
  The plain "**.NET Runtime**" download is sufficient.)
- **Bitness matters and it's not optional.** VB6 and Visual FoxPro 9 are **always 32-bit**, full
  stop — there is no 64-bit VB6 or VFP9. A 32-bit host can only load a 32-bit (`x86`) COM server.
  If your VM also has a Delphi or Office (VBA) install you want to test, check whether *that*
  install is 32-bit or 64-bit before assuming — Delphi and Office both come in both bitnesses, and
  the COM server's bitness must match the calling process's bitness exactly.

## 2. Downloads

| What | Where | Which one |
|---|---|---|
| .NET 10 SDK (build machine only) | <https://dotnet.microsoft.com/download/dotnet/10.0> | "SDK" column, x64 (or your build machine's native arch) |
| .NET 10 Runtime (the VM) | <https://dotnet.microsoft.com/download/dotnet/10.0> | "Runtime" column → **x86** installer, for VB6/VFP9 |
| .NET 10 Runtime, 64-bit (only if also testing a 64-bit Delphi/VBA host on the same VM) | <https://dotnet.microsoft.com/download/dotnet/10.0> | "Runtime" column → **x64** installer |

The x86 and x64 runtimes install side by side without conflict, so if you're testing both a
32-bit (VB6/VFP) and a 64-bit (Delphi/VBA) host on the same VM, install both.

## 3. Build and publish the bridge (on your build machine, not the VM)

```bash
# From the repo root, with the .NET 10 SDK installed

# 32-bit -- required for VB6 and Visual FoxPro 9
dotnet publish src/Meridian.Interop.Com -r win-x86 --self-contained false -c Release

# 64-bit -- only if you'll also test a 64-bit Delphi/VBA host
dotnet publish src/Meridian.Interop.Com -r win-x64 --self-contained false -c Release
```

Each command produces a `publish/` folder containing `Meridian.Interop.Com.comhost.dll`,
`Meridian.Interop.Com.dll`, and their dependencies, at:

```
src/Meridian.Interop.Com/bin/Release/net10.0/win-x86/publish/
src/Meridian.Interop.Com/bin/Release/net10.0/win-x64/publish/
```

**Copy the entire `publish/` folder** to the VM (e.g. a shared folder, USB, or however your VM
tooling moves files) — not just the `.comhost.dll`. Registration will fail or load incorrectly if
the dependent DLLs aren't sitting right next to it.

## 4. Install the .NET Runtime on the VM

Run the installer from §2 matching your host's bitness (x86 for VB6/VFP9). Confirm it installed
with:

```
dotnet --list-runtimes
```

You should see a `Microsoft.NETCore.App 10.0.x` entry.

## 5. Set environment variables on the VM

The COM host configures itself entirely from environment variables (a VB6/VFP/Delphi/VBA caller
has no way to run the C# fluent builder). From an elevated Command Prompt on the VM:

```bat
setx /M MERIDIAN_DEFAULT_PROVIDER "OpenAI"
setx /M MERIDIAN_OPENAI_API_KEY "sk-..."
```

Swap in whichever provider/key(s) you're testing with — the full variable list (per-provider API
keys, chat/embedding/image model selection) is in [`COM-INTEROP.md`](COM-INTEROP.md#3-environment-variables).
`/M` sets it machine-wide so any COM client process can see it. **Environment variables are only
read at process start** — any app you launch to test this (VB6 IDE, VFP, etc.) must be started
*after* `setx`, so close and reopen it if it was already running.

## 6. Register the COM server

From an **elevated** (Run as Administrator) Command Prompt, `cd` into the `publish/` folder you
copied over, then:

```bat
regsvr32 Meridian.Interop.Com.comhost.dll
```

A success dialog ("DllRegisterServer in ... succeeded") confirms registration. To unregister
later (e.g. before copying over a newer build):

```bat
regsvr32 /u Meridian.Interop.Com.comhost.dll
```

**Bitness of `regsvr32.exe` itself matters too**, independent of the DLL's own bitness:

- On a 64-bit VM, registering the **x86** comhost requires the 32-bit `regsvr32`, at
  `%WINDIR%\SysWOW64\regsvr32.exe` — it registers under the `HKCR\WOW6432Node` registry hive,
  which is what a 32-bit process (VB6, VFP9) looks under.
- The native 64-bit `regsvr32` (`%WINDIR%\System32\regsvr32.exe`) registers for 64-bit clients.
- If you register both the x86 and x64 publishes, they coexist fine under their separate hives —
  no conflict.

On a 32-bit VM, there's only one `regsvr32` and only the x86 publish is relevant anyway.

## 7. Smoke-test it

VB6 Immediate Window (or any VB6/VBA code):

```vb
Dim chat As Object
Set chat = CreateObject("Meridian.ChatService")
Debug.Print chat.Chat("What's 2 + 2?")
```

Visual FoxPro:

```foxpro
loChat = CREATEOBJECT("Meridian.ChatService")
? loChat.Chat("Summarize FoxPro in one sentence.")
```

If this returns a real model response, the bridge is registered and working end to end. The five
available ProgIds (`Meridian.ChatService`, `Meridian.EmbeddingService`, `Meridian.MemoryService`,
`Meridian.AgentService`, `Meridian.ImageService`) and their full method signatures are documented
in [`COM-INTEROP.md`](COM-INTEROP.md).

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| "Class not registered" | `regsvr32` wasn't run, or was run with the wrong bitness's `regsvr32.exe` |
| "ActiveX component can't create object" | Dependent DLLs aren't next to the comhost (always register from inside the full `publish/` folder), or the .NET Runtime isn't installed/doesn't match the published bitness |
| A method call throws immediately with a configuration-sounding message | An environment variable is missing or mistyped -- double check against [`COM-INTEROP.md`](COM-INTEROP.md#3-environment-variables) |
| Changes to an env var don't seem to take effect | The calling app (VB6 IDE, VFP) was already running when you set it -- close and reopen it |

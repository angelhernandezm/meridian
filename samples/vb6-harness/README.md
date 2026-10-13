# Meridian Harness (VB6)

A VB6 application exercising all five Meridian COM services (`Meridian.ChatService`,
`Meridian.AgentService`, `Meridian.EmbeddingService`, `Meridian.MemoryService`,
`Meridian.ImageService`) through `Meridian.Interop.Com` — the same bridge proven working in this
repo's VB6/Visual FoxPro testing. Built with only intrinsic VB6 controls, plus one small GDI+
helper module (`modGdiPlus.bas`) for displaying PNG images (see below) — no OCX references needed
at all.

## Prerequisites

1. **The Meridian COM bridge must already be registered** on this machine. See
   `COM-BRIDGE-VM-SETUP.md` and `COM-INTEROP.md` in the main Meridian repo for the full build,
   publish, and `regsvr32` steps.
2. **The `MERIDIAN_*` environment variables must be set** (API key + `MERIDIAN_DEFAULT_PROVIDER`
   at minimum), and this machine restarted (or at least VB6 closed/reopened) after setting them.
3. **An "assistant" agent must be registered in `MeridianComHost.Build()`** (in
   `src/Meridian.Interop.Com/Configuration/MeridianComHost.cs` in the main repo) for the Agent
   form to work — `Meridian.AgentService` looks agents up by name, and the COM host ships with
   none registered by default (it's documented in `COM-INTEROP.md` as a step you add yourself).
   If you pulled this harness after that file added an `options.AddAgent("assistant", ...)` call,
   you're covered; otherwise add it, then rebuild, republish, and re-register the COM bridge.
4. Classic VB6 IDE.

## Opening the project

1. Copy this folder to `C:\temp\Meridian\VB6` (or wherever you prefer).
2. Open `MeridianHarness.vbp` in the VB6 IDE.
3. Press **F5** to run, or **File > Make MeridianHarness.exe** to compile.

## Layout

```
MeridianHarness.vbp    Project file
frmMain.frm             Launcher -- five buttons, one per capability
frmChat.frm              Stateless chat (Meridian.ChatService)
frmAgent.frm             Multi-turn agent session (Meridian.AgentService)
frmEmbedding.frm         Embeddings + cosine similarity (Meridian.EmbeddingService)
frmMemory.frm            Remember / recall / forget (Meridian.MemoryService)
frmImage.frm             Generate / edit / analyze (Meridian.ImageService)
modGdiPlus.bas          GDI+-based PNG loader, used by frmImage.frm's previews
```

Each form creates its own COM object in `Form_Load` and is otherwise self-contained -- open any
one independently from `frmMain`.

## Notes on the Images form

- There's no file-picker dialog (deliberately, to avoid a Common Dialog/COMDLG32.OCX reference
  that might not be registered on every machine) -- **Load Source Image...** and **Load Image...**
  prompt for a full file path via `InputBox`. Type or paste a path like
  `C:\Users\you\Pictures\photo.png`.
- Generated/edited images round-trip through a temp file
  (`%TEMP%\meridian_generated.png` / `%TEMP%\meridian_edited.png`) before being loaded into the
  preview `PictureBox`.
- Previews use `modGdiPlus.GdiPlusLoadPicture`, not VB6's native `LoadPicture()`. Classic VB6's
  `LoadPicture` reliably handles BMP/ICO/WMF but **not PNG** (it predates the format), and
  Meridian's image generation returns PNG — with native `LoadPicture`, the file writes correctly
  to disk but the `PictureBox` stays blank. `GdiPlusLoadPicture` routes through `gdiplus.dll`
  (present on every Windows version VB6 runs on) instead, which decodes PNG correctly.
- **Edit** has no mask parameter on the COM surface (unlike the full .NET
  `IMeridianImageService`) -- it edits the whole image per your prompt.
- Image generation/editing only work when the configured provider supports it (OpenAI or Azure
  OpenAI, with an image model/deployment set) -- see `COM-INTEROP.md`. Analysis (vision) works
  with any vision-capable chat model.

## If something throws

Every button wraps its Meridian call in `On Error GoTo ErrHandler` and shows `Err.Description` in
the form's output box, so failures are visible in-app rather than crashing to a generic VB6 error
dialog. If you see "Class not registered" or similar, re-check the COM bridge registration steps
in the main repo's `COM-BRIDGE-VM-SETUP.md`.

**A note on `modGdiPlus.bas` specifically**: this is lower-level Windows API interop (GDI+ P/Invoke
declares, raw struct layouts, an OLE picture wrapper) than the rest of this project. Two issues
already found and fixed during testing:

- The compiler error **"User-defined type not defined"** on the `OleCreatePictureIndirect` declare
  happens because `IPicture` is only resolvable as a named type when a project has the "OLE
  Automation" reference explicitly checked under Project > References -- something the VB6 IDE
  adds automatically to new projects, but a hand-assembled `.vbp` (like this one) doesn't have.
  Fixed by typing every picture handle in the module as `Object` instead of the named `IPicture`
  interface -- `Set pic.Picture = GdiPlusLoadPicture(path)` still binds correctly either way, since
  the real `IPicture` resolution happens inside `PictureBox.Picture` itself, not in how the
  variable handing it off is declared.
- Classic VB6's native `LoadPicture()` doesn't reliably support PNG, which is why the Images form's
  previews came back blank even though the file on disk was correct -- `GdiPlusLoadPicture` routes
  through `gdiplus.dll` instead, which decodes PNG correctly.

If a preview still comes back blank, or `cmdGenerate_Click` throws something like "GDI+ failed to
start" or "could not be wrapped as an OLE picture," the struct layouts and GUID bytes in
`modGdiPlus.bas` are the next thing to check -- not the Meridian call itself (the file write to
disk being correct means `imgSvc.Generate(...)` and the byte-to-file write are both fine either
way).

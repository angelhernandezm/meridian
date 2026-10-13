* ============================================================================
* Meridian Harness (Visual FoxPro)
* MAIN.PRG -- entry point. Run with: DO MAIN.PRG
*
* First run: generates frmChat.scx, frmAgent.scx, frmEmbedding.scx,
* frmMemory.scx, frmImage.scx, and frmMenu.scx (see BUILDFORMS.PRG) if they
* don't exist yet, then opens the launcher. Later runs just open the launcher
* -- once built, you can also skip this file entirely and run DO FORM frmMenu
* directly.
* ============================================================================

CLOSE ALL
CLEAR ALL

IF NOT FILE("frmMenu.scx")
    DO BUILDFORMS.PRG
ENDIF

DO FORM frmMenu

READ EVENTS

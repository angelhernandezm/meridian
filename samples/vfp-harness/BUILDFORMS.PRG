* ============================================================================
* Meridian Harness (Visual FoxPro)
* BUILDFORMS.PRG -- run this ONCE to generate real, Form-Designer-editable
* .scx/.sct form files from the class definitions in frmchat.prg, frmagent.prg,
* frmembedding.prg, frmmemory.prg, frmimage.prg, and frmmenu.prg.
*
* Why this exists: VFP's native form format is a pair of binary, DBF-based
* files, not something safe to hand-author as text. This script sidesteps
* that entirely -- it loads each class definition (still plain-text .prg
* code, kept resolvable via SET PROCEDURE TO ... ADDITIVE) and lets VFP's
* own CREATEOBJECT + SaveAs() engine do the actual binary serialization, so
* the resulting .scx files are genuine, normal VFP forms you can open and
* edit in the Form Designer like any other.
*
* Each form's Init creates a real Meridian COM object, but every form in
* this project guards that in a TRY/CATCH (see cInitError), so this script
* runs fine even if the COM bridge isn't registered yet -- you'll just see
* a harmless "could not be created" message if you open a built form and
* click a button before Meridian.Interop.Com is registered.
*
* Usage, from the VFP command window:
*   SET DEFAULT TO C:\temp\Meridian\VFP
*   DO BUILDFORMS.PRG
* ============================================================================

CLOSE ALL
CLEAR ALL

* DO merely runs a .prg's top-level code once -- it does not reliably keep a
* DEFINE CLASS block inside it resolvable by CREATEOBJECT afterward. SET
* PROCEDURE TO ... ADDITIVE is the correct way to load a file's class/procedure
* definitions and keep them available for the rest of the session.
SET PROCEDURE TO frmchat.prg ADDITIVE
SET PROCEDURE TO frmagent.prg ADDITIVE
SET PROCEDURE TO frmembedding.prg ADDITIVE
SET PROCEDURE TO frmmemory.prg ADDITIVE
SET PROCEDURE TO frmimage.prg ADDITIVE
SET PROCEDURE TO frmmenu.prg ADDITIVE

LOCAL loForm

loForm = CREATEOBJECT("frmChat")
loForm.SaveAs("frmChat.scx")
loForm = .NULL.

loForm = CREATEOBJECT("frmAgent")
loForm.SaveAs("frmAgent.scx")
loForm = .NULL.

loForm = CREATEOBJECT("frmEmbedding")
loForm.SaveAs("frmEmbedding.scx")
loForm = .NULL.

loForm = CREATEOBJECT("frmMemory")
loForm.SaveAs("frmMemory.scx")
loForm = .NULL.

loForm = CREATEOBJECT("frmImage")
loForm.SaveAs("frmImage.scx")
loForm = .NULL.

loForm = CREATEOBJECT("frmMenu")
loForm.SaveAs("frmMenu.scx")
loForm = .NULL.

MESSAGEBOX("Done." + CHR(13) + CHR(10) + CHR(13) + CHR(10) + ;
    "Created in " + FULLPATH(CURDIR()) + ":" + CHR(13) + CHR(10) + ;
    "  frmChat.scx, frmAgent.scx, frmEmbedding.scx," + CHR(13) + CHR(10) + ;
    "  frmMemory.scx, frmImage.scx, frmMenu.scx" + CHR(13) + CHR(10) + CHR(13) + CHR(10) + ;
    "Run the app with:  DO FORM frmMenu" + CHR(13) + CHR(10) + ;
    "Or open any .scx in the Form Designer to edit it visually.", 64, "Meridian Harness")

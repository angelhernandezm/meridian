* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmimage.prg -- image generation, editing, and vision analysis via
* Meridian.ImageService.
*
* NOTE ON byte[] MARSHALING: .NET's byte[] crosses COM as a SAFEARRAY of
* VT_UI1. The SAFEARRAY(double)/SAFEARRAY(BSTR) cases (GetEmbedding, Recall)
* are proven working in this repo (see frmembedding.prg / COM-INTEROP.md) and
* marshal into ordinary VFP arrays, read with ALEN()/subscripting. This file
* assumes SAFEARRAY(byte) follows the same rule -- a VFP array of integers
* (0-255 each) -- which is consistent with VFP's general SAFEARRAY handling,
* but unlike Chat/Agent/Memory/Embedding, this specific path has not been
* empirically verified against the real COM bridge. If Generate/Edit/Analyze
* throw a type-mismatch error here where the other forms work fine, this is
* the first place to look -- see README.md for what to try instead.
* ============================================================================

DEFINE CLASS frmImage AS Form
    Caption = "Meridian Harness - Images"
    Height = 560
    Width = 480
    AutoCenter = .T.

    oImageSvc = .NULL.
    cInitError = ""
    DIMENSION laGenBytes[1]
    bHaveGen = .F.
    DIMENSION laEditSourceBytes[1]
    bHaveEditSource = .F.
    DIMENSION laEditBytes[1]
    bHaveEdit = .F.
    DIMENSION laAnalyzeBytes[1]
    bHaveAnalyze = .F.

    * ---- Generate ----

    ADD OBJECT lblGenPrompt AS Label WITH ;
        Top = 8, Left = 10, Width = 200, Caption = "Generate -- prompt:"

    ADD OBJECT txtGenPrompt AS TextBox WITH ;
        Top = 24, Left = 10, Width = 450, Value = "A lighthouse at sunset, watercolor style"

    ADD OBJECT lblGenW AS Label WITH Top = 54, Left = 10, Width = 20, Caption = "W:"
    ADD OBJECT txtGenWidth AS TextBox WITH Top = 52, Left = 30, Width = 60, Value = "1024"
    ADD OBJECT lblGenH AS Label WITH Top = 54, Left = 100, Width = 20, Caption = "H:"
    ADD OBJECT txtGenHeight AS TextBox WITH Top = 52, Left = 120, Width = 60, Value = "1024"

    ADD OBJECT cmdGenerate AS CommandButton WITH ;
        Top = 50, Left = 190, Width = 100, Height = 26, Caption = "Generate"

    ADD OBJECT cmdSaveGenerated AS CommandButton WITH ;
        Top = 50, Left = 300, Width = 100, Height = 26, Caption = "Save As..."

    ADD OBJECT imgGenerate AS Image WITH ;
        Top = 84, Left = 10, Width = 120, Height = 100, Stretch = 2

    * ---- Edit ----

    ADD OBJECT cmdLoadEditSource AS CommandButton WITH ;
        Top = 196, Left = 10, Width = 160, Height = 26, Caption = "Load Source Image..."

    ADD OBJECT imgEditSource AS Image WITH ;
        Top = 226, Left = 10, Width = 100, Height = 80, Stretch = 2

    ADD OBJECT txtEditPrompt AS TextBox WITH ;
        Top = 196, Left = 180, Width = 280, Value = "Add a sunset in the background."

    ADD OBJECT cmdEdit AS CommandButton WITH ;
        Top = 226, Left = 180, Width = 100, Height = 26, Caption = "Edit"

    ADD OBJECT cmdSaveEdited AS CommandButton WITH ;
        Top = 226, Left = 290, Width = 100, Height = 26, Caption = "Save As..."

    ADD OBJECT imgEditResult AS Image WITH ;
        Top = 260, Left = 180, Width = 100, Height = 80, Stretch = 2

    * ---- Analyze ----

    ADD OBJECT cmdLoadAnalyze AS CommandButton WITH ;
        Top = 350, Left = 10, Width = 140, Height = 26, Caption = "Load Image..."

    ADD OBJECT imgAnalyze AS Image WITH ;
        Top = 380, Left = 10, Width = 100, Height = 80, Stretch = 2

    ADD OBJECT txtAnalyzePrompt AS TextBox WITH ;
        Top = 350, Left = 160, Width = 300, Value = "Describe this image."

    ADD OBJECT cmdAnalyze AS CommandButton WITH ;
        Top = 378, Left = 160, Width = 100, Height = 26, Caption = "Analyze"

    ADD OBJECT txtAnalyzeOutput AS EditBox WITH ;
        Top = 410, Left = 160, Width = 300, Height = 60, ReadOnly = .T.

    ADD OBJECT lblStatus AS Label WITH ;
        Top = 480, Left = 10, Width = 450, Caption = ""

    PROCEDURE Init
        TRY
            THIS.oImageSvc = CREATEOBJECT("Meridian.ImageService")
        CATCH TO loError
            THIS.oImageSvc = .NULL.
            THIS.cInitError = loError.Message
        ENDTRY
    ENDPROC

    * ---- Generate ----

    PROCEDURE cmdGenerate.Click
        LOCAL lcTempPath

        IF ISNULL(THISFORM.oImageSvc)
            THISFORM.lblStatus.Caption = "Meridian.ImageService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtGenPrompt.Value))
            THISFORM.lblStatus.Caption = "Enter a generation prompt first."
            RETURN
        ENDIF

        TRY
            THISFORM.laGenBytes = THISFORM.oImageSvc.Generate(THISFORM.txtGenPrompt.Value, ;
                VAL(THISFORM.txtGenWidth.Value), VAL(THISFORM.txtGenHeight.Value))
            THISFORM.bHaveGen = .T.

            lcTempPath = ADDBS(SYS(2023)) + "meridian_generated.png"
            WriteBytesToFile(@THISFORM.laGenBytes, lcTempPath)
            THISFORM.imgGenerate.Picture = lcTempPath

            THISFORM.lblStatus.Caption = "Generated " + TRANSFORM(ALEN(THISFORM.laGenBytes)) + " bytes."
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdSaveGenerated.Click
        LOCAL lcSavePath

        IF NOT THISFORM.bHaveGen
            THISFORM.lblStatus.Caption = "Nothing generated yet."
            RETURN
        ENDIF

        lcSavePath = PUTFILE("Save generated image", "meridian-generated", "png")
        IF EMPTY(lcSavePath)
            RETURN
        ENDIF

        TRY
            WriteBytesToFile(@THISFORM.laGenBytes, lcSavePath)
            THISFORM.lblStatus.Caption = "Saved to " + lcSavePath
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error saving: " + loError.Message
        ENDTRY
    ENDPROC

    * ---- Edit ----

    PROCEDURE cmdLoadEditSource.Click
        LOCAL lcSourcePath

        lcSourcePath = GETFILE("PNG;JPG;JPEG;BMP", "Select an image to edit", "Open")
        IF EMPTY(lcSourcePath)
            RETURN
        ENDIF

        TRY
            THISFORM.laEditSourceBytes = ReadBytesFromFile(lcSourcePath)
            THISFORM.bHaveEditSource = .T.
            THISFORM.imgEditSource.Picture = lcSourcePath
            THISFORM.lblStatus.Caption = "Loaded source image (" + TRANSFORM(ALEN(THISFORM.laEditSourceBytes)) + " bytes)."
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error loading: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdEdit.Click
        LOCAL lcTempPath

        IF ISNULL(THISFORM.oImageSvc)
            THISFORM.lblStatus.Caption = "Meridian.ImageService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF NOT THISFORM.bHaveEditSource
            THISFORM.lblStatus.Caption = "Load a source image first."
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtEditPrompt.Value))
            THISFORM.lblStatus.Caption = "Enter edit instructions first."
            RETURN
        ENDIF

        TRY
            THISFORM.laEditBytes = THISFORM.oImageSvc.Edit(THISFORM.laEditSourceBytes, ;
                THISFORM.txtEditPrompt.Value, 1024, 1024)
            THISFORM.bHaveEdit = .T.

            lcTempPath = ADDBS(SYS(2023)) + "meridian_edited.png"
            WriteBytesToFile(@THISFORM.laEditBytes, lcTempPath)
            THISFORM.imgEditResult.Picture = lcTempPath

            THISFORM.lblStatus.Caption = "Edited image: " + TRANSFORM(ALEN(THISFORM.laEditBytes)) + " bytes."
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdSaveEdited.Click
        LOCAL lcSavePath

        IF NOT THISFORM.bHaveEdit
            THISFORM.lblStatus.Caption = "Nothing edited yet."
            RETURN
        ENDIF

        lcSavePath = PUTFILE("Save edited image", "meridian-edited", "png")
        IF EMPTY(lcSavePath)
            RETURN
        ENDIF

        TRY
            WriteBytesToFile(@THISFORM.laEditBytes, lcSavePath)
            THISFORM.lblStatus.Caption = "Saved to " + lcSavePath
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error saving: " + loError.Message
        ENDTRY
    ENDPROC

    * ---- Analyze ----

    PROCEDURE cmdLoadAnalyze.Click
        LOCAL lcSourcePath

        lcSourcePath = GETFILE("PNG;JPG;JPEG;BMP", "Select an image to analyze", "Open")
        IF EMPTY(lcSourcePath)
            RETURN
        ENDIF

        TRY
            THISFORM.laAnalyzeBytes = ReadBytesFromFile(lcSourcePath)
            THISFORM.bHaveAnalyze = .T.
            THISFORM.imgAnalyze.Picture = lcSourcePath
            THISFORM.lblStatus.Caption = "Loaded image to analyze."
        CATCH TO loError
            THISFORM.lblStatus.Caption = "Error loading: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdAnalyze.Click
        IF ISNULL(THISFORM.oImageSvc)
            THISFORM.txtAnalyzeOutput.Value = "Meridian.ImageService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF NOT THISFORM.bHaveAnalyze
            THISFORM.txtAnalyzeOutput.Value = "Load an image first."
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtAnalyzePrompt.Value))
            THISFORM.txtAnalyzeOutput.Value = "Enter a question about the image first."
            RETURN
        ENDIF

        TRY
            THISFORM.txtAnalyzeOutput.Value = THISFORM.oImageSvc.Analyze(THISFORM.laAnalyzeBytes, THISFORM.txtAnalyzePrompt.Value)
        CATCH TO loError
            THISFORM.txtAnalyzeOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

ENDDEFINE

* ============================================================================
* File <-> byte-array helpers, shared by this form's procedures above.
* ============================================================================

PROCEDURE WriteBytesToFile(taBytes, tcPath)
    LOCAL lnHandle, lnI, lnCount

    lnCount = ALEN(taBytes)
    lnHandle = FCREATE(tcPath)
    IF lnHandle < 0
        ERROR "Could not create file: " + tcPath
    ENDIF

    FOR lnI = 1 TO lnCount
        =FWRITE(lnHandle, CHR(taBytes[lnI]))
    ENDFOR

    =FCLOSE(lnHandle)
ENDPROC

FUNCTION ReadBytesFromFile(tcPath)
    LOCAL lcContents, lnLen, lnI
    LOCAL ARRAY laBytes[1]

    lcContents = FILETOSTR(tcPath)
    lnLen = LEN(lcContents)

    IF lnLen = 0
        DIMENSION laBytes[1]
        laBytes[1] = 0
        RETURN laBytes
    ENDIF

    DIMENSION laBytes[lnLen]
    FOR lnI = 1 TO lnLen
        laBytes[lnI] = ASC(SUBSTR(lcContents, lnI, 1))
    ENDFOR

    RETURN laBytes
ENDFUNC

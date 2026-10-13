* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmmemory.prg -- long-term semantic memory via Meridian.MemoryService
* (remember / recall / forget).
* ============================================================================

DEFINE CLASS frmMemory AS Form
    Caption = "Meridian Harness - Memory"
    Height = 400
    Width = 460
    AutoCenter = .T.

    oMemSvc = .NULL.
    cInitError = ""

    ADD OBJECT lblRememberText AS Label WITH ;
        Top = 10, Left = 10, Width = 300, Caption = "Fact to remember:"

    ADD OBJECT txtRememberText AS EditBox WITH ;
        Top = 28, Left = 10, Width = 320, Height = 50

    ADD OBJECT cmdRemember AS CommandButton WITH ;
        Top = 28, Left = 340, Width = 100, Height = 28, Caption = "Remember"

    ADD OBJECT lblRecallQuery AS Label WITH ;
        Top = 85, Left = 10, Width = 200, Caption = "Recall query:"

    ADD OBJECT txtRecallQuery AS TextBox WITH ;
        Top = 103, Left = 10, Width = 260

    ADD OBJECT lblTopK AS Label WITH ;
        Top = 106, Left = 280, Width = 30, Caption = "Top K:"

    ADD OBJECT txtRecallTopK AS TextBox WITH ;
        Top = 103, Left = 320, Width = 40, Value = "5"

    ADD OBJECT cmdRecall AS CommandButton WITH ;
        Top = 101, Left = 370, Width = 70, Height = 26, Caption = "Recall"

    ADD OBJECT lblForgetId AS Label WITH ;
        Top = 140, Left = 10, Width = 300, Caption = "Id to forget:"

    ADD OBJECT txtForgetId AS TextBox WITH ;
        Top = 158, Left = 10, Width = 330

    ADD OBJECT cmdForget AS CommandButton WITH ;
        Top = 156, Left = 350, Width = 90, Height = 26, Caption = "Forget"

    ADD OBJECT lblOutput AS Label WITH ;
        Top = 195, Left = 10, Width = 200, Caption = "Result:"

    ADD OBJECT txtOutput AS EditBox WITH ;
        Top = 213, Left = 10, Width = 430, Height = 150, ReadOnly = .T.

    PROCEDURE Init
        TRY
            THIS.oMemSvc = CREATEOBJECT("Meridian.MemoryService")
        CATCH TO loError
            THIS.oMemSvc = .NULL.
            THIS.cInitError = loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdRemember.Click
        LOCAL lcId

        IF ISNULL(THISFORM.oMemSvc)
            THISFORM.txtOutput.Value = "Meridian.MemoryService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtRememberText.Value))
            THISFORM.txtOutput.Value = "Enter a fact to remember first."
            RETURN
        ENDIF

        TRY
            lcId = THISFORM.oMemSvc.Remember(THISFORM.txtRememberText.Value)
            THISFORM.txtOutput.Value = 'Remembered as id "' + lcId + '".'
            THISFORM.txtRememberText.Value = ""
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdRecall.Click
        LOCAL lnTopK, lnI, lcOut
        LOCAL ARRAY laResults[1]

        IF ISNULL(THISFORM.oMemSvc)
            THISFORM.txtOutput.Value = "Meridian.MemoryService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtRecallQuery.Value))
            THISFORM.txtOutput.Value = "Enter a recall query first."
            RETURN
        ENDIF

        lnTopK = VAL(THISFORM.txtRecallTopK.Value)
        IF lnTopK <= 0
            lnTopK = 5
        ENDIF

        TRY
            laResults = THISFORM.oMemSvc.Recall(THISFORM.txtRecallQuery.Value, lnTopK)

            lcOut = ""
            IF ALEN(laResults) = 0
                lcOut = "Nothing relevant found."
            ELSE
                FOR lnI = 1 TO ALEN(laResults)
                    lcOut = lcOut + laResults(lnI) + CHR(13) + CHR(10) + CHR(13) + CHR(10)
                ENDFOR
            ENDIF
            THISFORM.txtOutput.Value = lcOut
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdForget.Click
        IF ISNULL(THISFORM.oMemSvc)
            THISFORM.txtOutput.Value = "Meridian.MemoryService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtForgetId.Value))
            THISFORM.txtOutput.Value = "Enter an id to forget first."
            RETURN
        ENDIF

        TRY
            THISFORM.oMemSvc.Forget(THISFORM.txtForgetId.Value)
            THISFORM.txtOutput.Value = 'Forgot id "' + THISFORM.txtForgetId.Value + '" (if it existed).'
            THISFORM.txtForgetId.Value = ""
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

ENDDEFINE

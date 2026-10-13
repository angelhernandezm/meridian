* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmchat.prg -- stateless, single-turn chat via Meridian.ChatService.
* ============================================================================

DEFINE CLASS frmChat AS Form
    Caption = "Meridian Harness - Chat"
    Height = 330
    Width = 460
    AutoCenter = .T.

    oChatSvc = .NULL.
    cInitError = ""

    ADD OBJECT lblInstructions AS Label WITH ;
        Top = 10, Left = 10, Width = 300, ;
        Caption = "System instructions (optional):"

    ADD OBJECT txtInstructions AS EditBox WITH ;
        Top = 30, Left = 10, Width = 430, Height = 50

    ADD OBJECT lblPrompt AS Label WITH ;
        Top = 90, Left = 10, Width = 300, ;
        Caption = "Prompt:"

    ADD OBJECT txtPrompt AS EditBox WITH ;
        Top = 110, Left = 10, Width = 430, Height = 50

    ADD OBJECT cmdSend AS CommandButton WITH ;
        Top = 165, Left = 10, Width = 100, Height = 28, ;
        Caption = "Send"

    ADD OBJECT lblOutput AS Label WITH ;
        Top = 200, Left = 10, Width = 300, ;
        Caption = "Response:"

    ADD OBJECT txtOutput AS EditBox WITH ;
        Top = 220, Left = 10, Width = 430, Height = 90, ;
        ReadOnly = .T.

    PROCEDURE Init
        TRY
            THIS.oChatSvc = CREATEOBJECT("Meridian.ChatService")
        CATCH TO loError
            THIS.oChatSvc = .NULL.
            THIS.cInitError = loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdSend.Click
        LOCAL lcPrompt, lcInstructions

        IF ISNULL(THISFORM.oChatSvc)
            THISFORM.txtOutput.Value = "Meridian.ChatService could not be created: " + THISFORM.cInitError + ;
                CHR(13) + CHR(10) + "Check that the COM bridge is registered and MERIDIAN_* env vars are set (see COM-BRIDGE-VM-SETUP.md)."
            RETURN
        ENDIF

        lcPrompt = ALLTRIM(THISFORM.txtPrompt.Value)
        IF EMPTY(lcPrompt)
            THISFORM.txtOutput.Value = "Enter a message first."
            RETURN
        ENDIF

        lcInstructions = ALLTRIM(THISFORM.txtInstructions.Value)

        TRY
            IF NOT EMPTY(lcInstructions)
                THISFORM.txtOutput.Value = THISFORM.oChatSvc.ChatWithInstructions(lcInstructions, lcPrompt)
            ELSE
                THISFORM.txtOutput.Value = THISFORM.oChatSvc.Chat(lcPrompt)
            ENDIF
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

ENDDEFINE

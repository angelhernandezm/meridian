* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmagent.prg -- multi-turn conversation with a named agent via
* Meridian.AgentService. "assistant" must be registered in MeridianComHost's
* AddAgent(...) call (see Meridian.Interop.Com) for this to resolve.
* ============================================================================

DEFINE CLASS frmAgent AS Form
    Caption = "Meridian Harness - Agent"
    Height = 420
    Width = 460
    AutoCenter = .T.

    oAgentSvc = .NULL.
    cSessionId = ""
    cInitError = ""

    ADD OBJECT lblAgentName AS Label WITH ;
        Top = 10, Left = 10, Width = 100, ;
        Caption = "Agent name:"

    ADD OBJECT txtAgentName AS TextBox WITH ;
        Top = 8, Left = 115, Width = 160, ;
        Value = "assistant"

    ADD OBJECT cmdNewSession AS CommandButton WITH ;
        Top = 6, Left = 290, Width = 150, Height = 28, ;
        Caption = "New Session"

    ADD OBJECT lblTranscript AS Label WITH ;
        Top = 45, Left = 10, Width = 300, ;
        Caption = "Transcript:"

    ADD OBJECT txtTranscript AS EditBox WITH ;
        Top = 65, Left = 10, Width = 430, Height = 220, ;
        ReadOnly = .T.

    ADD OBJECT lblMessage AS Label WITH ;
        Top = 295, Left = 10, Width = 100, ;
        Caption = "Message:"

    ADD OBJECT txtMessage AS TextBox WITH ;
        Top = 315, Left = 10, Width = 330

    ADD OBJECT cmdSend AS CommandButton WITH ;
        Top = 313, Left = 350, Width = 90, Height = 26, ;
        Caption = "Send"

    PROCEDURE Init
        TRY
            THIS.oAgentSvc = CREATEOBJECT("Meridian.AgentService")
        CATCH TO loError
            THIS.oAgentSvc = .NULL.
            THIS.cInitError = loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE Destroy
        IF NOT EMPTY(THIS.cSessionId)
            TRY
                THIS.oAgentSvc.CloseSession(THIS.cSessionId)
            CATCH
                * ignore -- best effort cleanup
            ENDTRY
        ENDIF
    ENDPROC

    PROCEDURE cmdNewSession.Click
        IF ISNULL(THISFORM.oAgentSvc)
            THISFORM.txtTranscript.Value = "Meridian.AgentService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        TRY
            THISFORM.cSessionId = THISFORM.oAgentSvc.CreateSession(ALLTRIM(THISFORM.txtAgentName.Value))
            THISFORM.txtTranscript.Value = "New session started." + CHR(13) + CHR(10)
        CATCH TO loError
            THISFORM.txtTranscript.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdSend.Click
        LOCAL lcMessage, lcReply

        IF ISNULL(THISFORM.oAgentSvc)
            THISFORM.txtTranscript.Value = "Meridian.AgentService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF

        lcMessage = ALLTRIM(THISFORM.txtMessage.Value)
        IF EMPTY(lcMessage)
            RETURN
        ENDIF

        TRY
            IF EMPTY(THISFORM.cSessionId)
                THISFORM.cSessionId = THISFORM.oAgentSvc.CreateSession(ALLTRIM(THISFORM.txtAgentName.Value))
            ENDIF

            lcReply = THISFORM.oAgentSvc.Run(THISFORM.cSessionId, lcMessage)

            THISFORM.txtTranscript.Value = THISFORM.txtTranscript.Value + ;
                "You: " + lcMessage + CHR(13) + CHR(10) + CHR(13) + CHR(10) + ;
                "Assistant: " + lcReply + CHR(13) + CHR(10) + CHR(13) + CHR(10)
            THISFORM.txtMessage.Value = ""
        CATCH TO loError
            THISFORM.txtTranscript.Value = THISFORM.txtTranscript.Value + "Error: " + loError.Message + CHR(13) + CHR(10)
        ENDTRY
    ENDPROC

ENDDEFINE

* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmmenu.prg -- launcher: five buttons, one per Meridian COM service.
* ============================================================================

DEFINE CLASS frmMenu AS Form
    Caption = "Meridian Harness (VFP)"
    Height = 230
    Width = 300
    AutoCenter = .T.
    MinButton = .F.
    MaxButton = .F.

    ADD OBJECT cmdChat AS CommandButton WITH ;
        Top = 15, Left = 15, Width = 260, Height = 28, ;
        Caption = "Chat (stateless)"

    ADD OBJECT cmdAgent AS CommandButton WITH ;
        Top = 50, Left = 15, Width = 260, Height = 28, ;
        Caption = "Agent (multi-turn)"

    ADD OBJECT cmdEmbedding AS CommandButton WITH ;
        Top = 85, Left = 15, Width = 260, Height = 28, ;
        Caption = "Embeddings"

    ADD OBJECT cmdMemory AS CommandButton WITH ;
        Top = 120, Left = 15, Width = 260, Height = 28, ;
        Caption = "Memory"

    ADD OBJECT cmdImage AS CommandButton WITH ;
        Top = 155, Left = 15, Width = 260, Height = 28, ;
        Caption = "Images (Generate / Edit / Analyze)"

    PROCEDURE cmdChat.Click
        DO FORM frmChat
    ENDPROC

    PROCEDURE cmdAgent.Click
        DO FORM frmAgent
    ENDPROC

    PROCEDURE cmdEmbedding.Click
        DO FORM frmEmbedding
    ENDPROC

    PROCEDURE cmdMemory.Click
        DO FORM frmMemory
    ENDPROC

    PROCEDURE cmdImage.Click
        DO FORM frmImage
    ENDPROC

    PROCEDURE Destroy
        CLEAR EVENTS
    ENDPROC

ENDDEFINE

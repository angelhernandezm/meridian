* ============================================================================
* Meridian Harness (Visual FoxPro)
* frmembedding.prg -- text embeddings and cosine similarity via
* Meridian.EmbeddingService. SAFEARRAY-of-double results marshal into VFP
* arrays, read with ALEN()/element subscripting (1-based), matching the
* SAFEARRAY-of-BSTR pattern documented for Recall() in COM-INTEROP.md.
* ============================================================================

DEFINE CLASS frmEmbedding AS Form
    Caption = "Meridian Harness - Embeddings"
    Height = 350
    Width = 460
    AutoCenter = .T.

    oEmbedSvc = .NULL.
    cInitError = ""
    bHaveA = .F.
    bHaveB = .F.
    DIMENSION laVectorA[1]
    DIMENSION laVectorB[1]

    ADD OBJECT lblTextA AS Label WITH ;
        Top = 10, Left = 10, Width = 200, Caption = "Text A:"

    ADD OBJECT txtTextA AS EditBox WITH ;
        Top = 28, Left = 10, Width = 320, Height = 50

    ADD OBJECT cmdEmbedA AS CommandButton WITH ;
        Top = 28, Left = 340, Width = 100, Height = 28, Caption = "Embed Text A"

    ADD OBJECT lblTextB AS Label WITH ;
        Top = 85, Left = 10, Width = 200, Caption = "Text B:"

    ADD OBJECT txtTextB AS EditBox WITH ;
        Top = 103, Left = 10, Width = 320, Height = 50

    ADD OBJECT cmdEmbedB AS CommandButton WITH ;
        Top = 103, Left = 340, Width = 100, Height = 28, Caption = "Embed Text B"

    ADD OBJECT cmdCosine AS CommandButton WITH ;
        Top = 160, Left = 10, Width = 200, Height = 28, Caption = "Cosine Similarity (A, B)"

    ADD OBJECT lblOutput AS Label WITH ;
        Top = 200, Left = 10, Width = 200, Caption = "Result:"

    ADD OBJECT txtOutput AS EditBox WITH ;
        Top = 218, Left = 10, Width = 430, Height = 90, ReadOnly = .T.

    PROCEDURE Init
        TRY
            THIS.oEmbedSvc = CREATEOBJECT("Meridian.EmbeddingService")
        CATCH TO loError
            THIS.oEmbedSvc = .NULL.
            THIS.cInitError = loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdEmbedA.Click
        IF ISNULL(THISFORM.oEmbedSvc)
            THISFORM.txtOutput.Value = "Meridian.EmbeddingService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtTextA.Value))
            THISFORM.txtOutput.Value = "Enter text A first."
            RETURN
        ENDIF
        TRY
            THISFORM.laVectorA = THISFORM.oEmbedSvc.GetEmbedding(THISFORM.txtTextA.Value)
            THISFORM.bHaveA = .T.
            THISFORM.txtOutput.Value = "Text A: " + TRANSFORM(ALEN(THISFORM.laVectorA)) + " dimensions."
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdEmbedB.Click
        IF ISNULL(THISFORM.oEmbedSvc)
            THISFORM.txtOutput.Value = "Meridian.EmbeddingService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF
        IF EMPTY(ALLTRIM(THISFORM.txtTextB.Value))
            THISFORM.txtOutput.Value = "Enter text B first."
            RETURN
        ENDIF
        TRY
            THISFORM.laVectorB = THISFORM.oEmbedSvc.GetEmbedding(THISFORM.txtTextB.Value)
            THISFORM.bHaveB = .T.
            THISFORM.txtOutput.Value = "Text B: " + TRANSFORM(ALEN(THISFORM.laVectorB)) + " dimensions."
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

    PROCEDURE cmdCosine.Click
        LOCAL lnSimilarity

        IF ISNULL(THISFORM.oEmbedSvc)
            THISFORM.txtOutput.Value = "Meridian.EmbeddingService could not be created: " + THISFORM.cInitError
            RETURN
        ENDIF

        TRY
            IF NOT THISFORM.bHaveA
                IF EMPTY(ALLTRIM(THISFORM.txtTextA.Value))
                    THISFORM.txtOutput.Value = "Embed text A first."
                    RETURN
                ENDIF
                THISFORM.laVectorA = THISFORM.oEmbedSvc.GetEmbedding(THISFORM.txtTextA.Value)
                THISFORM.bHaveA = .T.
            ENDIF

            IF NOT THISFORM.bHaveB
                IF EMPTY(ALLTRIM(THISFORM.txtTextB.Value))
                    THISFORM.txtOutput.Value = "Embed text B first."
                    RETURN
                ENDIF
                THISFORM.laVectorB = THISFORM.oEmbedSvc.GetEmbedding(THISFORM.txtTextB.Value)
                THISFORM.bHaveB = .T.
            ENDIF

            lnSimilarity = THISFORM.oEmbedSvc.CosineSimilarity(THISFORM.laVectorA, THISFORM.laVectorB)
            THISFORM.txtOutput.Value = "Cosine similarity: " + TRANSFORM(lnSimilarity, "9.9999") + " (range -1..1)"
        CATCH TO loError
            THISFORM.txtOutput.Value = "Error: " + loError.Message
        ENDTRY
    ENDPROC

ENDDEFINE

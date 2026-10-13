VERSION 5.00
Begin VB.Form frmEmbedding 
   Caption         =   "Meridian Harness - Embeddings"
   ClientHeight    =   4800
   ClientLeft      =   2340
   ClientTop       =   1950
   ClientWidth     =   6180
   LinkTopic       =   "frmEmbedding"
   ScaleHeight     =   4800
   ScaleWidth      =   6180
   StartUpPosition =   3  'Windows Default
   Begin VB.TextBox txtOutput 
      BeginProperty Font 
         Name            =   "Courier New"
         Size            =   9
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   1455
      Left            =   240
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   7
      Top             =   3240
      Width           =   5700
   End
   Begin VB.CommandButton cmdCosine 
      Caption         =   "Cosine Similarity (A, B)"
      Height          =   375
      Left            =   240
      TabIndex        =   6
      Top             =   2520
      Width           =   2535
   End
   Begin VB.CommandButton cmdEmbedB 
      Caption         =   "Embed Text B"
      Height          =   375
      Left            =   4440
      TabIndex        =   5
      Top             =   1680
      Width           =   1500
   End
   Begin VB.TextBox txtTextB 
      Height          =   615
      Left            =   240
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   3
      Top             =   1680
      Width           =   4095
   End
   Begin VB.CommandButton cmdEmbedA 
      Caption         =   "Embed Text A"
      Height          =   375
      Left            =   4440
      TabIndex        =   2
      Top             =   480
      Width           =   1500
   End
   Begin VB.TextBox txtTextA 
      Height          =   615
      Left            =   240
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   1
      Top             =   480
      Width           =   4095
   End
   Begin VB.Label lblOutput 
      Caption         =   "Result:"
      Height          =   255
      Left            =   240
      TabIndex        =   8
      Top             =   3000
      Width           =   2175
   End
   Begin VB.Label lblTextB 
      Caption         =   "Text B:"
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   1440
      Width           =   2175
   End
   Begin VB.Label lblTextA 
      Caption         =   "Text A:"
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   2175
   End
End
Attribute VB_Name = "frmEmbedding"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmEmbedding.frm -- text embeddings and cosine similarity via
' Meridian.EmbeddingService. SAFEARRAY-of-double / SAFEARRAY-of-BSTR results
' marshal directly into VB6 Double()/String() dynamic arrays.
' ============================================================================

Private embedSvc As Object
Private vectorA() As Double
Private vectorB() As Double
Private haveA As Boolean
Private haveB As Boolean

Private Sub Form_Load()
    Set embedSvc = CreateObject("Meridian.EmbeddingService")
End Sub

Private Sub cmdEmbedA_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtTextA.Text)) = 0 Then
        txtOutput.Text = "Enter text A first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    vectorA = embedSvc.GetEmbedding(txtTextA.Text)
    haveA = True
    Screen.MousePointer = vbDefault

    txtOutput.Text = "Text A: " & (UBound(vectorA) - LBound(vectorA) + 1) & " dimensions."
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

Private Sub cmdEmbedB_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtTextB.Text)) = 0 Then
        txtOutput.Text = "Enter text B first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    vectorB = embedSvc.GetEmbedding(txtTextB.Text)
    haveB = True
    Screen.MousePointer = vbDefault

    txtOutput.Text = "Text B: " & (UBound(vectorB) - LBound(vectorB) + 1) & " dimensions."
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

Private Sub cmdCosine_Click()
    On Error GoTo ErrHandler

    If Not haveA Then
        If Len(Trim$(txtTextA.Text)) = 0 Then
            txtOutput.Text = "Embed text A first."
            Exit Sub
        End If
        Screen.MousePointer = vbHourglass
        vectorA = embedSvc.GetEmbedding(txtTextA.Text)
        haveA = True
        Screen.MousePointer = vbDefault
    End If

    If Not haveB Then
        If Len(Trim$(txtTextB.Text)) = 0 Then
            txtOutput.Text = "Embed text B first."
            Exit Sub
        End If
        Screen.MousePointer = vbHourglass
        vectorB = embedSvc.GetEmbedding(txtTextB.Text)
        haveB = True
        Screen.MousePointer = vbDefault
    End If

    Screen.MousePointer = vbHourglass
    Dim similarity As Double
    similarity = embedSvc.CosineSimilarity(vectorA, vectorB)
    Screen.MousePointer = vbDefault

    txtOutput.Text = "Cosine similarity: " & Format$(similarity, "0.0000") & " (range -1..1)"
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

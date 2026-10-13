VERSION 5.00
Begin VB.Form frmMemory 
   Caption         =   "Meridian Harness - Memory"
   ClientHeight    =   5415
   ClientLeft      =   2280
   ClientTop       =   1740
   ClientWidth     =   6180
   LinkTopic       =   "frmMemory"
   ScaleHeight     =   5415
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
      Height          =   1815
      Left            =   240
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   10
      Top             =   3480
      Width           =   5700
   End
   Begin VB.CommandButton cmdForget 
      Caption         =   "Forget"
      Height          =   375
      Left            =   4680
      TabIndex        =   9
      Top             =   2685
      Width           =   1260
   End
   Begin VB.TextBox txtForgetId 
      Height          =   375
      Left            =   240
      TabIndex        =   8
      Top             =   2685
      Width           =   4335
   End
   Begin VB.CommandButton cmdRecall 
      Caption         =   "Recall"
      Height          =   375
      Left            =   4680
      TabIndex        =   7
      Top             =   2000
      Width           =   1260
   End
   Begin VB.TextBox txtRecallTopK 
      Height          =   375
      Left            =   4680
      TabIndex        =   5
      Text            =   "5"
      Top             =   1440
      Width           =   615
   End
   Begin VB.TextBox txtRecallQuery 
      Height          =   375
      Left            =   240
      TabIndex        =   4
      Top             =   1440
      Width           =   4335
   End
   Begin VB.CommandButton cmdRemember 
      Caption         =   "Remember"
      Height          =   375
      Left            =   4680
      TabIndex        =   2
      Top             =   480
      Width           =   1260
   End
   Begin VB.TextBox txtRememberText 
      Height          =   615
      Left            =   240
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   1
      Top             =   480
      Width           =   4335
   End
   Begin VB.Label lblOutput 
      Caption         =   "Result:"
      Height          =   255
      Left            =   240
      TabIndex        =   11
      Top             =   3240
      Width           =   2175
   End
   Begin VB.Label lblForgetId 
      Caption         =   "Id to forget:"
      Height          =   255
      Left            =   240
      TabIndex        =   6
      Top             =   2445
      Width           =   3015
   End
   Begin VB.Label lblTopK 
      Caption         =   "Top K:"
      Height          =   255
      Left            =   3960
      TabIndex        =   3
      Top             =   1200
      Width           =   615
   End
   Begin VB.Label lblRecallQuery 
      Caption         =   "Recall query:"
      Height          =   255
      Left            =   240
      TabIndex        =   12
      Top             =   1200
      Width           =   3015
   End
   Begin VB.Label lblRememberText 
      Caption         =   "Fact to remember:"
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   3015
   End
End
Attribute VB_Name = "frmMemory"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmMemory.frm -- long-term semantic memory via Meridian.MemoryService
' (remember / recall / forget).
' ============================================================================

Private memSvc As Object

Private Sub Form_Load()
    Set memSvc = CreateObject("Meridian.MemoryService")
End Sub

Private Sub cmdRemember_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtRememberText.Text)) = 0 Then
        txtOutput.Text = "Enter a fact to remember first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    Dim newId As String
    newId = memSvc.Remember(txtRememberText.Text)
    Screen.MousePointer = vbDefault

    txtOutput.Text = "Remembered as id """ & newId & """."
    txtRememberText.Text = ""
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

Private Sub cmdRecall_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtRecallQuery.Text)) = 0 Then
        txtOutput.Text = "Enter a recall query first."
        Exit Sub
    End If

    Dim topK As Integer
    topK = CInt(Val(txtRecallTopK.Text))
    If topK <= 0 Then topK = 5

    Screen.MousePointer = vbHourglass
    Dim results() As String
    results = memSvc.Recall(txtRecallQuery.Text, topK)
    Screen.MousePointer = vbDefault

    Dim i As Integer
    Dim out As String
    If (UBound(results) - LBound(results) + 1) <= 0 Then
        out = "Nothing relevant found."
    Else
        For i = LBound(results) To UBound(results)
            out = out & results(i) & vbCrLf & vbCrLf
        Next i
    End If
    txtOutput.Text = out
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

Private Sub cmdForget_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtForgetId.Text)) = 0 Then
        txtOutput.Text = "Enter an id to forget first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    memSvc.Forget txtForgetId.Text
    Screen.MousePointer = vbDefault

    txtOutput.Text = "Forgot id """ & txtForgetId.Text & """ (if it existed)."
    txtForgetId.Text = ""
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

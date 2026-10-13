VERSION 5.00
Begin VB.Form frmChat 
   Caption         =   "Meridian Harness - Chat"
   ClientHeight    =   4995
   ClientLeft      =   2400
   ClientTop       =   2100
   ClientWidth     =   6180
   LinkTopic       =   "frmChat"
   ScaleHeight     =   4995
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
      Height          =   1935
      Left            =   240
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   5
      Top             =   2880
      Width           =   5700
   End
   Begin VB.CommandButton cmdSend 
      Caption         =   "Send"
      Height          =   375
      Left            =   240
      TabIndex        =   4
      Top             =   2160
      Width           =   1335
   End
   Begin VB.TextBox txtPrompt 
      Height          =   615
      Left            =   240
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   2
      Top             =   1440
      Width           =   5700
   End
   Begin VB.TextBox txtInstructions 
      Height          =   615
      Left            =   240
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   0
      Top             =   360
      Width           =   5700
   End
   Begin VB.Label lblOutput 
      Caption         =   "Response:"
      Height          =   255
      Left            =   240
      TabIndex        =   6
      Top             =   2640
      Width           =   2175
   End
   Begin VB.Label lblPrompt 
      Caption         =   "Prompt:"
      Height          =   255
      Left            =   240
      TabIndex        =   3
      Top             =   1200
      Width           =   2175
   End
   Begin VB.Label lblInstructions 
      Caption         =   "System instructions (optional):"
      Height          =   255
      Left            =   240
      TabIndex        =   1
      Top             =   120
      Width           =   3015
   End
End
Attribute VB_Name = "frmChat"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmChat.frm -- stateless, single-turn chat via Meridian.ChatService.
' ============================================================================

Private chatSvc As Object

Private Sub Form_Load()
    Set chatSvc = CreateObject("Meridian.ChatService")
End Sub

Private Sub cmdSend_Click()
    On Error GoTo ErrHandler

    Dim prompt As String
    prompt = Trim$(txtPrompt.Text)
    If Len(prompt) = 0 Then
        txtOutput.Text = "Enter a message first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    If Len(Trim$(txtInstructions.Text)) > 0 Then
        txtOutput.Text = chatSvc.ChatWithInstructions(txtInstructions.Text, prompt)
    Else
        txtOutput.Text = chatSvc.Chat(prompt)
    End If
    Screen.MousePointer = vbDefault
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtOutput.Text = "Error: " & Err.Description
End Sub

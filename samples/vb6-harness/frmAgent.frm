VERSION 5.00
Begin VB.Form frmAgent 
   Caption         =   "Meridian Harness - Agent"
   ClientHeight    =   5565
   ClientLeft      =   2280
   ClientTop       =   1800
   ClientWidth     =   6315
   LinkTopic       =   "frmAgent"
   ScaleHeight     =   5565
   ScaleWidth      =   6315
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdSend 
      Caption         =   "Send"
      Height          =   375
      Left            =   5040
      TabIndex        =   7
      Top             =   5040
      Width           =   1140
   End
   Begin VB.TextBox txtMessage 
      Height          =   375
      Left            =   240
      TabIndex        =   6
      Top             =   5040
      Width           =   4455
   End
   Begin VB.TextBox txtTranscript 
      BeginProperty Font 
         Name            =   "Courier New"
         Size            =   9
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   3255
      Left            =   240
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      ScrollBars      =   2  'Vertical
      TabIndex        =   5
      Top             =   1440
      Width           =   5700
   End
   Begin VB.CommandButton cmdNewSession 
      Caption         =   "New Session"
      Height          =   375
      Left            =   240
      TabIndex        =   2
      Top             =   720
      Width           =   1575
   End
   Begin VB.TextBox txtAgentName 
      Height          =   375
      Left            =   1680
      TabIndex        =   1
      Text            =   "assistant"
      Top             =   240
      Width           =   2415
   End
   Begin VB.Label lblMessage 
      Caption         =   "Message:"
      Height          =   255
      Left            =   240
      TabIndex        =   3
      Top             =   4800
      Width           =   2175
   End
   Begin VB.Label lblTranscript 
      Caption         =   "Transcript:"
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   1200
      Width           =   2175
   End
   Begin VB.Label lblAgentName 
      Caption         =   "Agent name:"
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   300
      Width           =   1335
   End
End
Attribute VB_Name = "frmAgent"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmAgent.frm -- multi-turn conversation with a named agent via
' Meridian.AgentService. "assistant" must be registered in MeridianComHost's
' AddAgent(...) call (see Meridian.Interop.Com) for this to resolve.
' ============================================================================

Private agentSvc As Object
Private sessionId As String

Private Sub Form_Load()
    Set agentSvc = CreateObject("Meridian.AgentService")
End Sub

Private Sub Form_Unload(Cancel As Integer)
    On Error Resume Next
    If Len(sessionId) > 0 Then
        agentSvc.CloseSession sessionId
    End If
End Sub

Private Sub cmdNewSession_Click()
    On Error GoTo ErrHandler

    Screen.MousePointer = vbHourglass
    sessionId = agentSvc.CreateSession(Trim$(txtAgentName.Text))
    Screen.MousePointer = vbDefault

    txtTranscript.Text = "New session started." & vbCrLf
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtTranscript.Text = "Error: " & Err.Description
End Sub

Private Sub cmdSend_Click()
    On Error GoTo ErrHandler

    Dim msg As String
    msg = Trim$(txtMessage.Text)
    If Len(msg) = 0 Then Exit Sub

    If Len(sessionId) = 0 Then
        Screen.MousePointer = vbHourglass
        sessionId = agentSvc.CreateSession(Trim$(txtAgentName.Text))
        Screen.MousePointer = vbDefault
    End If

    Screen.MousePointer = vbHourglass
    Dim reply As String
    reply = agentSvc.Run(sessionId, msg)
    Screen.MousePointer = vbDefault

    txtTranscript.Text = txtTranscript.Text & "You: " & msg & vbCrLf & vbCrLf & _
                          "Assistant: " & reply & vbCrLf & vbCrLf
    txtMessage.Text = ""
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtTranscript.Text = txtTranscript.Text & "Error: " & Err.Description & vbCrLf
End Sub


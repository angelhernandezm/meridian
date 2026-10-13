VERSION 5.00
Begin VB.Form frmMain
   Caption         =   "Meridian Harness (VB6)"
   ClientHeight    =   3195
   ClientLeft      =   2760
   ClientTop       =   3195
   ClientWidth     =   4680
   LinkTopic       =   "Form1"
   ScaleHeight     =   3195
   ScaleWidth      =   4680
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdImage
      Caption         =   "Images (Generate / Edit / Analyze)"
      Height          =   495
      Left            =   360
      TabIndex        =   4
      Top             =   2520
      Width           =   3975
   End
   Begin VB.CommandButton cmdMemory
      Caption         =   "Memory"
      Height          =   495
      Left            =   360
      TabIndex        =   3
      Top             =   1920
      Width           =   3975
   End
   Begin VB.CommandButton cmdEmbedding
      Caption         =   "Embeddings"
      Height          =   495
      Left            =   360
      TabIndex        =   2
      Top             =   1320
      Width           =   3975
   End
   Begin VB.CommandButton cmdAgent
      Caption         =   "Agent (multi-turn)"
      Height          =   495
      Left            =   360
      TabIndex        =   1
      Top             =   720
      Width           =   3975
   End
   Begin VB.CommandButton cmdChat
      Caption         =   "Chat (stateless)"
      Height          =   495
      Left            =   360
      TabIndex        =   0
      Top             =   120
      Width           =   3975
   End
End
Attribute VB_Name = "frmMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmMain.frm -- launcher: pick a capability, opens the matching form.
' Each form creates its own Meridian COM object on load (Meridian.ChatService,
' Meridian.AgentService, Meridian.EmbeddingService, Meridian.MemoryService,
' Meridian.ImageService), all backed by the same Meridian pipeline configured
' via MERIDIAN_* environment variables. See README.md for setup.
' ============================================================================

Private Sub cmdChat_Click()
    frmChat.Show
End Sub

Private Sub cmdAgent_Click()
    frmAgent.Show
End Sub

Private Sub cmdEmbedding_Click()
    frmEmbedding.Show
End Sub

Private Sub cmdMemory_Click()
    frmMemory.Show
End Sub

Private Sub cmdImage_Click()
    frmImage.Show
End Sub

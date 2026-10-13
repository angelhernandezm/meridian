VERSION 5.00
Begin VB.Form frmImage
   Caption         =   "Meridian Harness - Images"
   ClientHeight    =   8550
   ClientLeft      =   1800
   ClientTop       =   600
   ClientWidth     =   7020
   LinkTopic       =   "frmImage"
   ScaleHeight     =   8550
   ScaleWidth      =   7020
   StartUpPosition =   3  'Windows Default
   Begin VB.Label lblStatus
      BeginProperty Font
         Name            =   "MS Sans Serif"
         Size            =   8.25
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   495
      Left            =   240
      TabIndex        =   28
      Top             =   8040
      Width           =   6540
   End
   Begin VB.Frame fraAnalyze
      Caption         =   "Analyze (vision)"
      Height          =   2175
      Left            =   240
      TabIndex        =   20
      Top             =   5760
      Width           =   6540
      Begin VB.TextBox txtAnalyzeOutput
         BeginProperty Font
            Name            =   "Courier New"
            Size            =   9
            Charset         =   0
            Weight          =   400
            Underline       =   0   'False
            Italic          =   0   'False
            Strikethrough   =   0   'False
         EndProperty
         Height          =   855
         Left            =   1680
         Locked          =   -1  'True
         MultiLine       =   -1  'True
         ScrollBars      =   2  'Vertical
         TabIndex        =   26
         Top             =   1080
         Width           =   4680
      End
      Begin VB.CommandButton cmdAnalyze
         Caption         =   "Analyze"
         Height          =   375
         Left            =   1680
         TabIndex        =   25
         Top             =   640
         Width           =   1215
      End
      Begin VB.TextBox txtAnalyzePrompt
         Height          =   375
         Left            =   1680
         TabIndex        =   23
         Text            =   "Describe this image."
         Top             =   240
         Width           =   4680
      End
      Begin VB.CommandButton cmdLoadAnalyze
         Caption         =   "Load Image..."
         Height          =   375
         Left            =   240
         TabIndex        =   21
         Top             =   240
         Width           =   1335
      End
      Begin VB.PictureBox picAnalyze
         AutoSize        =   -1  'True
         Height          =   615
         Left            =   240
         ScaleHeight     =   555
         ScaleWidth      =   1275
         TabIndex        =   22
         Top             =   680
         Width           =   1335
      End
   End
   Begin VB.Frame fraEdit
      Caption         =   "Edit"
      Height          =   2895
      Left            =   240
      TabIndex        =   11
      Top             =   2760
      Width           =   6540
      Begin VB.CommandButton cmdSaveEdited
         Caption         =   "Save As..."
         Height          =   375
         Left            =   5280
         TabIndex        =   19
         Top             =   2360
         Width           =   1095
      End
      Begin VB.TextBox txtEditHeight
         Height          =   375
         Left            =   5520
         TabIndex        =   17
         Text            =   "1024"
         Top             =   1360
         Width           =   855
      End
      Begin VB.TextBox txtEditWidth
         Height          =   375
         Left            =   3960
         TabIndex        =   15
         Text            =   "1024"
         Top             =   1360
         Width           =   855
      End
      Begin VB.CommandButton cmdEdit
         Caption         =   "Edit"
         Height          =   375
         Left            =   1680
         TabIndex        =   14
         Top             =   1800
         Width           =   1215
      End
      Begin VB.TextBox txtEditPrompt
         Height          =   375
         Left            =   1680
         TabIndex        =   13
         Text            =   "Add a sunset in the background."
         Top             =   1360
         Width           =   2175
      End
      Begin VB.CommandButton cmdLoadEditSource
         Caption         =   "Load Source Image..."
         Height          =   375
         Left            =   240
         TabIndex        =   12
         Top             =   240
         Width           =   1935
      End
      Begin VB.PictureBox picEditResult
         AutoSize        =   -1  'True
         Height          =   615
         Left            =   3960
         ScaleHeight     =   555
         ScaleWidth      =   1275
         TabIndex        =   18
         Top             =   1800
         Width           =   1335
      End
      Begin VB.PictureBox picEditSource
         AutoSize        =   -1  'True
         Height          =   615
         Left            =   240
         ScaleHeight     =   555
         ScaleWidth      =   1275
         TabIndex        =   16
         Top             =   680
         Width           =   1335
      End
      Begin VB.Label lblEditHeight
         Caption         =   "H:"
         Height          =   255
         Left            =   5280
         TabIndex        =   27
         Top             =   1420
         Width           =   255
      End
      Begin VB.Label lblEditWidth
         Caption         =   "W:"
         Height          =   255
         Left            =   3720
         TabIndex        =   24
         Top             =   1420
         Width           =   255
      End
   End
   Begin VB.Frame fraGenerate
      Caption         =   "Generate"
      Height          =   2535
      Left            =   240
      TabIndex        =   0
      Top             =   120
      Width           =   6540
      Begin VB.CommandButton cmdSaveGenerated
         Caption         =   "Save As..."
         Height          =   375
         Left            =   5280
         TabIndex        =   7
         Top             =   2000
         Width           =   1095
      End
      Begin VB.TextBox txtGenHeight
         Height          =   375
         Left            =   4560
         TabIndex        =   5
         Text            =   "1024"
         Top             =   640
         Width           =   855
      End
      Begin VB.TextBox txtGenWidth
         Height          =   375
         Left            =   3000
         TabIndex        =   3
         Text            =   "1024"
         Top             =   640
         Width           =   855
      End
      Begin VB.CommandButton cmdGenerate
         Caption         =   "Generate"
         Height          =   375
         Left            =   240
         TabIndex        =   2
         Top             =   640
         Width           =   1335
      End
      Begin VB.TextBox txtGenPrompt
         Height          =   375
         Left            =   240
         TabIndex        =   1
         Text            =   "A lighthouse at sunset, watercolor style"
         Top             =   240
         Width           =   6060
      End
      Begin VB.PictureBox picGenerate
         AutoSize        =   -1  'True
         Height          =   615
         Left            =   240
         ScaleHeight     =   555
         ScaleWidth      =   1275
         TabIndex        =   6
         Top             =   1200
         Width           =   1335
      End
      Begin VB.Label lblGenHeight
         Caption         =   "H:"
         Height          =   255
         Left            =   4320
         TabIndex        =   4
         Top             =   700
         Width           =   255
      End
      Begin VB.Label lblGenWidth
         Caption         =   "W:"
         Height          =   255
         Left            =   2760
         TabIndex        =   8
         Top             =   700
         Width           =   255
      End
   End
End
Attribute VB_Name = "frmImage"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

' ============================================================================
' Meridian Harness (VB6)
' frmImage.frm -- image generation, editing, and vision analysis via
' Meridian.ImageService. Returned/loaded image bytes round-trip through a
' temp file and are displayed via modGdiPlus.GdiPlusLoadPicture, not the
' native LoadPicture() -- classic VB6's LoadPicture does not reliably
' support PNG (the format Meridian's providers return), so previews would
' otherwise come back blank even though the file itself is written correctly.
' Source images for Edit/Analyze are picked by typing/pasting a full path
' into an InputBox, avoiding any Common Dialog (COMDLG32.OCX) dependency.
' ============================================================================

Private imgSvc As Object

Private genBytes() As Byte
Private haveGenBytes As Boolean

Private editSourceBytes() As Byte
Private haveEditSource As Boolean
Private editBytes() As Byte
Private haveEditBytes As Boolean

Private analyzeSourceBytes() As Byte
Private haveAnalyzeSource As Boolean

Private Sub Form_Load()
    Set imgSvc = CreateObject("Meridian.ImageService")
End Sub

' ---- Generate ----

Private Sub cmdGenerate_Click()
    On Error GoTo ErrHandler

    If Len(Trim$(txtGenPrompt.Text)) = 0 Then
        lblStatus.Caption = "Enter a generation prompt first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    genBytes = imgSvc.Generate(txtGenPrompt.Text, CLng(Val(txtGenWidth.Text)), CLng(Val(txtGenHeight.Text)))
    haveGenBytes = True

    Dim tempPath As String
    tempPath = Environ$("TEMP") & "\meridian_generated.png"
    WriteFileBytes tempPath, genBytes
    Set picGenerate.Picture = GdiPlusLoadPicture(tempPath)
    Screen.MousePointer = vbDefault

    lblStatus.Caption = "Generated " & (UBound(genBytes) - LBound(genBytes) + 1) & " bytes."
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    lblStatus.Caption = "Error: " & Err.Description
End Sub

Private Sub cmdSaveGenerated_Click()
    If Not haveGenBytes Then
        lblStatus.Caption = "Nothing generated yet."
        Exit Sub
    End If

    Dim savePath As String
    savePath = InputBox("Save generated image as (full path, e.g. C:\temp\out.png):", "Save Image", Environ$("USERPROFILE") & "\Desktop\meridian-generated.png")
    If Len(savePath) = 0 Then Exit Sub

    On Error GoTo ErrHandler
    WriteFileBytes savePath, genBytes
    lblStatus.Caption = "Saved to " & savePath
    Exit Sub

ErrHandler:
    lblStatus.Caption = "Error saving: " & Err.Description
End Sub

' ---- Edit ----

Private Sub cmdLoadEditSource_Click()
    Dim sourcePath As String
    sourcePath = InputBox("Full path to the image to edit:", "Load Source Image")
    If Len(sourcePath) = 0 Then Exit Sub

    On Error GoTo ErrHandler
    editSourceBytes = ReadFileBytes(sourcePath)
    haveEditSource = True
    Set picEditSource.Picture = GdiPlusLoadPicture(sourcePath)
    lblStatus.Caption = "Loaded source image (" & (UBound(editSourceBytes) - LBound(editSourceBytes) + 1) & " bytes)."
    Exit Sub

ErrHandler:
    lblStatus.Caption = "Error loading: " & Err.Description
End Sub

Private Sub cmdEdit_Click()
    On Error GoTo ErrHandler

    If Not haveEditSource Then
        lblStatus.Caption = "Load a source image first."
        Exit Sub
    End If
    If Len(Trim$(txtEditPrompt.Text)) = 0 Then
        lblStatus.Caption = "Enter edit instructions first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    editBytes = imgSvc.Edit(editSourceBytes, txtEditPrompt.Text, CLng(Val(txtEditWidth.Text)), CLng(Val(txtEditHeight.Text)))
    haveEditBytes = True

    Dim tempPath As String
    tempPath = Environ$("TEMP") & "\meridian_edited.png"
    WriteFileBytes tempPath, editBytes
    Set picEditResult.Picture = GdiPlusLoadPicture(tempPath)
    Screen.MousePointer = vbDefault

    lblStatus.Caption = "Edited image: " & (UBound(editBytes) - LBound(editBytes) + 1) & " bytes."
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    lblStatus.Caption = "Error: " & Err.Description
End Sub

Private Sub cmdSaveEdited_Click()
    If Not haveEditBytes Then
        lblStatus.Caption = "Nothing edited yet."
        Exit Sub
    End If

    Dim savePath As String
    savePath = InputBox("Save edited image as (full path):", "Save Image", Environ$("USERPROFILE") & "\Desktop\meridian-edited.png")
    If Len(savePath) = 0 Then Exit Sub

    On Error GoTo ErrHandler
    WriteFileBytes savePath, editBytes
    lblStatus.Caption = "Saved to " & savePath
    Exit Sub

ErrHandler:
    lblStatus.Caption = "Error saving: " & Err.Description
End Sub

' ---- Analyze ----

Private Sub cmdLoadAnalyze_Click()
    Dim sourcePath As String
    sourcePath = InputBox("Full path to the image to analyze:", "Load Image")
    If Len(sourcePath) = 0 Then Exit Sub

    On Error GoTo ErrHandler
    analyzeSourceBytes = ReadFileBytes(sourcePath)
    haveAnalyzeSource = True
    Set picAnalyze.Picture = GdiPlusLoadPicture(sourcePath)
    lblStatus.Caption = "Loaded image to analyze."
    Exit Sub

ErrHandler:
    lblStatus.Caption = "Error loading: " & Err.Description
End Sub

Private Sub cmdAnalyze_Click()
    On Error GoTo ErrHandler

    If Not haveAnalyzeSource Then
        txtAnalyzeOutput.Text = "Load an image first."
        Exit Sub
    End If
    If Len(Trim$(txtAnalyzePrompt.Text)) = 0 Then
        txtAnalyzeOutput.Text = "Enter a question about the image first."
        Exit Sub
    End If

    Screen.MousePointer = vbHourglass
    txtAnalyzeOutput.Text = imgSvc.Analyze(analyzeSourceBytes, txtAnalyzePrompt.Text)
    Screen.MousePointer = vbDefault
    Exit Sub

ErrHandler:
    Screen.MousePointer = vbDefault
    txtAnalyzeOutput.Text = "Error: " & Err.Description
End Sub

' ---- File byte helpers ----

Private Function ReadFileBytes(path As String) As Byte()
    Dim fileNum As Integer
    Dim buffer() As Byte
    Dim fileLen As Long

    fileNum = FreeFile
    Open path For Binary Access Read As #fileNum
    fileLen = LOF(fileNum)
    If fileLen > 0 Then
        ReDim buffer(0 To fileLen - 1)
        Get #fileNum, , buffer
    Else
        ReDim buffer(0 To -1)
    End If
    Close #fileNum

    ReadFileBytes = buffer
End Function

Private Sub WriteFileBytes(path As String, data() As Byte)
    Dim fileNum As Integer
    fileNum = FreeFile
    If Dir$(path) <> "" Then Kill path
    Open path For Binary Access Write As #fileNum
    Put #fileNum, , data
    Close #fileNum
End Sub

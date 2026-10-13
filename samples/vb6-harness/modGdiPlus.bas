Attribute VB_Name = "modGdiPlus"
' ============================================================================
' Meridian Harness (VB6)
' modGdiPlus.bas -- loads PNG (and other GDI+-supported) image files into a
' picture object usable by PictureBox.Picture, routing around classic VB6's
' native LoadPicture(), which does not reliably support PNG (it predates the
' format and was built primarily for BMP/ICO/WMF). Meridian's image
' generation/editing returns PNG bytes, so frmImage.frm uses this instead of
' LoadPicture() for every preview.
'
' Technique: GDI+ (gdiplus.dll, present on every Windows version VB6 runs on
' from XP onward) loads the file and converts it to a GDI HBITMAP, which is
' then wrapped as an OLE picture via OleCreatePictureIndirect. The result is
' declared/returned As Object rather than As IPicture deliberately -- IPicture
' is only resolvable as a named type when a project has the "OLE Automation"
' (stdole2.tlb) reference explicitly checked under Project > References,
' which the VB6 IDE normally adds to new projects automatically but a
' hand-assembled .vbp (like this one) doesn't have. Using Object instead
' depends on nothing but VB6's own built-in Set semantics: `Set pic.Picture =
' GdiPlusLoadPicture(path)` still works correctly, since PictureBox.Picture
' itself does the real IPicture binding against whatever object it's handed.
' ============================================================================

Option Explicit

Private Type GdiplusStartupInput
    GdiplusVersion As Long
    DebugEventCallback As Long
    SuppressBackgroundThread As Long
    SuppressExternalCodecs As Long
End Type

Private Type PictDesc
    Size As Long
    PicType As Long
    hPic As Long
    hPal As Long
End Type

Private Type GUID
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(0 To 7) As Byte
End Type

Private Const PICTYPE_BITMAP As Long = 1

Private Declare Function GdiplusStartup Lib "gdiplus" (ByRef token As Long, ByRef inputBuf As GdiplusStartupInput, ByVal outputBuf As Long) As Long
Private Declare Sub GdiplusShutdown Lib "gdiplus" (ByVal token As Long)
Private Declare Function GdipLoadImageFromFile Lib "gdiplus" (ByVal fileName As Long, ByRef gdipImage As Long) As Long
Private Declare Function GdipCreateHBITMAPFromBitmap Lib "gdiplus" (ByVal gdipBitmap As Long, ByRef hbmReturn As Long, ByVal background As Long) As Long
Private Declare Function GdipDisposeImage Lib "gdiplus" (ByVal gdipImage As Long) As Long

Private Declare Function OleCreatePictureIndirect Lib "olepro32.dll" (ByRef PicDesc As PictDesc, ByRef RefIID As GUID, ByVal fOwn As Long, ByRef IPic As Object) As Long

' IID_IPicture = {7BF80980-BF32-101A-8BBB-00AA00300CAB}, the standard OLE picture interface
' PictureBox.Picture is already typed as.
Private Function IID_IPicture() As GUID
    Dim g As GUID
    g.Data1 = &H7BF80980
    g.Data2 = &HFFFFBF32   ' bit pattern for &HBF32 as a signed Integer -- correct, not a typo
    g.Data3 = &H101A
    g.Data4(0) = &H8B
    g.Data4(1) = &HBB
    g.Data4(2) = &H0
    g.Data4(3) = &HAA
    g.Data4(4) = &H0
    g.Data4(5) = &H30
    g.Data4(6) = &HC
    g.Data4(7) = &HAB
    IID_IPicture = g
End Function

''' Loads an image file (PNG, JPEG, BMP, GIF -- anything GDI+ supports) and
''' returns it as a picture object, suitable for `Set pic.Picture = ...`.
''' Raises an error (catchable by the caller's own On Error handler) if GDI+
''' can't be started or the file can't be decoded.
Public Function GdiPlusLoadPicture(ByVal filePath As String) As Object
    Dim token As Long
    Dim startupInput As GdiplusStartupInput
    Dim gdipImage As Long
    Dim hBmp As Long
    Dim desc As PictDesc
    Dim iid As GUID
    Dim resultPic As Object

    startupInput.GdiplusVersion = 1

    If GdiplusStartup(token, startupInput, 0) <> 0 Then
        Err.Raise vbObjectError + 1, "GdiPlusLoadPicture", "GDI+ failed to start."
    End If

    On Error GoTo CleanupAndFail

    If GdipLoadImageFromFile(StrPtr(filePath), gdipImage) <> 0 Then
        Err.Raise vbObjectError + 2, "GdiPlusLoadPicture", "GDI+ could not load """ & filePath & """."
    End If

    If GdipCreateHBITMAPFromBitmap(gdipImage, hBmp, &HFFFFFFFF) <> 0 Then
        GdipDisposeImage gdipImage
        Err.Raise vbObjectError + 3, "GdiPlusLoadPicture", "GDI+ could not convert the decoded image to a bitmap."
    End If

    GdipDisposeImage gdipImage

    desc.Size = Len(desc)
    desc.PicType = PICTYPE_BITMAP
    desc.hPic = hBmp
    desc.hPal = 0

    iid = IID_IPicture()

    ' fOwn:=1 -- the resulting picture object takes ownership of hBmp and deletes it when
    ' released, so no manual DeleteObject call is needed here.
    If OleCreatePictureIndirect(desc, iid, 1, resultPic) <> 0 Then
        Err.Raise vbObjectError + 4, "GdiPlusLoadPicture", "Could not wrap the bitmap as an OLE picture."
    End If

    GdiplusShutdown token
    Set GdiPlusLoadPicture = resultPic
    Exit Function

CleanupAndFail:
    GdiplusShutdown token
    Err.Raise Err.Number, Err.Source, Err.Description
End Function

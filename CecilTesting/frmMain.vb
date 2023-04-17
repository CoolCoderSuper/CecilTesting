Imports ICSharpCode.TextEditor.Document
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Mono.Cecil

Public Class frmMain
    Dim currentDef As MethodDefinition
    Dim manager As AssemblyManager

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim objSyntaxProvider As New FileSyntaxModeProvider(Application.StartupPath)
        HighlightingManager.Manager.AddSyntaxModeFileProvider(objSyntaxProvider)
        tecCode.SetHighlighting("VB.NET")
        tecCode.IsReadOnly = True
        manager = New AssemblyManager
        manager.LoadAssembly("C:\CodingCool\Code\Projects\CecilTesting\VBTest\bin\Debug\net8.0\VBTest.dll", tvClasses)
    End Sub

    Private Sub tvClasses_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvClasses.NodeMouseDoubleClick
        If e.Node.Tag IsNot Nothing Then
            Dim code As String = ""
            Dim mDef As MethodDefinition = TryCast(e.Node.Tag, MethodDefinition)
            If mDef IsNot Nothing Then
                code = manager.DecompileMethod(mDef)
            End If
            Dim tDef As TypeDefinition = TryCast(e.Node.Tag, TypeDefinition)
            If tDef IsNot Nothing Then
                code = manager.DecompileType(tDef)
            End If
            Dim pDef As PropertyDefinition = TryCast(e.Node.Tag, PropertyDefinition)
            If pDef IsNot Nothing Then
                code = manager.DecompileProperty(pDef)
            End If
            Dim fDef As FieldDefinition = TryCast(e.Node.Tag, FieldDefinition)
            If fDef IsNot Nothing Then
                code = manager.DecompileField(fDef)
            End If
            Dim eDef As EventDefinition = TryCast(e.Node.Tag, EventDefinition)
            If eDef IsNot Nothing Then
                code = manager.DecompileEvent(eDef)
            End If
            code = SyntaxFactory.ParseSyntaxTree(code).GetRoot.NormalizeWhitespace(True).ToFullString
            tecCode.Text = code
        End If
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        currentDef = Nothing
        tecCode.Text = ""
        manager = New AssemblyManager
        manager.LoadAssembly("C:\CodingCool\Code\Projects\CecilTesting\VBTest\bin\Debug\net8.0\VBTest.dll", tvClasses)
    End Sub

End Class
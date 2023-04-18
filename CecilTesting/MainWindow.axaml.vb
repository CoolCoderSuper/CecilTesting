Imports Avalonia.Controls
Imports Avalonia.Interactivity
Imports Avalonia.Markup.Xaml
Imports Microsoft.CodeAnalysis
Imports Mono.Cecil

Partial Public Class MainWindow
    Inherits Window

#Region "Components"
    Private WithEvents tvAssembly As TreeView
    Private WithEvents txtCode As TextBox

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        AvaloniaXamlLoader.Load(Me)
        tvAssembly = FindControl(Of TreeView)("tvAssembly")
        txtCode = FindControl(Of TextBox)("txtCode")
    End Sub

#End Region
    Dim manager As AssemblyManager

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        manager = New AssemblyManager
        tvAssembly.Items = manager.LoadAssembly("C:\CodingCool\Code\Projects\CecilTesting\VBTest\bin\Debug\net8.0\VBTest.dll")
    End Sub

    Private Sub tvAssembly_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles tvAssembly.SelectionChanged
        If e.AddedItems.Count > 0 Then
            Dim item As AssemblyItem = e.AddedItems(0)
            If item.Tag IsNot Nothing Then
                Dim tree As SyntaxTree
                Dim mDef As MethodDefinition = TryCast(item.Tag, MethodDefinition)
                If mDef IsNot Nothing Then
                    tree = manager.DecompileMethodAsSyntaxTree(mDef)
                End If
                Dim tDef As TypeDefinition = TryCast(item.Tag, TypeDefinition)
                If tDef IsNot Nothing Then
                    tree = manager.DecompileTypeAsSyntaxTree(tDef)
                End If
                Dim pDef As PropertyDefinition = TryCast(item.Tag, PropertyDefinition)
                If pDef IsNot Nothing Then
                    tree = manager.DecompilePropertyAsSyntaxTree(pDef)
                End If
                Dim fDef As FieldDefinition = TryCast(item.Tag, FieldDefinition)
                If fDef IsNot Nothing Then
                    tree = manager.DecompileFieldAsSyntaxTree(fDef)
                End If
                Dim eDef As EventDefinition = TryCast(item.Tag, EventDefinition)
                If eDef IsNot Nothing Then
                    tree = manager.DecompileEventAsSyntaxTree(eDef)
                End If
                txtCode.Text = tree.GetRoot.NormalizeWhitespace().ToFullString
            End If
        End If
    End Sub

End Class
Imports System.IO
Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Interactivity
Imports Avalonia.Markup.Xaml
Imports Avalonia.Platform.Storage
Imports Microsoft.CodeAnalysis
Imports Mono.Cecil

Partial Public Class MainWindow
    Inherits Window

#Region "Components"
    Private WithEvents tvAssembly As TreeView
    Private WithEvents txtCode As TextBox
    Private WithEvents btnOpen As MenuItem
    Private WithEvents btnExit As MenuItem

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        AvaloniaXamlLoader.Load(Me)
        tvAssembly = FindControl(Of TreeView)("tvAssembly")
        txtCode = FindControl(Of TextBox)("txtCode")
        btnOpen = FindControl(Of MenuItem)("btnOpen")
        btnExit = FindControl(Of MenuItem)("btnExit")
    End Sub

#End Region

    Dim manager As AssemblyManager
    Dim path As String = "C:\CodingCool\Code\Projects\CecilTesting\VBTest\bin\Debug\net8.0\VBTest.dll"

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not File.Exists(path) Then Return
        manager = New AssemblyManager
        tvAssembly.Items = manager.LoadAssembly(path)
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

    Private Async Sub btnOpen_Click(sender As Object, e As RoutedEventArgs) Handles btnOpen.Click
        Dim files As IStorageFile() = (Await StorageProvider.OpenFilePickerAsync(New Avalonia.Platform.Storage.FilePickerOpenOptions() With {.AllowMultiple = False, .Title = "Select assembly"})).ToArray
        If files.Any Then
            path = files(0).Path.AbsolutePath
            manager = New AssemblyManager
            tvAssembly.Items = manager.LoadAssembly(path)
        End If
    End Sub

    Private Sub btnExit_Click(sender As Object, e As RoutedEventArgs) Handles btnExit.Click
        Close()
    End Sub
End Class
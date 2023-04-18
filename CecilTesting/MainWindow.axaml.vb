Imports System.IO
Imports Avalonia.Controls
Imports Avalonia.Input
Imports Avalonia.Interactivity
Imports Avalonia.Markup.Xaml
Imports Avalonia.Platform.Storage
Imports AvaloniaEdit
Imports AvaloniaEdit.Rendering
Imports AvaloniaEdit.TextMate
Imports Microsoft.CodeAnalysis
Imports Mono.Cecil
Imports TextMateSharp.Grammars
Imports Pair = System.Collections.Generic.KeyValuePair(Of Integer, Avalonia.Controls.Control)

Partial Public Class MainWindow
    Inherits Window

#Region "Components"
    Private WithEvents tvAssembly As TreeView
    Private WithEvents btnOpen As MenuItem
    Private WithEvents btnExit As MenuItem
    Private WithEvents txtCode As TextEditor
    Private _textMateInstallation As TextMate.Installation
    Private _generator As New ElementGenerator()
    Private _registryOptions As RegistryOptions
    Private _currentTheme As Integer = CInt(ThemeName.DarkPlus)

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        AvaloniaXamlLoader.Load(Me)
        tvAssembly = FindControl(Of TreeView)("tvAssembly")
        btnOpen = FindControl(Of MenuItem)("btnOpen")
        btnExit = FindControl(Of MenuItem)("btnExit")

        txtCode = Me.FindControl(Of TextEditor)("txtCode")
        txtCode.HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Visible
        txtCode.Background = Avalonia.Media.Brushes.Transparent
        txtCode.ShowLineNumbers = True
        txtCode.ContextMenu = New ContextMenu With {
                .Items = New List(Of MenuItem) From {
                            New MenuItem With {
                            .Header = "Copy",
                            .InputGesture = New KeyGesture(Key.C, KeyModifiers.Control)
                            },
                            New MenuItem With {
                            .Header = "Paste",
                            .InputGesture = New KeyGesture(Key.V, KeyModifiers.Control)
                            },
                            New MenuItem With {
                            .Header = "Cut",
                            .InputGesture = New KeyGesture(Key.X, KeyModifiers.Control)
                            }
                }
        }
        txtCode.TextArea.Background = Me.Background
        txtCode.Options.ShowBoxForControlCharacters = True
        txtCode.Options.ColumnRulerPositions = {80, 100}.ToList
        txtCode.TextArea.RightClickMovesCaret = True
        txtCode.TextArea.TextView.ElementGenerators.Add(_generator)
        _registryOptions = New RegistryOptions(CType(_currentTheme, ThemeName))
        _textMateInstallation = txtCode.InstallTextMate(_registryOptions)
        Dim vbLanguage As Language = _registryOptions.GetLanguageByExtension(".vb")
        Dim scopeName As String = _registryOptions.GetScopeByLanguageId(vbLanguage.Id)
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(vbLanguage.Id))
        [AddHandler](PointerWheelChangedEvent, Sub(o, i)
                                                   If i.KeyModifiers <> KeyModifiers.Control Then Return
                                                   If i.Delta.Y > 0 Then
                                                       txtCode.FontSize += 1
                                                   Else
                                                       txtCode.FontSize = If(txtCode.FontSize > 1, txtCode.FontSize - 1, 1)
                                                   End If
                                               End Sub, RoutingStrategies.Bubble, True)
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

Public Class ElementGenerator
    Inherits VisualLineElementGenerator
    Implements IComparer(Of Pair)
    Public controls As List(Of Pair) = New List(Of Pair)()

    ''' <summary>
    ''' Gets the first interested offset using binary search
    ''' </summary>
    ''' <returns>The first interested offset.</returns>
    ''' <param name="startOffset">Start offset.</param>
    Public Overrides Function GetFirstInterestedOffset(startOffset As Integer) As Integer
        Dim pos As Integer = controls.BinarySearch(New Pair(startOffset, Nothing), Me)
        If pos < 0 Then pos = Not pos
        If pos < controls.Count Then
            Return controls(pos).Key
        Else
            Return -1
        End If
    End Function

    Public Overrides Function ConstructElement(offset As Integer) As VisualLineElement
        Dim pos As Integer = controls.BinarySearch(New Pair(offset, Nothing), Me)
        If pos >= 0 Then
            Return New InlineObjectElement(0, controls(pos).Value)
        Else
            Return Nothing
        End If
    End Function

    Private Function Compare(x As Pair, y As Pair) As Integer Implements IComparer(Of Pair).Compare
        Return x.Key.CompareTo(y.Key)
    End Function
End Class
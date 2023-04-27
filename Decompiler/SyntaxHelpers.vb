Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Public Class SyntaxHelpers
    Public Shared Function MergeNamespaces(tree As SyntaxTree) As SyntaxTree
        Dim namespaces As New Dictionary(Of String, List(Of NamespaceBlockSyntax))
        For Each nameSpaceBlock As NamespaceBlockSyntax In tree.GetRoot.DescendantNodes.OfType(Of NamespaceBlockSyntax)
            Dim name As String = nameSpaceBlock.NamespaceStatement.Name.ToString
            If namespaces.ContainsKey(name) Then
                namespaces(name).Add(nameSpaceBlock)
            Else
                namespaces.Add(name, {nameSpaceBlock}.ToList)
            End If
        Next
        Dim results As New List(Of NamespaceBlockSyntax)
        For Each p As KeyValuePair(Of String, List(Of NamespaceBlockSyntax)) In namespaces
            Dim nameSpaceBlock As NamespaceBlockSyntax = p.Value.First
            p.Value.Remove(nameSpaceBlock)
            For Each block As NamespaceBlockSyntax In p.Value
                nameSpaceBlock = nameSpaceBlock.AddMembers(block.Members.ToArray)
            Next
            results.Add(nameSpaceBlock)
        Next
        Dim root As CompilationUnitSyntax = tree.GetCompilationUnitRoot
        root = root.RemoveNodes(root.Members, SyntaxRemoveOptions.KeepNoTrivia).AddMembers(results.ToArray)
        Return SyntaxFactory.SyntaxTree(root)
    End Function
End Class

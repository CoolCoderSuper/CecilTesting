Public Class AssemblyItem
    Public Property Key As String
    Public Property Text As String
    Public Property Children As New List(Of AssemblyItem)
    Public Property Tag As Object
End Class
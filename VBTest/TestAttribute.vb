<AttributeUsage(AttributeTargets.Method)>
Public Class TestAttribute
    Inherits Attribute
    Public Sub New(description As String, age As Integer, gender As Gender)
    End Sub

    Public Property Other As String
End Class

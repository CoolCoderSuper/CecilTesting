Imports System.Timers

Public Class Test(Of T)

    <Test("Hello", 10, Gender.Male, Other:="Bye")>
    <TimersDescription("Hello")>
    Public Sub TestAttribute()
    End Sub

    Public Sub TestGenericArguments(Of T)(l As List(Of T), l1 As List(Of List(Of T)))
    End Sub

    Public Function TestGenericParameters(l As List(Of Integer), l1 As List(Of List(Of String))) As List(Of List(Of List(Of Integer)))
        Return Nothing
    End Function

    <Obsolete()>
    Public Property TestProperty As Integer
    Public Property TestParam(str As String) As Integer
        Get

        End Get
        Set

        End Set
    End Property

    Const Thing As String = "sdfsd"
End Class

Public Module TestMod
    Public Sub TestInModule()
    End Sub
End Module
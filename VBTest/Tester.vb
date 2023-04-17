Public Class Tester
    Implements ITester, IDisposable

    Public Property TestProp As String Implements ITester.TestProp

    Public Sub Test() Implements ITester.Test1
        Throw New NotImplementedException()
    End Sub

    Private Sub Test2(t As Tester2)
        Throw New NotImplementedException()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Event TestEvent(h As String) Implements ITester.TestEvent

    Public Sub Test(h As String) Implements ITester.Test
        Throw New NotImplementedException()
    End Sub

    Private Class Tester2
        Implements ITester

        Public Property TestProp As String Implements ITester.TestProp

        Public Event TestEvent(h As String) Implements ITester.TestEvent

        Public Sub Test() Implements ITester.Test1
            Throw New NotImplementedException()
        End Sub

        Public Sub Test(h As String) Implements ITester.Test
            Throw New NotImplementedException()
        End Sub
    End Class
End Class
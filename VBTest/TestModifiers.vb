Public Class TestModifiers
    Inherits ModifiersTestBase

    Public Sub TestPublic()
    End Sub

    Private Sub TestPrivate()
    End Sub

    Friend Sub TestFriend()
    End Sub

    Shared Sub TestShared()
    End Sub

    Public Async Sub TestAsync()
    End Sub

    'N\A
    Public Iterator Function TestIterator() As IEnumerable(Of Integer)
        Yield 1
        Yield 2
        Yield 3
    End Function

    Protected Sub TestProtected()
    End Sub

    Protected Friend Sub TestProtectedFriend()
    End Sub

    Private Protected Sub TestPrivateProtected()
    End Sub

    'N\A
    Public Shadows Sub TestShadowed()
    End Sub

    Public NotOverridable Overrides Sub TestOverridable()
    End Sub

End Class

Public Class ModifiersTestBase
    Public Sub TestShadowed()
    End Sub

    Public Overridable Sub TestOverridable()
    End Sub

End Class
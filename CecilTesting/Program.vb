Imports Avalonia

Module Program
    Sub Main(args As String())
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)
    End Sub

    Public Function BuildAvaloniaApp() As AppBuilder
        Return AppBuilder.Configure(Of App)().UsePlatformDetect().LogToTrace()
    End Function


End Module
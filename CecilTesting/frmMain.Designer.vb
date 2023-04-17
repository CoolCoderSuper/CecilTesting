<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        SplitContainer1 = New SplitContainer()
        tvClasses = New TreeView()
        tecCode = New ICSharpCode.TextEditor.TextEditorControl()
        MenuStrip1 = New MenuStrip()
        btnReload = New ToolStripMenuItem()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New Point(0, 24)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(tvClasses)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(tecCode)
        SplitContainer1.Size = New Size(970, 564)
        SplitContainer1.SplitterDistance = 323
        SplitContainer1.TabIndex = 0
        ' 
        ' tvClasses
        ' 
        tvClasses.Dock = DockStyle.Fill
        tvClasses.Location = New Point(0, 0)
        tvClasses.Name = "tvClasses"
        tvClasses.Size = New Size(323, 564)
        tvClasses.TabIndex = 0
        ' 
        ' tecCode
        ' 
        tecCode.Dock = DockStyle.Fill
        tecCode.IsReadOnly = False
        tecCode.Location = New Point(0, 0)
        tecCode.Name = "tecCode"
        tecCode.Size = New Size(643, 564)
        tecCode.TabIndex = 0
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {btnReload})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(970, 24)
        MenuStrip1.TabIndex = 1
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' btnReload
        ' 
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(55, 20)
        btnReload.Text = "Reload"
        ' 
        ' frmMain
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(970, 588)
        Controls.Add(SplitContainer1)
        Controls.Add(MenuStrip1)
        MainMenuStrip = MenuStrip1
        Name = "frmMain"
        Text = "Decompiler"
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents tvClasses As TreeView
    Friend WithEvents tecCode As ICSharpCode.TextEditor.TextEditorControl
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents btnReload As ToolStripMenuItem
End Class

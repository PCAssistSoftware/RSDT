<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
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
        btnStart = New Button()
        tbError = New TextBox()
        Label1 = New Label()
        tbInfo = New TextBox()
        Button3 = New Button()
        GridControl1 = New DevExpress.XtraGrid.GridControl()
        GridView1 = New DevExpress.XtraGrid.Views.Grid.GridView()
        btnCancel = New Button()
        lblInvocationStateInfo = New Label()
        cbShowToast = New CheckBox()
        cbCheckProcess = New CheckBox()
        Panel1 = New Panel()
        GroupBox4 = New GroupBox()
        Label8 = New Label()
        Label7 = New Label()
        tbSoftwareDisplayVersion = New TextBox()
        Label6 = New Label()
        tbSoftwareDisplayName = New TextBox()
        GroupBox3 = New GroupBox()
        Label9 = New Label()
        btnBrowseMSI = New Button()
        tbSourceFilePath = New TextBox()
        Label10 = New Label()
        tbInstallerFileName = New TextBox()
        GroupBox2 = New GroupBox()
        Label5 = New Label()
        tbProcessName = New TextBox()
        GroupBox1 = New GroupBox()
        Label4 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        tbToastBody = New TextBox()
        tbToastTitle = New TextBox()
        cbUseAD = New CheckBox()
        CType(GridControl1, ComponentModel.ISupportInitialize).BeginInit()
        CType(GridView1, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        GroupBox4.SuspendLayout()
        GroupBox3.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnStart
        ' 
        btnStart.Location = New Point(321, 41)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(75, 23)
        btnStart.TabIndex = 0
        btnStart.Text = "Start"
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' tbError
        ' 
        tbError.Location = New Point(321, 489)
        tbError.Multiline = True
        tbError.Name = "tbError"
        tbError.ScrollBars = ScrollBars.Vertical
        tbError.Size = New Size(894, 124)
        tbError.TabIndex = 1
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(321, 471)
        Label1.Name = "Label1"
        Label1.Size = New Size(40, 15)
        Label1.TabIndex = 4
        Label1.Text = "Errors:"
        ' 
        ' tbInfo
        ' 
        tbInfo.Location = New Point(321, 75)
        tbInfo.Multiline = True
        tbInfo.Name = "tbInfo"
        tbInfo.ScrollBars = ScrollBars.Vertical
        tbInfo.Size = New Size(894, 85)
        tbInfo.TabIndex = 6
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(1093, 41)
        Button3.Name = "Button3"
        Button3.Size = New Size(75, 23)
        Button3.TabIndex = 7
        Button3.Text = "Test Code"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' GridControl1
        ' 
        GridControl1.Location = New Point(321, 166)
        GridControl1.MainView = GridView1
        GridControl1.Name = "GridControl1"
        GridControl1.Size = New Size(894, 302)
        GridControl1.TabIndex = 8
        GridControl1.ViewCollection.AddRange(New DevExpress.XtraGrid.Views.Base.BaseView() {GridView1})
        ' 
        ' GridView1
        ' 
        GridView1.GridControl = GridControl1
        GridView1.Name = "GridView1"
        ' 
        ' btnCancel
        ' 
        btnCancel.Location = New Point(436, 41)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(75, 23)
        btnCancel.TabIndex = 9
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' lblInvocationStateInfo
        ' 
        lblInvocationStateInfo.AutoSize = True
        lblInvocationStateInfo.Location = New Point(598, 45)
        lblInvocationStateInfo.Name = "lblInvocationStateInfo"
        lblInvocationStateInfo.Size = New Size(123, 15)
        lblInvocationStateInfo.TabIndex = 10
        lblInvocationStateInfo.Text = "lblInvocationStateInfo"
        ' 
        ' cbShowToast
        ' 
        cbShowToast.AutoSize = True
        cbShowToast.Location = New Point(19, 25)
        cbShowToast.Name = "cbShowToast"
        cbShowToast.Size = New Size(85, 19)
        cbShowToast.TabIndex = 11
        cbShowToast.Text = "Show Toast"
        cbShowToast.UseVisualStyleBackColor = True
        ' 
        ' cbCheckProcess
        ' 
        cbCheckProcess.AutoSize = True
        cbCheckProcess.Location = New Point(19, 25)
        cbCheckProcess.Name = "cbCheckProcess"
        cbCheckProcess.Size = New Size(102, 19)
        cbCheckProcess.TabIndex = 12
        cbCheckProcess.Text = "Check Process"
        cbCheckProcess.UseVisualStyleBackColor = True
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(GroupBox4)
        Panel1.Controls.Add(GroupBox3)
        Panel1.Controls.Add(GroupBox2)
        Panel1.Controls.Add(GroupBox1)
        Panel1.Location = New Point(12, 12)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(303, 599)
        Panel1.TabIndex = 13
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(Label8)
        GroupBox4.Controls.Add(Label7)
        GroupBox4.Controls.Add(tbSoftwareDisplayVersion)
        GroupBox4.Controls.Add(Label6)
        GroupBox4.Controls.Add(tbSoftwareDisplayName)
        GroupBox4.Location = New Point(3, 136)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(297, 119)
        GroupBox4.TabIndex = 3
        GroupBox4.TabStop = False
        GroupBox4.Text = "Software Information - to check if already installed"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(11, 92)
        Label8.Name = "Label8"
        Label8.Size = New Size(254, 15)
        Label8.TabIndex = 23
        Label8.Text = "TIP: As they appear in Control Panel or Registry"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(11, 64)
        Label7.Name = "Label7"
        Label7.Size = New Size(86, 15)
        Label7.TabIndex = 22
        Label7.Text = "DisplayVersion:"
        ' 
        ' tbSoftwareDisplayVersion
        ' 
        tbSoftwareDisplayVersion.Location = New Point(102, 61)
        tbSoftwareDisplayVersion.Name = "tbSoftwareDisplayVersion"
        tbSoftwareDisplayVersion.Size = New Size(175, 23)
        tbSoftwareDisplayVersion.TabIndex = 21
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(11, 30)
        Label6.Name = "Label6"
        Label6.Size = New Size(80, 15)
        Label6.TabIndex = 20
        Label6.Text = "DisplayName:"
        ' 
        ' tbSoftwareDisplayName
        ' 
        tbSoftwareDisplayName.Location = New Point(102, 27)
        tbSoftwareDisplayName.Name = "tbSoftwareDisplayName"
        tbSoftwareDisplayName.Size = New Size(175, 23)
        tbSoftwareDisplayName.TabIndex = 19
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(Label9)
        GroupBox3.Controls.Add(btnBrowseMSI)
        GroupBox3.Controls.Add(tbSourceFilePath)
        GroupBox3.Controls.Add(Label10)
        GroupBox3.Controls.Add(tbInstallerFileName)
        GroupBox3.Location = New Point(3, 6)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(297, 124)
        GroupBox3.TabIndex = 2
        GroupBox3.TabStop = False
        GroupBox3.Text = "MSI - Select MSI to deploy"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(11, 92)
        Label9.Name = "Label9"
        Label9.Size = New Size(34, 15)
        Label9.TabIndex = 27
        Label9.Text = "Path:"
        ' 
        ' btnBrowseMSI
        ' 
        btnBrowseMSI.Location = New Point(11, 22)
        btnBrowseMSI.Name = "btnBrowseMSI"
        btnBrowseMSI.Size = New Size(70, 23)
        btnBrowseMSI.TabIndex = 0
        btnBrowseMSI.Text = "Browse"
        btnBrowseMSI.UseVisualStyleBackColor = True
        ' 
        ' tbSourceFilePath
        ' 
        tbSourceFilePath.Location = New Point(102, 89)
        tbSourceFilePath.Name = "tbSourceFilePath"
        tbSourceFilePath.ReadOnly = True
        tbSourceFilePath.Size = New Size(175, 23)
        tbSourceFilePath.TabIndex = 26
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Location = New Point(11, 58)
        Label10.Name = "Label10"
        Label10.Size = New Size(58, 15)
        Label10.TabIndex = 25
        Label10.Text = "Filename:"
        ' 
        ' tbInstallerFileName
        ' 
        tbInstallerFileName.Location = New Point(102, 55)
        tbInstallerFileName.Name = "tbInstallerFileName"
        tbInstallerFileName.ReadOnly = True
        tbInstallerFileName.Size = New Size(175, 23)
        tbInstallerFileName.TabIndex = 24
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(Label5)
        GroupBox2.Controls.Add(tbProcessName)
        GroupBox2.Controls.Add(cbCheckProcess)
        GroupBox2.Location = New Point(3, 261)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(297, 100)
        GroupBox2.TabIndex = 1
        GroupBox2.TabStop = False
        GroupBox2.Text = "Process Information - check if a process is running"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(11, 62)
        Label5.Name = "Label5"
        Label5.Size = New Size(85, 15)
        Label5.TabIndex = 18
        Label5.Text = "Process Name:"
        ' 
        ' tbProcessName
        ' 
        tbProcessName.Location = New Point(102, 59)
        tbProcessName.Name = "tbProcessName"
        tbProcessName.Size = New Size(175, 23)
        tbProcessName.TabIndex = 17
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(Label4)
        GroupBox1.Controls.Add(Label3)
        GroupBox1.Controls.Add(Label2)
        GroupBox1.Controls.Add(tbToastBody)
        GroupBox1.Controls.Add(tbToastTitle)
        GroupBox1.Controls.Add(cbShowToast)
        GroupBox1.Location = New Point(3, 367)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(297, 210)
        GroupBox1.TabIndex = 0
        GroupBox1.TabStop = False
        GroupBox1.Text = "Toast - to display to user when MSI installed"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(67, 175)
        Label4.Name = "Label4"
        Label4.Size = New Size(126, 15)
        Label4.TabIndex = 16
        Label4.Text = "TIP: Use `n for new line"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(11, 97)
        Label3.Name = "Label3"
        Label3.Size = New Size(56, 15)
        Label3.TabIndex = 15
        Label3.Text = "Message:"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(11, 58)
        Label2.Name = "Label2"
        Label2.Size = New Size(32, 15)
        Label2.TabIndex = 14
        Label2.Text = "Title:"
        ' 
        ' tbToastBody
        ' 
        tbToastBody.Location = New Point(67, 94)
        tbToastBody.Multiline = True
        tbToastBody.Name = "tbToastBody"
        tbToastBody.Size = New Size(210, 69)
        tbToastBody.TabIndex = 13
        ' 
        ' tbToastTitle
        ' 
        tbToastTitle.Location = New Point(67, 55)
        tbToastTitle.Name = "tbToastTitle"
        tbToastTitle.Size = New Size(210, 23)
        tbToastTitle.TabIndex = 12
        ' 
        ' cbUseAD
        ' 
        cbUseAD.AutoSize = True
        cbUseAD.Location = New Point(321, 12)
        cbUseAD.Name = "cbUseAD"
        cbUseAD.Size = New Size(842, 19)
        cbUseAD.TabIndex = 14
        cbUseAD.Text = "Scan AD for Computers (or use list of computers - N.B. list currently hard coded for testing, but can be added to form by way of list box or similar in future)"
        cbUseAD.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1227, 623)
        Controls.Add(cbUseAD)
        Controls.Add(Panel1)
        Controls.Add(lblInvocationStateInfo)
        Controls.Add(btnCancel)
        Controls.Add(GridControl1)
        Controls.Add(Button3)
        Controls.Add(tbInfo)
        Controls.Add(Label1)
        Controls.Add(tbError)
        Controls.Add(btnStart)
        Name = "Form1"
        Text = "Remote Software Deployment Tool"
        CType(GridControl1, ComponentModel.ISupportInitialize).EndInit()
        CType(GridView1, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents btnStart As Button
    Friend WithEvents tbError As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents tbInfo As TextBox
    Friend WithEvents Button3 As Button
    Friend WithEvents GridControl1 As DevExpress.XtraGrid.GridControl
    Friend WithEvents GridView1 As DevExpress.XtraGrid.Views.Grid.GridView
    Friend WithEvents btnCancel As Button
    Friend WithEvents lblInvocationStateInfo As Label
    Friend WithEvents cbShowToast As CheckBox
    Friend WithEvents cbCheckProcess As CheckBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents tbToastBody As TextBox
    Friend WithEvents tbToastTitle As TextBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents tbProcessName As TextBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents Label6 As Label
    Friend WithEvents tbSoftwareDisplayName As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents tbSoftwareDisplayVersion As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents btnBrowseMSI As Button
    Friend WithEvents tbSourceFilePath As TextBox
    Friend WithEvents Label10 As Label
    Friend WithEvents tbInstallerFileName As TextBox
    Friend WithEvents cbUseAD As CheckBox
End Class

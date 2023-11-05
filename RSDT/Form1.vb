Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Management
Imports System.Management.Automation
Imports System.Management.Automation.Runspaces
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports DevExpress
Imports DevExpress.Utils
Imports DevExpress.XtraGrid
Imports DevExpress.XtraGrid.Columns

Public Class Form1

    Dim stpw As New Stopwatch

    Dim ps As PowerShell

    ' cancellation variables
    Dim tokenSource As New CancellationTokenSource()
    Dim token As CancellationToken = tokenSource.Token

    ' result variables
    Dim online As New List(Of String)
    Dim offline As New List(Of String)

    Dim remoteworking As New List(Of String)
    Dim remotenotworking As New List(Of String)

    Dim softwarealreadyinstalled As New List(Of String)
    Dim softwarenotalreadyinstalled As New List(Of String)

    Dim processrunning As New List(Of String)
    Dim processnotrunning As New List(Of String)

    Dim installsuccess As New List(Of String)
    Dim installfailed As New List(Of String)

    Dim toastsuccess As New List(Of String)
    Dim toastfailed As New List(Of String)

    ' grid data source
    Dim _TestResults As New BindingList(Of TestResults)

    ' settings variables
    Dim softwaredisplayname
    Dim softwaredisplayversion
    Dim softwareregpath64 = "'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'"
    Dim softwareregpath32 = "'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'"

    Dim processtocheck As String

    Dim installerfilename
    Dim sourcefilepath
    Dim destinationfilepath = "c$\windows\temp\" ' N.B.remote destination file path needs to use c$ not c:
    Dim destinationfilepathforinstall ' was Dim destinationfilepathforinstall = destinationfilepath.Replace("$", ":") + installerfilename  - but now moved to btnSelectMSI so created after user selects MSI file

    Dim toasttitle As String
    Dim toastbody As String

    ' booleans
    Dim checkprocess As Boolean = True
    Dim showtoast As Boolean = False

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' add grid columns
        Dim col1 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Computer)) ' was originally GridView1.Columns.AddField("Computer")
        col1.Caption = "Computer"
        col1.Width = 150
        GridView1.Columns.Add(col1)
        col1.Visible = True

        Dim col2 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test1))
        col2.Caption = "Test Connection"
        col2.Width = 150
        GridView1.Columns.Add(col2)
        col2.Visible = True

        Dim col3 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test2))
        col3.Caption = "Check Remoting"
        col3.Width = 150
        GridView1.Columns.Add(col3)
        col3.Visible = True

        Dim col4 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test3))
        col4.Caption = "Check Installed"
        col4.Width = 150
        GridView1.Columns.Add(col4)
        col4.Visible = True

        Dim col5 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test4))
        col5.Caption = "Check Process"
        col5.Width = 150
        GridView1.Columns.Add(col5)
        col5.Visible = True

        Dim col6 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test5))
        col6.Caption = "Deploy Software"
        col6.Width = 150
        GridView1.Columns.Add(col6)
        col6.Visible = True

        Dim col7 As GridColumn = GridView1.Columns.AddField(NameOf(TestResults.Test6))
        col7.Caption = "Show Toast"
        col7.Width = 150
        GridView1.Columns.Add(col7)
        col7.Visible = True

        ' configure grid
        GridView1.OptionsCustomization.AllowGroup = False
        GridView1.GroupPanelText = " "
        GridView1.OptionsSelection.EnableAppearanceFocusedRow = False
        GridView1.OptionsBehavior.Editable = False
        GridView1.FocusRectStyle = XtraGrid.Views.Grid.DrawFocusRectStyle.None
        GridView1.OptionsView.ShowFooter = True ' so can see summaries
        GridView1.Appearance.FooterPanel.Options.UseTextOptions = True
        GridView1.Appearance.FooterPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near


        ' configure grid formatting

        Using appearance As New AppearanceObjectEx
            appearance.BackColor = Color.LightGreen
            appearance.ForeColor = Color.Black
            AddFormatRule(col2, appearance, "Contains([Test1], 'Online')")
            AddFormatRule(col3, appearance, "Contains([Test2], 'Remoting Working')")
            AddFormatRule(col4, appearance, "Contains([Test3], 'Software Installed')")
            AddFormatRule(col5, appearance, "Contains([Test4], 'Process Not Running')")
            AddFormatRule(col6, appearance, "Contains([Test5], 'Deploy Success')")
            AddFormatRule(col7, appearance, "Contains([Test6], 'Toast Success')")
        End Using

        Using appearance As New AppearanceObjectEx
            appearance.BackColor = Color.Red
            appearance.ForeColor = Color.White
            AddFormatRule(col2, appearance, "Contains([Test1], 'Offline')")
            AddFormatRule(col3, appearance, "StartsWith([Test2], 'Remoting Not Working')")
            AddFormatRule(col4, appearance, "Contains([Test3], 'Software Not Installed')")
            AddFormatRule(col5, appearance, "Contains([Test4], 'Process Running')")
            AddFormatRule(col6, appearance, "StartsWith([Test5], 'Deploy Failed')")
            AddFormatRule(col6, appearance, "StartsWith([Test5], 'Already Installed')")
            AddFormatRule(col6, appearance, "StartsWith([Test5], 'Fatal Error')")
            AddFormatRule(col6, appearance, "StartsWith([Test5], 'Rebooting')")
            AddFormatRule(col6, appearance, "StartsWith([Test5], 'Needs Reboot')")
            AddFormatRule(col7, appearance, "Contains([Test6], 'Toast Failed')")
        End Using

        ' configure form
        btnCancel.Enabled = False


        ' set example values on form for testing

        tbProcessName.Text = "notepad"

        tbToastTitle.Text = "IT Department"
        tbToastBody.Text = "Update has been completed `nYou may now use xxxxxx again"

        tbSoftwareDisplayName.Text = "Disk Cleanup Service"
        tbSoftwareDisplayVersion.Text = "2.2.0"

    End Sub

    Sub AddFormatRule(column As GridColumn, appearance As AppearanceObjectEx, expression As String)
        Dim rule As New GridFormatRule()
        Dim ruleExpression As New DevExpress.XtraEditors.FormatConditionRuleExpression()
        rule.Column = column
        rule.ApplyToRow = False
        ruleExpression.Appearance.Assign(appearance)
        ruleExpression.Expression = expression
        ruleExpression.Appearance.Options.HighPriority = True
        rule.Rule = ruleExpression
        GridView1.FormatRules.Add(rule)
    End Sub


    Private Async Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click

        showtoast = cbShowToast.Checked

        If showtoast = True Then
            toasttitle = tbToastTitle.Text
            toastbody = tbToastBody.Text
        End If

        checkprocess = cbCheckProcess.Checked

        If checkprocess = True Then

            processtocheck = tbProcessName.Text

        End If


        softwaredisplayname = tbSoftwareDisplayName.Text
        softwaredisplayversion = tbSoftwareDisplayVersion.Text

        btnStart.Enabled = False
        btnCancel.Enabled = True

        ' Clear existing variables and text boxes                         
        Clear()

        ' Start Timer
        stpw.Start()


        ' Get Computers from AD or use Computer List - currently hardcoded for testing but future will be added via form and shown in listview or similar

        Dim computerlist

        If cbUseAD.Checked = True Then

            computerlist = Await GetComputers()

            ' Add computers to grid
            For Each computer As PSObject In computerlist
                If Not _TestResults.Any(Function(p) p.Computer = computer.ToString) Then
                    _TestResults.Add(New TestResults() With {.Computer = computer.ToString})
                End If
            Next

        Else

            computerlist = New List(Of String)({"PC-1", "PC-2", "PC-3"})

            ' Add computers to grid
            For Each computer As String In computerlist
                If Not _TestResults.Any(Function(p) p.Computer = computer.ToString) Then
                    _TestResults.Add(New TestResults() With {.Computer = computer.ToString})
                End If
            Next

        End If


        ' Set data source to my results list
        GridControl1.DataSource = _TestResults



        ' Test Connection - returns 'online' and 'offline' - 'online' passes to next stage unless count is zero
        If computerlist.Count <> 0 Then
            Await Task.Run(Sub() CheckTCPPort(computerlist))
            ' OR
            ' online = Await TestConnection(computerlist)
        Else
            Exit Sub
        End If


        ' Check Remoting - using 'online' from last stage - returns 'remoteworking' and 'remotenotworking' -  'remoteworking' passes to next stage unless count is zero
        If online.Count <> 0 Then
            Await CheckRemoting(online)
        Else
            Exit Sub
        End If


        ' Check if product already installed - using 'remoteworking' from last stage - returns 'softwarenotalreadyinstalled' and 'softwarealreadyinstalled' -  'softwarenotalreadyinstalled' passes to next stage unless count is zero
        If remoteworking.Count <> 0 Then
            Await CheckSoftwareInstalled(remoteworking)
        Else
            Exit Sub
        End If



        ' Check if process is running (optional) - using 'softwarenotalreadyinstalled' from last stage - returns 'processnotrunning' and 'processrunning' -  'processnotrunning' passes to next stage unless count is zero
        If checkprocess = True Then
            If softwarenotalreadyinstalled.Count <> 0 Then
                Await CheckProcessNotRunning(softwarenotalreadyinstalled)

                If processnotrunning.Count <> 0 Then

                    ' Copy file and install MSI / EXE -  using 'processnotrunning' - returns 'installsuccess' and 'installfailed' - 'installsuccess' passes to next stage unless count is zero
                    ' Ask for confirmation
                    Dim result As DialogResult = MessageBox.Show(softwaredisplayname & " Is Not installed on " & processnotrunning.Count & " computer(s)." & vbCrLf & "Do you wish to proceed with installation?", "Install Software?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    If result = DialogResult.Yes Then

                        Await DeploySoftware(processnotrunning)

                        ' Show toast notification (optional) - using 'installsuccess' from last stage - returns 'toastsuccess' and 'toastfailed' 
                        If showtoast = True Then
                            If installsuccess.Count <> 0 Then
                                Await ShowToastNotification(installsuccess)
                            Else
                                Exit Sub
                            End If
                        End If

                    End If

                Else
                    Exit Sub
                End If

            Else
                Exit Sub
            End If
        Else

            If softwarenotalreadyinstalled.Count <> 0 Then

                ' Copy file and install MSI / EXE - using 'softwarenotalreadyinstalled' - returns 'installsuccess' and 'installfailed' - 'installsuccess' passes to next stage unless count is zero
                ' Ask for confirmation
                Dim result As DialogResult = MessageBox.Show(softwaredisplayname & " Is Not installed on " & softwarenotalreadyinstalled.Count & " computer(s)." & vbCrLf & "Do you wish to proceed with installation?", "Install Software?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then

                    Await DeploySoftware(softwarenotalreadyinstalled)

                    ' Show toast notification (optional) - using 'installsuccess' from last stage - returns 'toastsuccess' and 'toastfailed' 
                    If showtoast = True Then
                        If installsuccess.Count <> 0 Then
                            Await ShowToastNotification(installsuccess)
                        Else
                            Exit Sub
                        End If
                    End If

                End If

            Else
                Exit Sub
            End If

        End If



        ' Stop Timer

        stpw.Stop()

        btnStart.Enabled = True
        btnCancel.Enabled = False


    End Sub

    Public Sub Clear()

        online.Clear()
        offline.Clear()
        remoteworking.Clear()
        remotenotworking.Clear()
        softwarealreadyinstalled.Clear()
        softwarenotalreadyinstalled.Clear()
        processrunning.Clear()
        processnotrunning.Clear()
        installsuccess.Clear()
        installfailed.Clear()
        toastsuccess.Clear()
        toastfailed.Clear()

        stpw.Reset()

        tbError.Clear()
        tbInfo.Clear()
        'tbOutput.Clear()

        _TestResults.Clear()

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

        tokenSource.Cancel()

    End Sub


    Private Sub btnBrowseMSI_Click(sender As Object, e As EventArgs) Handles btnBrowseMSI.Click

        Dim openFileDialog1 As New OpenFileDialog()

        openFileDialog1.InitialDirectory = "c:\"
        openFileDialog1.Filter = "MSI files (*.MSI)|*.MSI"
        openFileDialog1.RestoreDirectory = True

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            Dim selectedpath = openFileDialog1.FileName

            installerfilename = IO.Path.GetFileName(selectedpath)
            tbInstallerFileName.Text = installerfilename

            sourcefilepath = IO.Path.GetDirectoryName(selectedpath)
            tbSourceFilePath.Text = sourcefilepath

            destinationfilepathforinstall = destinationfilepath.Replace("$", ":") + installerfilename

        End If

    End Sub



#Region "Get Computers from AD"

    Public Async Function GetComputers() As Task(Of Object)

        Dim computerlist = Await Task.Run(Function() GetADComputers())

        UpdateTextBox(tbInfo, "AD Computers found:  " & computerlist.Count)

        ' show grid column summary
        UpdateColumnSummaries("Computer", "Computers: ", computerlist.Count)

        ' return list of computers for next stage
        Return computerlist

    End Function

    Public Function GetADComputers()

        Dim lastused = DateTime.Today.AddDays(-365)

        Dim ps As PowerShell = PowerShell.Create()
        Dim listofADcomputers = ps.AddCommand("Get-ADComputer").AddParameter("Filter", "OperatingSystem -notlike '*Server*' -and LastLogonDate -gt '" & lastused & "'").AddCommand("Select-Object").AddParameter("Expand", "name").Invoke
        ps.Dispose()

        Return listofADcomputers

    End Function


#End Region

#Region "Test Connection"

    Public Async Function TestConnection(ByVal listofcomputers) As Task

        ' Uses Test-Connection which uses WMI with Win32_PingStatus and returns StatusCode - these are explained here - https://docs.microsoft.com/en-gb/previous-versions/windows/desktop/wmipicmp/win32-pingstatus?redirectedfrom=MSDN
        ' Successfull results are handled by output.DataAdded and failures by ps.Streams.Error.DataAdded - with the exception of TimedOut, DestinationHostUnreachable and TtlExpired which get caught by output.DataAdded

        ps = PowerShell.Create()

        ps.AddCommand("Test-Connection").AddParameter("ComputerName", listofcomputers).AddParameter("Count", 1)

        Dim output As New PSDataCollection(Of PSObject)()

        AddHandler output.DataAdded, AddressOf Output_DataAdded_TestConnection

        AddHandler ps.Streams.Error.DataAdded, AddressOf Error_DataAdded_TestConnection

        AddHandler ps.InvocationStateChanged, AddressOf OnInvocationStateChanged

        Try
            Await InvokeAsync(ps, token, output)

        Catch ex As Exception
        End Try

        ps.Dispose()

        ' show grid column summary
        UpdateColumnSummaries("Test1", "Online: ", online.Count)
        UpdateColumnSummaries("Test1", "Offline: ", offline.Count)
        UpdateColumnSummaries("Test1", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Online: " & online.Count)
        UpdateTextBox(tbInfo, "Offline: " & offline.Count)
        UpdateTextBox(tbInfo, "")

        ' Sort grid
        GridView1.Columns("Test1").SortOrder = DevExpress.Data.ColumnSortOrder.Descending
        GridView1.FocusedRowHandle = 0

    End Function

    Public Function CheckTCPPort(computerlist)

        For Each r In computerlist

            If _CheckTCPPort(r.ToString, 5985, 400) = True Then

                ' add to list
                online.Add(r.ToString)

                'output for debug purposes only
                'UpdateTextBox(tbOutput, r.ToString & " - " & "Online")
                Debug.WriteLine(r.ToString & " - " & "Online")

                ' update grid
                Me.Invoke(Sub() UpdateRecord(r.ToString, NameOf(TestResults.Test1), "Online"))

            Else
                ' add to list
                offline.Add(r.ToString)

                'output for debug purposes only
                'UpdateTextBox(tbOutput, r.ToString & " - " & "Offline")
                Debug.WriteLine(r.ToString & " - " & "Offline")

                ' update grid
                Me.Invoke(Sub() UpdateRecord(r.ToString, NameOf(TestResults.Test1), "Offline"))

            End If

        Next

        ' show grid column summary
        UpdateColumnSummaries("Test1", "Online: ", online.Count)
        UpdateColumnSummaries("Test1", "Offline: ", offline.Count)
        UpdateColumnSummaries("Test1", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Online: " & online.Count)
        UpdateTextBox(tbInfo, "Offline: " & offline.Count)
        UpdateTextBox(tbInfo, "")

        ' Sort grid
        Me.Invoke(Sub() GridView1.Columns("Test1").SortOrder = DevExpress.Data.ColumnSortOrder.Descending)
        Me.Invoke(Sub() GridView1.FocusedRowHandle = 0)


    End Function

    ' from https://gist.github.com/elpatron68/257e4e2531fdb8729874
    Private Function _CheckTCPPort(ByVal sIPAdress As String, ByVal iPort As Integer, Optional ByVal iTimeout As Integer = 5000)
        Dim socket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        ' Connect using a timeout (default 5 seconds)
        Dim result As IAsyncResult = socket.BeginConnect(sIPAdress, iPort, Nothing, Nothing)
        Dim success As Boolean = result.AsyncWaitHandle.WaitOne(iTimeout, True)

        If Not success Then
            ' NOTE, MUST CLOSE THE SOCKET
            socket.Close()
            Return False
        End If
        Return True
    End Function

#End Region

#Region "Check Remoting"

    Public Async Function CheckRemoting(ByVal listofcomputers) As Task

        ps = PowerShell.Create()

        ps.AddCommand("Invoke-Command").AddParameter("ComputerName", listofcomputers).AddParameter("ScriptBlock", ScriptBlock.Create("1"))

        Dim output As New PSDataCollection(Of PSObject)()

        AddHandler output.DataAdded, AddressOf Output_DataAdded_CheckRemoting

        AddHandler ps.Streams.Error.DataAdded, AddressOf Error_DataAdded_CheckRemoting

        AddHandler ps.InvocationStateChanged, AddressOf OnInvocationStateChanged

        Try
            Await InvokeAsync(ps, token, output)

        Catch ex As Exception
        End Try

        ps.Dispose()

        ' show grid column summary
        UpdateColumnSummaries("Test2", "Remote Working: ", remoteworking.Count)
        UpdateColumnSummaries("Test2", "Remote Not Working: ", remotenotworking.Count)
        UpdateColumnSummaries("Test2", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Remote Working: " & remoteworking.Count)
        UpdateTextBox(tbInfo, "Remote Not Working: " & remotenotworking.Count)
        UpdateTextBox(tbInfo, "")

        ' Sort grid
        GridView1.Columns("Test2").SortOrder = DevExpress.Data.ColumnSortOrder.Descending
        GridView1.FocusedRowHandle = 0

    End Function

#End Region

#Region "Check if product already installed"

    Public Async Function CheckSoftwareInstalled(ByVal listofcomputers) As Task

        ps = PowerShell.Create()

        Dim command As New PSCommand()
        command.AddCommand("Invoke-Command")
        command.AddParameter("ComputerName", listofcomputers)
        If softwaredisplayversion <> "" Then
            command.AddParameter("ScriptBlock", ScriptBlock.Create("Get-ItemProperty " & softwareregpath64 & " | Where-Object { $_.DisplayName -eq '" & softwaredisplayname & "' -and $_.DisplayVersion -eq '" & softwaredisplayversion & "'}"))
        Else
            command.AddParameter("ScriptBlock", ScriptBlock.Create("Get-ItemProperty " & softwareregpath64 & " | Where-Object { $_.DisplayName -eq '" & softwaredisplayname & "'}"))
        End If
        ps.Commands = command

        Dim output As New PSDataCollection(Of PSObject)()

        AddHandler output.DataAdded, AddressOf Output_DataAdded_CheckInstalled

        AddHandler ps.Streams.Error.DataAdded, AddressOf Error_DataAdded_CheckInstalled ' TODO don't really need to handle this as we add softwarenotinstalled afterwards rather than through error event

        AddHandler ps.InvocationStateChanged, AddressOf OnInvocationStateChanged

        Try
            Await InvokeAsync(ps, token, output)
        Catch ex As Exception
        End Try

        ps.Dispose()

        ' create list of not installed
        softwarenotalreadyinstalled = remoteworking.Except(softwarealreadyinstalled).ToList()

        ' update grid for all notinstalled
        For Each ni In softwarenotalreadyinstalled
            Me.Invoke(Sub() UpdateRecord(ni, NameOf(TestResults.Test3), "Software Not Installed"))
        Next

        ' show grid column summary
        UpdateColumnSummaries("Test3", "Software Installed: ", softwarealreadyinstalled.Count)
        UpdateColumnSummaries("Test3", "Software Not Installed: ", softwarenotalreadyinstalled.Count)
        UpdateColumnSummaries("Test3", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Software Installed: " & softwarealreadyinstalled.Count)
        UpdateTextBox(tbInfo, "Software Not Installed: " & softwarenotalreadyinstalled.Count)
        UpdateTextBox(tbInfo, "")

        ' Sort grid
        GridView1.Columns("Test3").SortOrder = DevExpress.Data.ColumnSortOrder.Descending
        GridView1.FocusedRowHandle = 0

    End Function

#End Region

#Region "Check if process is running"

    Public Async Function CheckProcessNotRunning(ByVal listofcomputers) As Task

        ps = PowerShell.Create()

        Dim command As New PSCommand()
        command.AddCommand("Invoke-Command")
        command.AddParameter("ComputerName", listofcomputers)
        command.AddParameter("ScriptBlock", ScriptBlock.Create("Get-Process " & processtocheck & " -ErrorAction SilentlyContinue"))
        ps.Commands = command

        Dim output As New PSDataCollection(Of PSObject)()

        AddHandler output.DataAdded, AddressOf Output_DataAdded_CheckProcess

        AddHandler ps.Streams.Error.DataAdded, AddressOf Error_DataAdded_CheckProcess ' TODO don't really need to handle this as we add processnotrunning afterwards rather than through error event

        AddHandler ps.InvocationStateChanged, AddressOf OnInvocationStateChanged

        Try
            Await InvokeAsync(ps, token, output)
        Catch ex As Exception
        End Try

        ps.Dispose()

        ' create list of process not running
        processnotrunning = softwarenotalreadyinstalled.Except(processrunning).ToList()


        ' update grid for all processnotrunning
        For Each nr In processnotrunning
            Me.Invoke(Sub() UpdateRecord(nr, NameOf(TestResults.Test4), "Process Not Running"))
        Next

        ' show grid column summary
        UpdateColumnSummaries("Test4", "Process Running: ", processrunning.Count)
        UpdateColumnSummaries("Test4", "Process Not Running: ", processnotrunning.Count)
        UpdateColumnSummaries("Test4", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Process Running: " & processrunning.Count)
        UpdateTextBox(tbInfo, "Process Not Running: " & processnotrunning.Count)
        UpdateTextBox(tbInfo, "")

        ' Sort grid
        GridView1.Columns("Test4").SortOrder = DevExpress.Data.ColumnSortOrder.Descending
        GridView1.FocusedRowHandle = 0

    End Function


#End Region

#Region "Copy file and deploy MSI/EXE to those with process NOT running"

    Public Async Function DeploySoftware(ByVal listofcomputers) As Task

        ' Loop through each computer to deploy

        For Each computertodeploy In listofcomputers

            ' Copy installer to remote computer

            Try
                IO.File.Copy(IO.Path.Combine(sourcefilepath, installerfilename), IO.Path.Combine("\\" & computertodeploy & "\", destinationfilepath, installerfilename), True)
            Catch ex As Exception
                If ex.Message.Contains("The network path was not found.") Then
                    'TO DO handle path not existing
                End If
            End Try

            ' Run installer

            ps = PowerShell.Create()

            Dim command As New PSCommand()
            command.AddCommand("Invoke-Command")
            command.AddParameter("ComputerName", computertodeploy)
            command.AddParameter("ScriptBlock", ScriptBlock.Create("(Start-Process 'msiexec' -ArgumentList '/i " & destinationfilepathforinstall & " /quiet /norestart' -Wait -PassThru).ExitCode"))
            ps.Commands = command

            Dim output As New PSDataCollection(Of PSObject)()

            AddHandler output.DataAdded, AddressOf Output_DataAdded_DeploySoftware

            AddHandler ps.Streams.Error.DataAdded, AddressOf Error_DataAdded_DeploySoftware

            AddHandler ps.InvocationStateChanged, AddressOf OnInvocationStateChanged

            Try
                Await InvokeAsync(ps, token, output)
            Catch ex As Exception
            End Try

            ps.Dispose()

            ' Delete installer from remote computer

            Try
                IO.File.Delete(IO.Path.Combine("\\" & computertodeploy & "\", destinationfilepath, installerfilename))
            Catch ex As Exception
            End Try

        Next

        ' show grid column summary
        UpdateColumnSummaries("Test5", "Deploy Success: ", installsuccess.Count)
        UpdateColumnSummaries("Test5", "Deploy Failed: ", installfailed.Count)
        UpdateColumnSummaries("Test5", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Deploy Success: " & installsuccess.Count)
        UpdateTextBox(tbInfo, "Deploy Failed: " & installfailed.Count)
        UpdateTextBox(tbInfo, "")

    End Function


#End Region


#Region "Show toast notification to all successful installs"

    Public Async Function ShowToastNotification(ByVal listofcomputers) As Task

        ps = PowerShell.Create()

        Dim scriptContents = New StringBuilder()

        scriptContents.AppendLine("Param($computers)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("$toastsuccess = @()")
        scriptContents.AppendLine("$toastfailed = @()")
        scriptContents.AppendLine("$toastfailedloggedoff = @()")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("Function Show-Toast {")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    param (")
        scriptContents.AppendLine("        [Parameter(Mandatory = $true)]")
        scriptContents.AppendLine("        [string]")
        scriptContents.AppendLine("        $Title,")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("        [Parameter(Mandatory = $true)]")
        scriptContents.AppendLine("        [string]")
        scriptContents.AppendLine("        $Body")
        scriptContents.AppendLine("    )")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Load required namespaces - not really needed it seems")
        scriptContents.AppendLine("    $null = [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime]")
        scriptContents.AppendLine("    $null = [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime]")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Set the AppID to use")
        scriptContents.AppendLine("    $AppID = '{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\WindowsPowerShell\v1.0\powershell.exe' # using PowerShell")
        scriptContents.AppendLine("    #$AppID = 'PC.Assist' # using a custom AppID entry")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Configure the AppID in the registry for use with the Action Center, if required (so notification stays in notification center until dismissed)")
        scriptContents.AppendLine("    $RegPath = 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings'")
        scriptContents.AppendLine("    If (!(Test-Path -Path '$RegPath\$AppId')) {")
        scriptContents.AppendLine("        $null = New-Item -Path '$RegPath\$AppId' -Force")
        scriptContents.AppendLine("        $null = New-ItemProperty -Path '$RegPath\$AppId' -Name 'ShowInActionCenter' -Value 1 -PropertyType 'DWORD'")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("    Else {")
        scriptContents.AppendLine("        Set-ItemProperty 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\$AppID' -Name 'ShowInActionCenter' -Type Dword -Value '1' -ErrorAction Ignore")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("  # Define the toast notification in XML format")
        scriptContents.AppendLine("    [xml]$ToastTemplate = @""")
        scriptContents.AppendLine("<toast scenario=""reminder"">")
        scriptContents.AppendLine("    <visual>")
        scriptContents.AppendLine("    <binding template=""ToastGeneric"">")
        scriptContents.AppendLine("        <text>$Title</text>")
        scriptContents.AppendLine("		<text>$Body</text>")
        scriptContents.AppendLine("    </binding>")
        scriptContents.AppendLine("    </visual>")
        scriptContents.AppendLine("	<actions>")
        scriptContents.AppendLine("		<action activationType=""system"" arguments=""dismiss"" content=""""/>")
        scriptContents.AppendLine("	</actions>")
        scriptContents.AppendLine("    <audio src=""ms-winsoundevent:Notification.Reminder""/>")
        scriptContents.AppendLine("</toast>")
        scriptContents.AppendLine("""@")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Load the notification into the required format")
        scriptContents.AppendLine("    $ToastXml = New-Object -TypeName Windows.Data.Xml.Dom.XmlDocument")
        scriptContents.AppendLine("    $ToastXml.LoadXml($ToastTemplate.OuterXml)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Display")
        scriptContents.AppendLine("    Try {")
        scriptContents.AppendLine("        $Notify = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($AppID).Show($ToastXml)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("        Return 'Success'")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("    Catch {")
        scriptContents.AppendLine("        Return $PSItem.Exception.InnerException")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("}")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("$results = Invoke-Command -ComputerName $computers -ScriptBlock ${function:Show-Toast}  -ArgumentList """ & toasttitle & """,""" & toastbody & """ 2>&1 ")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("   foreach ($result in $results) {")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("        If ($result -eq ""Success"") {")
        scriptContents.AppendLine("            $toastsuccess += $result | Select-Object -ExpandProperty PSComputerName")
        scriptContents.AppendLine("        }")
        scriptContents.AppendLine("        Else {")
        scriptContents.AppendLine("            If ($result -is [System.Management.Automation.ErrorRecord]) {")
        scriptContents.AppendLine("                $toastfailed += $result.TargetObject ")
        scriptContents.AppendLine("            } ")
        scriptContents.AppendLine("            Else { ")
        scriptContents.AppendLine("                If ($result.Message.Contains(""The notification platform is unavailable."")) { ")
        scriptContents.AppendLine("                    $toastfailedloggedoff += ($result | Select-Object -ExpandProperty PSComputerName) ")
        scriptContents.AppendLine("                } ")
        scriptContents.AppendLine("                Else { ")
        scriptContents.AppendLine("                    $toastfailed += ($result | Select-Object -ExpandProperty PSComputerName) ")
        scriptContents.AppendLine("                }")
        scriptContents.AppendLine("            } ")
        scriptContents.AppendLine("        }")
        scriptContents.AppendLine("    } ")


        ps.AddScript(scriptContents.ToString)

        ps.AddParameter("computers", listofcomputers)

        Await InvokeAsync(ps, token)

        Dim sResults = ps.Runspace.SessionStateProxy.GetVariable("results")
        Dim sToastSuccess = ps.Runspace.SessionStateProxy.GetVariable("toastsuccess")
        Dim sToastFailed = ps.Runspace.SessionStateProxy.GetVariable("toastfailed")
        Dim sToastFailedLoggedOff = ps.Runspace.SessionStateProxy.GetVariable("toastfailedloggedoff")

        ps.Dispose()

        ' update grid for all toast results (and add results to variables for results count)
        For Each ts In sToastSuccess
            Me.Invoke(Sub() UpdateRecord(ts.ToString, NameOf(TestResults.Test6), "Toast Success"))
            toastsuccess.Add(ts.ToString)
        Next
        For Each tf In sToastFailed
            Me.Invoke(Sub() UpdateRecord(tf.ToString, NameOf(TestResults.Test6), "Toast Failed"))
            toastfailed.Add(tf.ToString)
        Next
        For Each tflo In sToastFailedLoggedOff
            Me.Invoke(Sub() UpdateRecord(tflo.ToString, NameOf(TestResults.Test6), "Toast Failed - Logged Off"))
            toastfailed.Add(tflo.ToString)
        Next

        ' N.B. - couldn't initially work out way to directly get information out of sResults ($results) so the results are processed in the PowerShell script - asked on various forums but initially no joy - https://docs.microsoft.com/en-us/answers/questions/945848/how-to-access-values-from-runspacesessionstateprox.html, https://forums.powershell.org/t/how-to-access-values-from-runspace-sessionstateproxy-getvariable/20006, https://stackoverflow.com/questions/73142148/how-to-access-values-from-runspace-sessionstateproxy-getvariable
        ' finally got answer on https://community.spiceworks.com/topic/2459073-how-to-access-values-from-runspace-sessionstateproxy-getvariable which is shown in 'Public Async Function ShowToastNotification2' below

        ' show grid column summary
        UpdateColumnSummaries("Test6", "Toast Success: ", toastsuccess.Count)
        UpdateColumnSummaries("Test6", "Toast Failed: ", toastfailed.Count)
        UpdateColumnSummaries("Test6", "Duration: ", stpw.Elapsed.ToString("hh\:mm\:ss"))

        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Toast Success: " & toastsuccess.Count)
        UpdateTextBox(tbInfo, "Toast Failed: " & toastfailed.Count)
        UpdateTextBox(tbInfo, "")

    End Function

    Public Async Function ShowToastNotification2(ByVal listofcomputers) As Task

        ' solution for above issue from https://community.spiceworks.com/topic/2459073-how-to-access-values-from-runspace-sessionstateproxy-getvariable?page=1

        ps = PowerShell.Create()

        Dim scriptContents = New StringBuilder()

        scriptContents.AppendLine("Param($computers)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("$toastsuccess = @()")
        scriptContents.AppendLine("$toastfailed = @()")
        scriptContents.AppendLine("$toastfailedloggedoff = @()")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("Function Show-Toast {")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    param (")
        scriptContents.AppendLine("        [Parameter(Mandatory = $true)]")
        scriptContents.AppendLine("        [string]")
        scriptContents.AppendLine("        $Title,")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("        [Parameter(Mandatory = $true)]")
        scriptContents.AppendLine("        [string]")
        scriptContents.AppendLine("        $Body")
        scriptContents.AppendLine("    )")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Load required namespaces - not really needed it seems")
        scriptContents.AppendLine("    $null = [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime]")
        scriptContents.AppendLine("    $null = [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime]")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Set the AppID to use")
        scriptContents.AppendLine("    $AppID = '{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\WindowsPowerShell\v1.0\powershell.exe' # using PowerShell")
        scriptContents.AppendLine("    #$AppID = 'PC.Assist' # using a custom AppID entry")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Configure the AppID in the registry for use with the Action Center, if required (so notification stays in notification center until dismissed)")
        scriptContents.AppendLine("    $RegPath = 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings'")
        scriptContents.AppendLine("    If (!(Test-Path -Path '$RegPath\$AppId')) {")
        scriptContents.AppendLine("        $null = New-Item -Path '$RegPath\$AppId' -Force")
        scriptContents.AppendLine("        $null = New-ItemProperty -Path '$RegPath\$AppId' -Name 'ShowInActionCenter' -Value 1 -PropertyType 'DWORD'")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("    Else {")
        scriptContents.AppendLine("        Set-ItemProperty 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\$AppID' -Name 'ShowInActionCenter' -Type Dword -Value '1' -ErrorAction Ignore")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("  # Define the toast notification in XML format")
        scriptContents.AppendLine("    [xml]$ToastTemplate = @""")
        scriptContents.AppendLine("<toast scenario=""reminder"">")
        scriptContents.AppendLine("    <visual>")
        scriptContents.AppendLine("    <binding template=""ToastGeneric"">")
        scriptContents.AppendLine("        <text>$Title</text>")
        scriptContents.AppendLine("		<text>$Body</text>")
        scriptContents.AppendLine("    </binding>")
        scriptContents.AppendLine("    </visual>")
        scriptContents.AppendLine("	<actions>")
        scriptContents.AppendLine("		<action activationType=""system"" arguments=""dismiss"" content=""""/>")
        scriptContents.AppendLine("	</actions>")
        scriptContents.AppendLine("    <audio src=""ms-winsoundevent:Notification.Reminder""/>")
        scriptContents.AppendLine("</toast>")
        scriptContents.AppendLine("""@")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Load the notification into the required format")
        scriptContents.AppendLine("    $ToastXml = New-Object -TypeName Windows.Data.Xml.Dom.XmlDocument")
        scriptContents.AppendLine("    $ToastXml.LoadXml($ToastTemplate.OuterXml)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    # Display")
        scriptContents.AppendLine("    Try {")
        scriptContents.AppendLine("        $Notify = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($AppID).Show($ToastXml)")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("        Return 'Success'")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("    Catch {")

        ' was
        'scriptContents.AppendLine("        Return $PSItem.Exception.InnerException")
        ' changed to
        scriptContents.AppendLine("        Return $_.Exception")

        scriptContents.AppendLine("    }")
        scriptContents.AppendLine("")
        scriptContents.AppendLine("}")
        scriptContents.AppendLine("")

        ' was
        'scriptContents.AppendLine("$results = Invoke-Command -ComputerName $computers -ScriptBlock ${function:Show-Toast}  -ArgumentList """ & toasttitle & """,""" & toastbody & """ 2>&1 ")
        ' changed to
        scriptContents.AppendLine("Invoke-Command -ComputerName $computers -ScriptBlock ${function:Show-Toast}  -ArgumentList """ & toasttitle & """,""" & toastbody & """ 2>&1 ")

        'removed all the script lines handling results

        ps.AddScript(scriptContents.ToString)

        ps.AddParameter("computers", listofcomputers)

        ' code from forum
        ' Dim sResults As Collection(Of PSObject) = ps.Invoke()
        ' replaced with my code to run script with cancellation token
        Dim sResults = Await InvokeAsync(ps, token)

        ps.Dispose()

        For Each sresult In sResults
            If sresult.BaseObject.ToString() <> "" Then
                If sresult.BaseObject.ToString() = "Success" Then
                    MsgBox(String.Format("{0} - {1}", sresult.Properties("PSComputerName").Value, "Success"))
                Else
                    MsgBox(String.Format("{0} - {1}", sresult.BaseObject.TargetObject, sresult.BaseObject.Exception.Message))
                End If
            Else
                MsgBox(String.Format("{0} - Exception - {1}", sresult.Properties("PSComputerName").Value, sresult.Properties("Message").Value))
            End If
        Next

        'TODO just an example of showing results above, would still need to add code to add it to grid etc if used this method


        ' Show results 
        UpdateTextBox(tbInfo, "")
        UpdateTextBox(tbInfo, "Test Duration: " & stpw.Elapsed.ToString("hh\:mm\:ss"))
        UpdateTextBox(tbInfo, "Toast Success: " & toastsuccess.Count)
        UpdateTextBox(tbInfo, "Toast Failed: " & toastfailed.Count)
        UpdateTextBox(tbInfo, "")

    End Function


#End Region

#Region "Handlers"


    Private Sub Output_DataAdded_TestConnection(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        Dim myp As PSDataCollection(Of PSObject) = DirectCast(sender, PSDataCollection(Of PSObject))

        Dim results As Collection(Of PSObject) = myp.ReadAll()
        For Each result As PSObject In results

            ' add to list
            If result.Properties("Status").Value.ToString = "Success" Then
                online.Add(result.Properties("Destination").Value)
            Else
                offline.Add(result.Properties("Destination").Value & " (" & result.Properties("Status").Value.ToString & ")")
            End If

            'output for debug purposes only
            'UpdateTextBox(tbOutput, result.Properties("Destination").Value & " = " & result.Properties("Status").Value.ToString)
            'or
            ' UpdateTextBox(tbOutput, result.Members("Destination").Value & " = " & result.Members("Status").Value.ToString)
            Debug.WriteLine(result.Properties("Destination").Value & " = " & result.Properties("Status").Value.ToString)

            ' update grid
            Me.Invoke(Sub() UpdateRecord(result.Properties("Destination").Value, NameOf(TestResults.Test1), "Online"))

        Next result

    End Sub
    Private Sub Error_DataAdded_TestConnection(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        ' do something when an error is written to the error stream
        Dim streamObjectsReceived = TryCast(sender, PSDataCollection(Of ErrorRecord))
        Dim currentStreamRecord = streamObjectsReceived(e.Index)

        ' add to list
        offline.Add(currentStreamRecord.TargetObject.ToString & " (" & currentStreamRecord.Exception.InnerException.Message & ")")

        'output for debug purposes only
        'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " - " & currentStreamRecord.Exception.InnerException.Message)
        UpdateTextBox(tbError, "TestConnection: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.InnerException.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)
        Debug.WriteLine("TestConnection: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.InnerException.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)

        ' update grid
        Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test1), "Offline"))

    End Sub


    Private Sub Output_DataAdded_CheckRemoting(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        Dim myp As PSDataCollection(Of PSObject) = DirectCast(sender, PSDataCollection(Of PSObject))

        Dim results As Collection(Of PSObject) = myp.ReadAll()
        For Each result As PSObject In results

            ' add to list
            remoteworking.Add(result.Properties("PSComputerName").Value)

            'output for debug purposes only
            'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " = " & " Remoting Working")
            Debug.WriteLine(result.Properties("PSComputerName").Value & " = " & " Remoting Working")

            ' update grid
            Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test2), "Remoting Working"))

        Next result

    End Sub
    Private Sub Error_DataAdded_CheckRemoting(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        ' do something when an error is written to the error stream
        Dim streamObjectsReceived = TryCast(sender, PSDataCollection(Of ErrorRecord))
        Dim currentStreamRecord = streamObjectsReceived(e.Index)

        'output for debug purposes only
        UpdateTextBox(tbError, "CheckRemoting: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)
        Debug.WriteLine("CheckRemoting: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)

        If currentStreamRecord.Exception.Message.Contains("WinRM cannot process the request. The following error with errorcode 0x80090322 occurred while using Kerberos authentication: An unknown security error occurred.") Then
            ' add to list
            remotenotworking.Add(currentStreamRecord.TargetObject.ToString & " (DNS)")
            'output for debug purposes only
            'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - DNS")
            Debug.WriteLine(currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - DNS")
            ' update grid
            Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test2), "Remoting Not Working (DNS)"))
        ElseIf currentStreamRecord.Exception.Message.Contains("Access is denied") Then
            ' add to list
            remotenotworking.Add(currentStreamRecord.TargetObject.ToString & " (Remoting Not Enabled)")
            'output for debug purposes only
            'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - Not Enabled")
            Debug.WriteLine(currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - Not Enabled")
            ' update grid
            Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test2), "Remoting Not Working (Not Enabled)"))
        ElseIf currentStreamRecord.Exception.Message.Contains("public profiles") Then
            ' add to list
            remotenotworking.Add(currentStreamRecord.TargetObject.ToString & " (Public)")
            'output for debug purposes only
            'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - Public")
            Debug.WriteLine(currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - Public")
            ' update grid
            Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test2), "Remoting Not Working (Public)"))
        Else
            ' add to list
            remotenotworking.Add(currentStreamRecord.TargetObject.ToString)
            'output for debug purposes only
            'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - " & currentStreamRecord.Exception.Message)
            Debug.WriteLine(currentStreamRecord.TargetObject.ToString & " = " & "Remoting Not Working - " & currentStreamRecord.Exception.Message)
            ' update grid
            Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test2), "Remoting Not Working (Other)"))
        End If

    End Sub

    Private Sub Output_DataAdded_CheckInstalled(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        Dim myp As PSDataCollection(Of PSObject) = DirectCast(sender, PSDataCollection(Of PSObject))

        Dim results As Collection(Of PSObject) = myp.ReadAll()
        For Each result As PSObject In results

            ' add to list
            AddIfNotExists(softwarealreadyinstalled, result.Properties("PSComputerName").Value)

            'output for debug purposes only
            'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " = " & " Software Installed")
            Debug.WriteLine(result.Properties("PSComputerName").Value & " = " & " Software Installed")

            ' update grid
            Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test3), "Software Installed"))

        Next result

    End Sub
    Private Sub Error_DataAdded_CheckInstalled(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        ' do something when an error is written to the error stream
        Dim streamObjectsReceived = TryCast(sender, PSDataCollection(Of ErrorRecord))
        Dim currentStreamRecord = streamObjectsReceived(e.Index)

        ' add to list
        softwarenotalreadyinstalled.Add(currentStreamRecord.TargetObject.ToString)

        'output for debug purposes only
        'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " - " & currentStreamRecord.Exception.Message)
        UpdateTextBox(tbError, "CheckInstalled: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)
        Debug.WriteLine("CheckInstalled: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)

        ' update grid
        ' N/A - done after results obtained

    End Sub

    Private Sub Output_DataAdded_CheckProcess(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        Dim myp As PSDataCollection(Of PSObject) = DirectCast(sender, PSDataCollection(Of PSObject))

        Dim results As Collection(Of PSObject) = myp.ReadAll()
        For Each result As PSObject In results

            ' add to list
            AddIfNotExists(processrunning, result.Properties("PSComputerName").Value)

            'output for debug purposes only
            'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " = " & " Process Running")
            Debug.WriteLine(result.Properties("PSComputerName").Value & " = " & " Process Running")

            ' update grid
            Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test4), "Process Running"))

        Next result

    End Sub
    Private Sub Error_DataAdded_CheckProcess(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        ' do something when an error is written to the error stream
        Dim streamObjectsReceived = TryCast(sender, PSDataCollection(Of ErrorRecord))
        Dim currentStreamRecord = streamObjectsReceived(e.Index)

        ' add to list
        processnotrunning.Add(currentStreamRecord.TargetObject.ToString)

        'output for debug purposes only
        'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " - " & currentStreamRecord.Exception.Message)
        UpdateTextBox(tbError, "CheckProcess: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)
        Debug.WriteLine("CheckProcess: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)

        ' update grid
        ' N/A - done after results obtained - if we wanted to handle here instead then we could remove '& " -ErrorAction SilentlyContinue"' from the PowerShell command so errors are thrown and then use code below to convert currentStreamRecord to RemotingErrorRecord which would then allow us to access values such as OriginInfo to retrieve PSComputerName - as per reply here https://docs.microsoft.com/en-us/answers/questions/942533/how-to-retrieve-pscomputername-from-origininfo-whe.html?childToView=945297#comment-945297
        Dim results = TryCast(currentStreamRecord, RemotingErrorRecord)
        If results IsNot Nothing Then
            Dim name = results.OriginInfo.PSComputerName
        End If

    End Sub

    Private Sub Output_DataAdded_DeploySoftware(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        Dim myp As PSDataCollection(Of PSObject) = DirectCast(sender, PSDataCollection(Of PSObject))

        Dim results As Collection(Of PSObject) = myp.ReadAll()
        For Each result As PSObject In results

            ' exit code 0 added to installsuccess, the rest are added to installfailed
            If result.ToString = 0 Then
                ' add to list
                AddIfNotExists(installsuccess, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Deploy Success")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Deploy Success")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Deploy Success"))
            ElseIf result.ToString = 1602 Then
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Already Installed (1602)")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Already Installed (1602)")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Already Installed (1602)"))
            ElseIf result.ToString = 1642 Then
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Already Installed (1642)")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Already Installed (1642)")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Already Installed (1642)"))
            ElseIf result.ToString = 1603 Then
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Fatal Error (1603)")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Fatal Error (1603)")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Fatal Error (1603)"))
            ElseIf result.ToString = 1641 Then
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Rebooting (1641)")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Rebooting (1641)")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Rebooting (1641)"))
            ElseIf result.ToString = 3010 Then
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Needs Reboot (3010)")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Needs Reboot (3010)")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Needs Reboot (3010)"))
            Else
                ' add to list
                AddIfNotExists(installfailed, result.Properties("PSComputerName").Value)
                'output for debug purposes only
                'UpdateTextBox(tbOutput, result.Properties("PSComputerName").Value & " Install Failed (" & result.ToString & ")")
                Debug.WriteLine(result.Properties("PSComputerName").Value & " Install Failed (" & result.ToString & ")")
                ' update grid
                Me.Invoke(Sub() UpdateRecord(result.Properties("PSComputerName").Value, NameOf(TestResults.Test5), "Install Failed (" & result.ToString & ")"))
            End If

        Next result

    End Sub
    Private Sub Error_DataAdded_DeploySoftware(ByVal sender As Object, ByVal e As DataAddedEventArgs)
        ' do something when an error is written to the error stream
        Dim streamObjectsReceived = TryCast(sender, PSDataCollection(Of ErrorRecord))
        Dim currentStreamRecord = streamObjectsReceived(e.Index)

        ' add to list
        ' N.B. most failures handled by exit codes caught in Output_DataAdded_SoftwareInstall
        installfailed.Add(currentStreamRecord.TargetObject.ToString)

        'output for debug purposes only
        'UpdateTextBox(tbOutput, currentStreamRecord.TargetObject.ToString & " - " & currentStreamRecord.Exception.Message)
        UpdateTextBox(tbError, "DeploySoftware: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)
        Debug.WriteLine("DeploySoftware: " & vbCrLf & currentStreamRecord.TargetObject.ToString & vbCrLf & currentStreamRecord.Exception.Message & vbCrLf & currentStreamRecord.Exception.ToString & vbCrLf)

        ' update grid
        ' N.B. failures handled by exit codes caught in Output_DataAdded_SoftwareInstall
        Me.Invoke(Sub() UpdateRecord(currentStreamRecord.TargetObject.ToString, NameOf(TestResults.Test5), "Deploy Failed"))

    End Sub

    Private Sub OnInvocationStateChanged(ByVal sender As Object, ByVal e As PSInvocationStateChangedEventArgs)
        UpdateLabel(lblInvocationStateInfo, "INVOCATION STATE: " & e.InvocationStateInfo.State.ToString)
        Debug.WriteLine("INVOCATION STATE: " & e.InvocationStateInfo.State.ToString)
    End Sub

#End Region



#Region "Shared"
    Public Sub UpdateTextBox(textbox As TextBox, text As String)
        If textbox.InvokeRequired Then
            'We are on a secondary thread.
            textbox.Invoke(New Action(Of TextBox, String)(AddressOf UpdateTextBox), textbox, text)
        Else
            'We are on the UI thread.
            textbox.AppendText(text & vbCrLf)
        End If
    End Sub

    Public Sub UpdateLabel(label As Label, text As String)
        If label.InvokeRequired Then
            'We are on a secondary thread.
            label.Invoke(New Action(Of Label, String)(AddressOf UpdateLabel), label, text)
        Else
            'We are on the UI thread.
            label.Text = text
        End If
    End Sub

    Public Function AddIfNotExists(Of T)(ByVal list As List(Of T), ByVal value As T) As Boolean

        If Not list.Contains(value) Then

            list.Add(value)
            Return True
        End If
        Return False
    End Function

    Public Sub UpdateColumnSummaries(columnname As String, caption As String, variable As String)

        GridView1.Columns(columnname).Summary.Add(DevExpress.Data.SummaryItemType.Custom, "", caption & variable)

    End Sub


#End Region




#Region "Test Code"

    ' for testing specific sections works with a predefined set of computers

    Private Async Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

        Button3.Enabled = False

        Dim computerlist As New List(Of String)({"PC-1", "PC-2", "PC-3"})

        ' Add computers to grid
        For Each computer As PSObject In computerlist
            If Not _TestResults.Any(Function(p) p.Computer = computer.ToString) Then
                _TestResults.Add(New TestResults() With {.Computer = computer.ToString})
            End If
        Next

        GridControl1.DataSource = _TestResults

        ' test code below

        Await ShowToastNotification2(computerlist)

        ' test code above

        Button3.Enabled = True

    End Sub











#End Region


#Region "Update grid"

    Private Function FindRowHandle(pcName As String) As Integer
        Return GridView1.LocateByValue("Computer", pcName)
    End Function

    Private Sub UpdateValue(rowHandle As Integer, fieldName As String, remoteValue As Object)
        GridView1.SetRowCellValue(rowHandle, fieldName, remoteValue)
    End Sub

    Private Sub UpdateRecord(pcName As String, fieldName As String, remoteValue As Object)
        Dim rowHandle = FindRowHandle(pcName)
        UpdateValue(rowHandle, fieldName, remoteValue)
        GridView1.MakeRowVisible(rowHandle) ' scroll grid to show last updated row
    End Sub

#End Region



    Public Class TestResults

        ' using info from reply here - https://supportcenter.devexpress.com/ticket/details/t974812/updating-results-in-a-gridview-realtime-best-way-to-achieve - a data source for grid which supports change notifications to automatically update values without needing to refresh etc

        Implements INotifyPropertyChanged

        Private mComputer As String
        Public Property Computer() As String
            Get
                Return mComputer
            End Get
            Set(ByVal value As String)
                If mComputer = value Then
                    Return
                End If

                mComputer = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Computer)))
            End Set
        End Property

        Private mTest1 As String
        Public Property Test1() As String
            Get
                Return mTest1
            End Get
            Set(ByVal value As String)
                If mTest1 = value Then
                    Return
                End If

                mTest1 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test1)))
            End Set
        End Property

        Private mTest2 As String
        Public Property Test2() As String
            Get
                Return mTest2
            End Get
            Set(ByVal value As String)
                If mTest2 = value Then
                    Return
                End If

                mTest2 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test2)))
            End Set
        End Property

        Private mTest3 As String
        Public Property Test3() As String
            Get
                Return mTest3
            End Get
            Set(ByVal value As String)
                If mTest3 = value Then
                    Return
                End If

                mTest3 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test3)))
            End Set
        End Property


        Private mTest4 As String
        Public Property Test4() As String
            Get
                Return mTest4
            End Get
            Set(ByVal value As String)
                If mTest4 = value Then
                    Return
                End If

                mTest4 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test4)))
            End Set
        End Property

        Private mTest5 As String
        Public Property Test5() As String
            Get
                Return mTest5
            End Get
            Set(ByVal value As String)
                If mTest5 = value Then
                    Return
                End If

                mTest5 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test5)))
            End Set
        End Property

        Private mTest6 As String
        Public Property Test6() As String
            Get
                Return mTest6
            End Get
            Set(ByVal value As String)
                If mTest6 = value Then
                    Return
                End If

                mTest6 = value
                PropertyChangedEvent?.Invoke(Me, New PropertyChangedEventArgs(NameOf(Test6)))
            End Set
        End Property


        Private Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    End Class

End Class


Module Module1
    <System.Runtime.CompilerServices.Extension>
    Public Function InvokeAsync(ByVal ps As PowerShell, ByVal cancel As CancellationToken, Optional ByVal output As PSDataCollection(Of PSObject) = Nothing) As Task(Of PSDataCollection(Of PSObject))
        Return Task.Factory.StartNew(Function()
                                         ' Do the invocation
                                         Dim invocation As IAsyncResult
                                         If output IsNot Nothing Then
                                             invocation = ps.BeginInvoke(Of PSObject, PSObject)(Nothing, output)
                                         Else
                                             invocation = ps.BeginInvoke()
                                         End If
                                         WaitHandle.WaitAny({invocation.AsyncWaitHandle, cancel.WaitHandle})

                                         If cancel.IsCancellationRequested Then
                                             ps.Stop()
                                         End If

                                         'n.b. to stop debugger catching exception here right click on System.OperationCanceledException in ExceptionSettings (CTRL+ALT+E) and check Continue When Unhandled in User Code) - see https://docs.microsoft.com/en-us/visualstudio/debugger/managing-exceptions-with-the-debugger?view=vs-2022#BKMK_UserUnhandled
                                         cancel.ThrowIfCancellationRequested()
                                         Return ps.EndInvoke(invocation)
                                     End Function, cancel)
    End Function

End Module
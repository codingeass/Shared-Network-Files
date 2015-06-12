Imports System.IO
Imports System.DirectoryServices
Imports System.Threading
Imports System.Net
Imports System.Text.RegularExpressions
Public Class Form1
    Dim version As Double = 1.5

    Public Function GetHostName() As String
        Return Dns.GetHostName()
    End Function
    Shared Function GetProcessText(ByVal process As String, ByVal param As String, ByVal workingDir As String) As String
        Dim p As Process = New Process
        ' this is the name of the process we want to execute 
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.FileName = process
        If Not (workingDir = "") Then
            p.StartInfo.WorkingDirectory = workingDir
        End If
        p.StartInfo.Arguments = param

        ' need to set this to false to redirect output
        p.StartInfo.UseShellExecute = False
        p.StartInfo.RedirectStandardOutput = True
        ' start the process 
        p.Start()
        ' read all the output
        ' here we could just read line by line and display it
        ' in an output window 
        Dim output As String = p.StandardOutput.ReadToEnd
        ' wait for the process to terminate 
        p.WaitForExit()
        Return output
    End Function
    Public Function GetIpAddress() As String
        Return Dns.GetHostEntry(GetHostName()).AddressList(0).ToString()
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = "All Ip's Sharing data in Network"
        'FindingThreats()
        'Me.BackColor = Color.DarkSlateGray
        Dim result As String = GetProcessText("arp", "-a", "")

        'Dim regex As Regex = New Regex("(([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.){3}([01]?\\d\\d?|2[0-4]\\d|25[0-5])", RegexOptions.IgnoreCase)
        'Dim match As Match = regex.Match(result)

        Dim res() As String = result.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        Dim res1() As String
        Dim i As Integer
        Dim nodeCount As Integer = 0
        Dim root = New TreeNode("Network")
        TreeView1.Nodes.Add(root)
        Dim treeIp As TreeNode
        i = 3
        While i < res.Length
            'TreeView1.Nodes.Clear()
            res1 = Split(res(i))

            If res1(2)(0) = "I" Then
                Exit While
            End If

            If (res1(2)(0) <> "2" Or res1(0) <> "") Then
                Try
                    treeIp = New TreeNode(res1(2))
                    treeIp.Name = res1(2)
                    treeIp.Text = res1(2)
                    TreeView1.Nodes(0).Nodes.Add(treeIp)
                    Dim thread As Thread = New System.Threading.Thread(Sub() Me.EnterTheFolder(res1(2), treeIp))
                    thread.Start()
                    nodeCount += 1
                    'TreeView1.Nodes(0).Nodes.Add(New TreeNode(System.Net.Dns.GetHostEntry(res1(2)).HostName.ToString))
                Catch
                End Try
                'Tree code End
            End If
            i += 1
        End While
        Label4.Text = "Count : " & nodeCount
        TextBox1.Text = GetHostName()
        '' for context menu
        'For Each RootNode As TreeNode In TreeView1.Nodes
        '    RootNode.ContextMenuStrip = ContextMenuStrip1
        '    For Each ChildNode As TreeNode In RootNode.Nodes
        '        ChildNode.ContextMenuStrip = ContextMenuStrip1
        '    Next
        'Next
        ''context menu end


    End Sub


    Sub EnterTheFolder(ipAddress As String, treeIp As TreeNode)
        Dim result As String = GetProcessText("net", "view " & ipAddress, "")
        Dim res() As String = result.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        Dim i As Integer = 0
        'Dim str As String = ""
        Dim counter As Integer = 0
        While i < res.Length
            If (Trim(res(i)).Split().Last = "Disk") Then
                Try
                    If TreeView1.InvokeRequired Then
                        TreeView1.Invoke(DirectCast(Sub() TreeView1.Nodes(0).Nodes(TreeView1.Nodes(0).Nodes.IndexOf(treeIp)).Nodes.Add(New TreeNode(Trim(res(i).Replace("Disk", "")))), MethodInvoker))
                    Else
                        TreeView1.Nodes(0).Nodes(TreeView1.Nodes(0).Nodes.IndexOf(treeIp)).Nodes.Add(New TreeNode(Trim(res(i).Replace("Disk", ""))))
                    End If
                    counter = 1
                    'TreeView1.Nodes(0).Nodes(TreeView1.Nodes(0).Nodes.IndexOf(treeIp)).Nodes.Add(New TreeNode(Trim(res(i).Replace("Disk", ""))))
                Catch m As Exception
                    'MsgBox(m.Message)
                End Try
            End If
            i += 1
        End While
    End Sub

    Sub FindingThreats()
        'ListView1.Items.Clear()
        Dim childEntry As DirectoryEntry
        Dim ParentEntry As New DirectoryEntry
        Try
            ParentEntry.Path = "WinNT:"
            For Each childEntry In ParentEntry.Children
                Select Case childEntry.SchemaClassName
                    Case "Domain"
                        Dim SubChildEntry As DirectoryEntry
                        Dim SubParentEntry As New DirectoryEntry
                        SubParentEntry.Path = "WinNT://" & childEntry.Name
                        For Each SubChildEntry In SubParentEntry.Children
                            Select Case SubChildEntry.SchemaClassName
                                Case "Computer"
                                    'ListView1.Items.Add(SubChildEntry.Name)
                            End Select
                        Next
                End Select
            Next
        Catch Excep As Exception
            MsgBox("Error While Reading Directories : " + Excep.Message.ToString)
        Finally
            ParentEntry = Nothing
        End Try
    End Sub

    Sub FindingHost()
        For i = 1 To 254
            Try
                'RichTextBox1.AppendText("Host name =  " & System.Net.Dns.GetHostEntry("172.17.220." & i.ToString).HostName.ToString & vbCrLf & vbCrLf)
            Catch ex As Exception
            End Try
        Next
    End Sub

    ''' <summary>
    ''' To open network folder selected
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Try
            Dim path As String = TreeView1.SelectedNode.FullPath.ToString.Replace("Network", "\")
            GetProcessText("explorer", path, "")
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Remove nodes from treeview1 to treeview2 that have not shared any data
    ''' </summary>
    ''' <remarks></remarks>
    Sub RemoveNodeTree()
        Dim treeIp As TreeNode
        Dim size As Integer = TreeView1.Nodes(0).Nodes.Count
        Dim i As Integer
        treeIp = New TreeNode("Network")
        TreeView2.Nodes.Add(treeIp)
        i = 0
        While i < size
            Try
                treeIp = TreeView1.Nodes(0).Nodes(i)
                If treeIp.Nodes.Count = 0 Then
                    treeIp = New TreeNode(TreeView1.Nodes(0).Nodes(i).Text)
                    TreeView1.Nodes(0).Nodes(i).Remove()
                    TreeView2.Nodes.Add(treeIp)
                    size -= 1
                    i -= 1
                End If
            Catch
            End Try
            i += 1
        End While
        Label4.Text = "Count : " & i
    End Sub
    ''' <summary>
    ''' Return pc name from ip
    ''' </summary>
    ''' <param name="ipAdress"></param>
    ''' <returns></returns>
    ''' <remarks>Helper function for ReturnPCName</remarks>
    Private Shared Function GetMachineNameFromIPAddress(ipAdress As String) As String
        Dim machineName As String = String.Empty
        Try
            Dim hostEntry As IPHostEntry = Dns.GetHostEntry(ipAdress)

            machineName = hostEntry.HostName
            ' Machine not found...
        Catch ex As Exception
        End Try
        Return machineName
    End Function

    ''' <summary>
    ''' starting thread for pcName change
    ''' </summary>
    ''' <remarks></remarks>
    Sub AssignPCname()
        Dim size As Integer = TreeView1.Nodes(0).Nodes.Count
        Dim i As Integer = 0
        Try
            While i < size - 1
                Dim thread As Thread = New System.Threading.Thread(Sub() Me.ReturnPCName(i))
                thread.Start()
                i += 1
            End While
        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
    End Sub

    Sub ReturnPCName(s As Integer)
        Dim ipName As String
        Dim pcName As String = ""
        Dim treeIp As TreeNode
        treeIp = TreeView1.Nodes(0).Nodes(s)
        ipName = treeIp.Text
        pcName = GetMachineNameFromIPAddress(ipName)
        If pcName = "" Then
            Return
        End If
        Try
            If TreeView1.InvokeRequired Then
                TreeView1.Invoke(DirectCast(Sub() treeIp.Text = pcName, MethodInvoker))
            Else
                treeIp.Text = pcName
            End If
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Clear nodes from treeView1 which are not sharing data and transfer to treeView2 and show pcName in treeView1
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Label1.Text = "Shared files in Network"
        RemoveNodeTree()
        AssignPCname()
        Button1.Enabled = False
    End Sub

    ''' <summary>
    ''' To refresh nodes
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Refresh done by clearing content of treeviews and then loading the Form1 again</remarks>
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        TreeView1.Nodes.Clear()
        TreeView2.Nodes.Clear()
        Button1.Enabled = True
        Me.Form1_Load(sender, e)
    End Sub

    Private Sub Form1_Close(sender As Object, e As EventArgs) Handles MyBase.FormClosing
        Application.ExitThread()
        Me.Dispose()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutUs.Show()
    End Sub

    Private Sub NewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewToolStripMenuItem.Click
        TreeView1.Nodes.Clear()
        TreeView2.Nodes.Clear()
        Button1.Enabled = True
        Me.Form1_Load(sender, e)
    End Sub

    Private Sub SeparateNetworkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SeparateNetworkToolStripMenuItem.Click
        Label1.Text = "Shared files in Network"
        RemoveNodeTree()
        AssignPCname()
        Button1.Enabled = False
    End Sub

    Private Sub CheckUpdateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckUpdateToolStripMenuItem.Click
        Try
            enteredUrl = "http://codingeass.github.io/manga-Downloader/update.xml"
            Dim request As HttpWebRequest = WebRequest.Create(enteredUrl)
            request.UserAgent = ".NET Framework Test Client"
            Dim response As HttpWebResponse = request.GetResponse()
            Dim reader As StreamReader = New StreamReader(response.GetResponseStream())
            Dim str As String = reader.ReadToEnd
            Dim regex As Regex = New Regex("<update name=""network"">.*?</update>")
            Dim regexStr As String = "<update name=""network"">.*?<\/update>"
            Dim match As Match = regex.Match(str, regexStr, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            If match.Success Then
                MsgBox("Are you sure you want to update?")
            Else
                MsgBox("Already Updated")
            End If
        Catch
            MsgBox("Check Your Connection")
        End Try
    End Sub
End Class

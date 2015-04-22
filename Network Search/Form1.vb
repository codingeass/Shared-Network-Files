﻿Imports System.IO
Imports System.DirectoryServices
Imports System.Threading
Imports System.Net
Imports System.Text.RegularExpressions
Public Class Form1
    Dim thread As Thread
    Public Function GetHostName() As String
        Return Dns.GetHostName()
    End Function
    Shared Function GetProcessText(ByVal process As String, ByVal param As String, ByVal workingDir As String) As String
        Dim p As Process = New Process
        ' this is the name of the process we want to execute 
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

        
        FindingThreats()
        Me.BackColor = Color.DarkSlateGray
        Dim result As String = GetProcessText("arp", "-a", "")
        'MsgBox(result)
        'Dim regex As Regex = New Regex("(([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.){3}([01]?\\d\\d?|2[0-4]\\d|25[0-5])", RegexOptions.IgnoreCase)
        'Dim match As Match = regex.Match(result)
        'MsgBox(match.Groups(1).Value & "hello")
        Dim res() As String = result.Split(New String() {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)
        Dim res1() As String
        Dim i As Integer
        'Dim i As Integer
        'Dim str As String = ""
        'While i < res1.Length
        '    str = str + " " + res1(i) + "=" + i.ToString
        '    i += 1
        'End While

        'MsgBox(res1(2))
        'Tree code
        Dim root = New TreeNode("Network")
        TreeView1.Nodes.Add(root)
        Dim treeIp As TreeNode
        i = 3
        While i < res.Length
            'TreeView1.Nodes.Clear()
            res1 = Split(res(i))
            If (res1(2)(0) <> "2") Then
                Try
                    treeIp = New TreeNode(res1(2))
                    treeIp.Name = res1(2)
                    treeIp.Text = res1(2)
                    TreeView1.Nodes(0).Nodes.Add(treeIp)
                    'EnterTheFolder(res1(2), treeIp)
                    Dim thread1 As Thread = New System.Threading.Thread(Sub() Me.EnterTheFolder(res1(2), treeIp))
                    thread1.Start()
                    'TreeView1.Nodes(0).Nodes.Add(New TreeNode(System.Net.Dns.GetHostEntry(res1(2)).HostName.ToString))
                Catch
                End Try
                'Tree code End
            End If
            i += 1
        End While
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
                    'MsgBox(TreeView1.Nodes(0).Nodes(0).FullPath)
                    'Try
                    '    If (counter = 0) Then
                    '        Dim arr As TreeNode() = TreeView1.Nodes.Find(ipAddress, True)
                    '        If arr.Length <> 0 Then
                    '            Return
                    '        End If
                    '        If TreeView1.InvokeRequired And arr.Length = 0 Then
                    '            MsgBox(arr.Length)
                    '            TreeView1.Invoke(DirectCast(Sub() TreeView1.Nodes(0).Nodes.Add(treeIp), MethodInvoker))
                    '        End If
                    '    End If
                    'Catch
                    'End Try
                    If TreeView1.InvokeRequired Then
                        TreeView1.Invoke(DirectCast(Sub() TreeView1.Nodes(0).Nodes(TreeView1.Nodes(0).Nodes.IndexOf(treeIp)).Nodes.Add(New TreeNode(Trim(res(i).Replace("Disk", "")))), MethodInvoker))
                    End If
                    counter = 1
                    'TreeView1.Nodes(0).Nodes(TreeView1.Nodes(0).Nodes.IndexOf(treeIp)).Nodes.Add(New TreeNode(Trim(res(i).Replace("Disk", ""))))
                Catch m As Exception
                    MsgBox(m.Message)
                End Try
                'str += Trim(res(i).Replace("Disk", ""))
            End If
            i += 1
        End While
    End Sub

    Sub FindingThreats()
        ListView1.Items.Clear()
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
                                    ListView1.Items.Add(SubChildEntry.Name)

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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'thread = New System.Threading.Thread(AddressOf FindingHost)
        'thread.Start()
        Dim strHostName As String = "aditya"
        Dim strIPAddress As String = ""
        'MsgBox(Dns.GetHostByAddress("172.17.221.10"))
        Dim objAddressList() As System.Net.IPAddress = _
            System.Net.Dns.GetHostEntry(strHostName).AddressList
        For x = 0 To objAddressList.GetUpperBound(0)
            If objAddressList(x).AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
                strIPAddress = objAddressList(x).ToString
                Exit For
            End If
        Next
        RichTextBox1.AppendText(strIpAddress)
    End Sub
    Sub FindingHost()
        For i = 1 To 254
            Try
                RichTextBox1.AppendText("Host name =  " & System.Net.Dns.GetHostEntry("172.17.220." & i.ToString).HostName.ToString & vbCrLf & vbCrLf)
            Catch ex As Exception
            End Try
        Next
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Try
            Dim path As String = TreeView1.SelectedNode.FullPath.ToString.Replace("Network", "\")
            GetProcessText("explorer", path, "")
        Catch
        End Try
    End Sub
End Class
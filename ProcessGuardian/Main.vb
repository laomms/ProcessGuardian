Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web.Script.Serialization
Imports System.Windows.Forms

Module Main

    Public cts As New CancellationTokenSource()
    Public runing As Boolean = False
    Public MaxMemory As Integer = 100
    Public MaxCPU As Integer = 10
#Region "收到私聊消息"
    Public funRecvicePrivateMsg As RecvicePrivateMsg = New RecvicePrivateMsg(AddressOf RecvicetPrivateMessage)
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function RecvicePrivateMsg(ByRef sMsg As PrivateMessageEvent) As Integer
    Public Function RecvicetPrivateMessage(ByRef sMsg As PrivateMessageEvent) As Integer
        Dim MessageRandom As New Long
        Dim MessageReq As New UInteger
        Dim thisqq = sMsg.ThisQQ
        If sMsg.SenderQQ <> sMsg.ThisQQ Then
            If sMsg.MessageContent.Contains("[pic,hash=") Then
                'Dim matches As MatchCollection = Regex.Matches(sMsg.MessageContent, "\[pic,hash.*?\]", RegexOptions.Multiline Or RegexOptions.IgnoreCase)
                'For Each match As Match In matches
                '    API.GetImageLink(sMsg.ThisQQ, sMsg.SenderQQ, 0, match.Value)
                'Next match
            ElseIf sMsg.MessageContent = "启动进程守护" Then
                Dim PID = Process.GetProcessesByName("小栗子框架")(0).Id
                'Process.GetCurrentProcess().Id
                If runing = False Then
                    WatchProcess.StartWatch(cts.Token, PID)
                    If _process Is Nothing OrElse _process.HasExited Then
                        _process = New Process()
                    Else
                        _process.Kill()
                        _process = Nothing
                        Return 0
                    End If
                    Dim startInfo As New ProcessStartInfo With {
                        .WorkingDirectory = My.Computer.FileSystem.SpecialDirectories.Temp,
                        .CreateNoWindow = True,
                        .WindowStyle = ProcessWindowStyle.Hidden,
                        .FileName = "cmd.exe",
                        .Arguments = "/c " + My.Computer.FileSystem.SpecialDirectories.Temp + "\restart.bat"}
                    _process.StartInfo = startInfo
                    _process.Start()
                    API.SendPrivateMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.SenderQQ, "已成功开启进程守护.", MessageRandom, MessageReq)
                Else
                    API.SendPrivateMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.SenderQQ, "进程已经守护中.", MessageRandom, MessageReq)
                End If
            ElseIf sMsg.MessageContent = "停止进程守护" Then
                Try
                    If Not cts Is Nothing Then
                        cts.Cancel()
                        cts.Dispose()
                        cts = New CancellationTokenSource()
                        runing = False
                        If Not _process Is Nothing Then
                            _process.Kill()
                            _process = Nothing
                            If File.Exists(myBatFilePath) Then
                                File.Delete(myBatFilePath)
                            End If
                        End If
                    End If
                Catch ex As Exception

                End Try
                Dim NewThread As New Thread(Sub()
                                                For Each p As Process In Process.GetProcesses()
                                                    If p.ProcessName = "cmd" Then
                                                        p.Kill()
                                                        Exit For
                                                    End If
                                                Next
                                            End Sub)
                NewThread.Start()
                API.SendPrivateMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.SenderQQ, "已停止进程守护.", MessageRandom, MessageReq)
                Try
                    Form1.MyInstance.Label1.Invoke(New MethodInvoker(Sub() Form1.MyInstance.Label1.Text = "已停止进程守护"))
                Catch ex As Exception

                End Try
            ElseIf sMsg.MessageContent = "重启框架" Then
                API.restart(Pinvoke.plugin_key)
            ElseIf sMsg.MessageContent = "查询资源占用" Then
                Dim szQQID = sMsg.SenderQQ
                Call (New Thread(Sub()
                                     Dim text As String = String.Join(Environment.NewLine, CpuMemoryCapacity.HardwareInfo())
                                     text = text & Environment.NewLine & String.Join(Environment.NewLine, CpuMemoryCapacity.MemoryAvailable())
                                     text = text & Environment.NewLine & String.Join(Environment.NewLine, CpuMemoryCapacity.GetUsage())
                                     API.SendPrivateMsg(Pinvoke.plugin_key, thisqq, szQQID, text, MessageRandom, MessageReq)
                                 End Sub)).Start()
            ElseIf UCase(sMsg.MessageContent) = "查询CPU占用" Then
                Dim szQQID = sMsg.SenderQQ
                Call (New Thread(Sub()
                                     Dim text As String = String.Join(Environment.NewLine, CpuMemoryCapacity.GetCpuUsage())
                                     API.SendPrivateMsg(Pinvoke.plugin_key, thisqq, szQQID, text, MessageRandom, MessageReq)
                                 End Sub)).Start()
            ElseIf sMsg.MessageContent = "查询内存占用" Then
                Dim szQQID = sMsg.SenderQQ
                Call (New Thread(Sub()
                                     Dim strArray() As String = CpuMemoryCapacity.GetMemoryUsage().ToArray()
                                     strArray = strArray.Select(Function(s) s.TrimStart("0")).ToArray()
                                     API.SendPrivateMsg(Pinvoke.plugin_key, thisqq, szQQID, String.Join(Environment.NewLine, strArray), MessageRandom, MessageReq)
                                 End Sub)).Start()
            ElseIf sMsg.MessageContent = "崩溃测试" Then
                Dim p As Object = 0
                Dim pnt As New IntPtr(&H123456789)
                Marshal.StructureToPtr(p, pnt, False)
            Else
                'SendPrivatemsg(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.SenderQQ.ToString + "发送了这样的消息:" + sMsg.MessageContent)
            End If
        End If
        Return 0
    End Function
#End Region

#Region "收到群聊消息"
    Public funRecviceGroupMsg As RecviceGroupMsg = New RecviceGroupMsg(AddressOf RecvicetGroupMessage)
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function RecviceGroupMsg(ByRef sMsg As GroupMessageEvent) As Integer
    Public Function RecvicetGroupMessage(ByRef sMsg As GroupMessageEvent) As Integer
        Dim szGroupQQID = sMsg.MessageGroupQQ
        Dim thisqq = sMsg.ThisQQ
        If sMsg.SenderQQ <> sMsg.ThisQQ Then
            If sMsg.MessageContent.Contains("[pic,hash=") Then
                'Dim matches As MatchCollection = Regex.Matches(sMsg.MessageContent, "\[pic,hash.*?\]", RegexOptions.Multiline Or RegexOptions.IgnoreCase)
                'For Each match As Match In matches
                '    API.GetImageLink(sMsg.ThisQQ, sMsg.SenderQQ, sMsg.MessageGroupQQ, match.Value)
                'Next match
            ElseIf sMsg.MessageContent.Contains("[file,fileId=") Then '发送文件

            ElseIf sMsg.MessageContent.Contains("[Audio,hash=") Then '发送语音

            ElseIf sMsg.MessageContent = "启动进程守护" Then
                Dim PID = Process.GetProcessesByName("小栗子框架")(0).Id
                'Process.GetCurrentProcess().Id
                If runing = False Then
                    WatchProcess.StartWatch(cts.Token, PID)
                    API.SendGroupMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString + "]" + "已成功开启进程守护.", False)
                Else
                    API.SendGroupMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString + "]" + "进程已经守护中.", False)
                End If

            ElseIf sMsg.MessageContent = "停止进程守护" Then
                Try
                    If Not cts Is Nothing Then
                        cts.Cancel()
                        cts.Dispose()
                        cts = New CancellationTokenSource()
                        runing = False
                    End If
                Catch ex As Exception

                End Try
                API.SendGroupMsg(Pinvoke.plugin_key, sMsg.ThisQQ, sMsg.MessageGroupQQ, "[@" + sMsg.SenderQQ.ToString + "]" + "已停止进程守护.", False)
                Try
                    Form1.MyInstance.Label1.Invoke(New MethodInvoker(Sub() Form1.MyInstance.Label1.Text = "已停止进程守护"))
                Catch ex As Exception

                End Try
            ElseIf sMsg.MessageContent = "查询资源占用" Then
                Call (New Thread(Sub()
                                     Dim text As String = String.Join(Environment.NewLine, CpuMemoryCapacity.HardwareInfo())
                                     text = text & Environment.NewLine & String.Join(Environment.NewLine, CpuMemoryCapacity.MemoryAvailable())
                                     text = text & Environment.NewLine & String.Join(Environment.NewLine, CpuMemoryCapacity.GetUsage())
                                     API.SendGroupMsg(Pinvoke.plugin_key, thisqq, szGroupQQID, text, False)
                                 End Sub)).Start()
            ElseIf UCase(sMsg.MessageContent) = "查询CPU占用" Then
                Call (New Thread(Sub()
                                     Dim text As String = String.Join(Environment.NewLine, CpuMemoryCapacity.GetCpuUsage())
                                     API.SendGroupMsg(Pinvoke.plugin_key, thisqq, szGroupQQID, text, False)
                                 End Sub)).Start()
            ElseIf sMsg.MessageContent = "查询内存占用" Then
                Call (New Thread(Sub()
                                     Dim strArray() As String = CpuMemoryCapacity.GetMemoryUsage().ToArray()
                                     strArray = strArray.Select(Function(s) s.TrimStart("0")).ToArray()
                                     API.SendGroupMsg(Pinvoke.plugin_key, thisqq, szGroupQQID, String.Join(Environment.NewLine, strArray), False)
                                 End Sub)).Start()
            ElseIf sMsg.MessageContent = "重启框架" Then
                API.restart(Pinvoke.plugin_key)
            ElseIf sMsg.MessageContent = "崩溃测试" Then
                Dim p As Object = 0
                Dim pnt As New IntPtr(&H123456789)
                Marshal.StructureToPtr(p, pnt, False)
            End If
        End If

        Return 0
    End Function
#End Region


End Module

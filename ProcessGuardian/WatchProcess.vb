Imports System.Threading
Imports System.Windows.Forms

Public Class WatchProcess
    Public Shared Sub StartWatch(ByVal token As CancellationToken, taskPID As Integer)
        Task.Factory.StartNew(Sub()
                                  Do While Not token.IsCancellationRequested
                                      runing = Process.GetProcesses().Any(Function(p) p.Id = taskPID)
                                      Trace.WriteLine($"框架状态：{DateTime.Now}-{runing}")
                                      Try
                                          Form1.MyInstance.Label1.Invoke(New MethodInvoker(Sub() Form1.MyInstance.Label1.Text = $"框架状态：{DateTime.Now}-{runing}"))
                                      Catch ex As Exception

                                      End Try
                                      If Not runing Or Process.GetCurrentProcess.MainWindowTitle.Contains("程序异常") Then
                                          API.restart(Pinvoke.plugin_key)
                                          'KillProcessAndChildren(taskPID)
                                          'taskPID = CreateProcess()
                                      End If
                                      Thread.Sleep(2000)
                                  Loop
                              End Sub, token)
    End Sub

    '供第三方程序调用
    Private workPath = AppDomain.CurrentDomain.BaseDirectory ' Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts")
    'Public Shared Function CreateProcess() As Integer
    '    Dim pid As Integer = -1
    '    Dim psi = New ProcessStartInfo(Path.Combine(workPath, "小栗子框架.exe")) With {
    '        .UseShellExecute = False,
    '        .WorkingDirectory = workPath,
    '        .ErrorDialog = False
    '    }
    '    psi.CreateNoWindow = True
    '    Dim ps = New Process With {.StartInfo = psi}
    '    If ps.Start() Then
    '        pid = ps.Id
    '    End If
    '    Return pid
    'End Function
    'Public Shared Sub KillProcessAndChildren(ByVal pid As Integer)
    '    If pid <= 0 Then
    '        Return
    '    End If
    '    Dim searcher As New ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" & pid)
    '    Dim moc As ManagementObjectCollection = searcher.Get()
    '    For Each mo As ManagementObject In moc
    '        KillProcessAndChildren(Convert.ToInt32(mo("ProcessID")))
    '    Next mo
    '    Try
    '        Dim proc As Process = Process.GetProcessById(pid)
    '        proc.Kill()
    '    Catch e1 As ArgumentException
    '        ' Process already exited.
    '    Catch e2 As Win32Exception
    '        ' Access denied
    '    End Try
    'End Sub
End Class

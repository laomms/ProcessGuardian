Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

Public Class Form1
    Friend Shared MyInstance As Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim PID = Process.GetProcessesByName("小栗子框架")(0).Id
        'Process.GetCurrentProcess().Id
        If runing = False Then
            WatchProcess.StartWatch(cts.Token, PID)
            If _process Is Nothing OrElse _process.HasExited Then
                _process = New Process()
            Else
                _process.Kill()
                _process = Nothing
                Return
            End If
            Dim startInfo As New ProcessStartInfo With {
                .WorkingDirectory = My.Computer.FileSystem.SpecialDirectories.Temp,
                .CreateNoWindow = True,
                .WindowStyle = ProcessWindowStyle.Hidden,
                .FileName = "cmd.exe",
                .Arguments = "/c " + My.Computer.FileSystem.SpecialDirectories.Temp + "\restart.bat"}
            _process.StartInfo = startInfo
            _process.Start()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
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
        Label1.Invoke(New MethodInvoker(Sub() Label1.Text = "已停止进程守护."))
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MyInstance = Me
        Call (New Thread(Sub()
                             Do
                                 Dim status As List(Of String) = CpuMemoryCapacity.GetUsage()
                                 Try
                                     Label3.Invoke(Sub() Label3.Text = String.Join("    ", status))
                                 Catch
                                 End Try
                                 Thread.Sleep(2000)
                             Loop

                         End Sub)).Start()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) '崩溃测试
        Dim PID = Process.GetProcessesByName("小栗子框架")(0).Id
        'Process.GetCurrentProcess().Id
        WatchProcess.StartWatch(cts.Token, PID)
        Dim p As Object = 0
        Dim pnt As New IntPtr(&H123456789)
        Marshal.StructureToPtr(p, pnt, False)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        API.restart(Pinvoke.plugin_key)
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            If workerThread1.IsAlive = True Then Return
            CpuMemoryCapacity.RestartMemoryWork()
            workerThread1 = New Thread(AddressOf CpuMemoryCapacity.DoMemoryWork)
            workerThread1.Start()
        Else
            CpuMemoryCapacity.RequestMemoryStop()
        End If

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            If workerThread2.IsAlive = True Then Return
            CpuMemoryCapacity.RestartCpuWork()
            workerThread2 = New Thread(AddressOf CpuMemoryCapacity.DoCpuWork)
            workerThread2.Start()
        Else
            CpuMemoryCapacity.RequestCpuStop()
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        MaxMemory = TextBox1.Text
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        MaxCPU = TextBox2.Text
    End Sub
End Class
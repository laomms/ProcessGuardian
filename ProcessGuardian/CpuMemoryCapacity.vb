Imports System.Dynamic
Imports System.IO
Imports System.Management
Imports System.Threading

Public Class CpuMemoryCapacity
    Public Shared Function MemoryAvailable() As List(Of String)
        Dim status As New List(Of String)()
        '获取总物理内存大小
        Dim cimobject1 As New ManagementClass("Win32_PhysicalMemory")
        Dim moc1 As ManagementObjectCollection = cimobject1.GetInstances()
        Dim available As Double = 0, capacity As Double = 0
        For Each mo1 As ManagementObject In moc1
            capacity += ((Math.Round(Int64.Parse(mo1.Properties("Capacity").Value.ToString()) / 1024 / 1024 / 1024.0, 1)))
        Next mo1
        moc1.Dispose()
        cimobject1.Dispose()
        '获取内存可用大小
        Dim cimobject2 As New ManagementClass("Win32_PerfFormattedData_PerfOS_Memory")
        Dim moc2 As ManagementObjectCollection = cimobject2.GetInstances()
        For Each mo2 As ManagementObject In moc2
            available += ((Math.Round(Int64.Parse(mo2.Properties("AvailableMBytes").Value.ToString()) / 1024.0, 1)))
        Next mo2
        moc2.Dispose()
        cimobject2.Dispose()
        status.Add("总内存=" & capacity.ToString() & "G")
        status.Add("可使用=" & available.ToString() & "G")
        status.Add("已使用=" & ((capacity - available)).ToString() & "G," & (Math.Round((capacity - available) / capacity * 100, 0)).ToString() & "%")
        Return status
    End Function
    Public Shared Function HardwareInfo() As List(Of String)
        Dim status As New List(Of String)()

        Dim CPUName As String = ""
        Dim mos As New ManagementObjectSearcher("Select * from Win32_Processor") 'Win32_Processor  CPU处理器
        For Each mo As ManagementObject In mos.Get()
            CPUName = mo("Name").ToString()
        Next mo
        mos.Dispose()
        Dim PhysicalMemory As String = ""
        Dim m As New ManagementClass("Win32_PhysicalMemory") '内存条
        Dim mn As ManagementObjectCollection = m.GetInstances()
        PhysicalMemory = "物理内存条数量：" & mn.Count.ToString() & "  "
        Dim capacity As Double = 0.0
        Dim count As Integer = 0
        For Each mo1 As ManagementObject In mn
            count += 1
            capacity = ((Math.Round(Int64.Parse(mo1.Properties("Capacity").Value.ToString()) / 1024 / 1024 / 1024.0, 1)))
            PhysicalMemory &= "第" & count.ToString() & "张内存条大小：" & capacity.ToString() & "G   "
        Next mo1
        mn.Dispose()
        m.Dispose()
        Dim h As New ManagementClass("win32_DiskDrive") '硬盘
        Dim hn As ManagementObjectCollection = h.GetInstances()
        For Each mo1 As ManagementObject In hn
            capacity += Int64.Parse(mo1.Properties("Size").Value.ToString()) \ 1024 \ 1024 \ 1024
        Next mo1
        mn.Dispose()
        m.Dispose()
        status.Add("CPU型号：" & CPUName)
        status.Add("内存状况：" & PhysicalMemory)
        status.Add("硬盘状况：" & "硬盘为：" & capacity.ToString() & "G")
        Return status
    End Function
    Public Shared Function GetUsage() As List(Of String)
        Dim status As New List(Of String)()
        Dim process As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
        Dim cpu = New PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName)
        Dim ram = New PerformanceCounter("Process", "Private Bytes", process.ProcessName, True)
        cpu.NextValue()
        ram.NextValue()
        System.Threading.Thread.Sleep(500)
        status.Add("机器人CPU使用率: " & Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2).ToString() & "%")
        status.Add("机器人使用内存:" & Math.Round(ram.NextValue() / 1024 / 1024, 2).ToString() & "M")
        Return status
    End Function

    Public Shared Function GetMemoryUsage() As List(Of String)
        Dim status As New List(Of String)()
        Using searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfProc_Process Where Name <> '_Total' AND Name <> 'Idle'")
            For Each obj As ManagementObject In searcher.Get()
                status.Add(String.Format("{0:0000.00}", Math.Round(Double.Parse(obj("WorkingSetPrivate").ToString()) / 1024 / 1024, 2)) & "MB (使用内存)  " + "进程名称: " + obj("Name").ToString())
            Next obj
        End Using
        status.Sort()
        status.Reverse()
        Return status.Take(10).ToList()
    End Function
    Public Shared Function GetCpuUsage() As List(Of String)
        Dim status As New List(Of String)()
        'Using searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT Name,PercentProcessorTime,WorkingSetPrivate FROM Win32_PerfFormattedData_PerfProc_Process Where Name <> '_Total' AND Name <> 'Idle'")
        '    For Each obj As ManagementObject In searcher.Get()
        '        status.Add(Math.Round(Double.Parse(obj("PercentProcessorTime").ToString()), 2).ToString() & "% (CPU占用)  " & Math.Round(Double.Parse(obj("WorkingSetPrivate").ToString()) / 1024 / 1024, 2).ToString() & "MB (使用内存)  " & "进程名称: " & obj("Name").ToString())
        '    Next obj
        'End Using

        Dim mos = New ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfProc_Process Where Name <> 'Idle'")
        Dim run1 = mos.Get().Cast(Of ManagementObject)().ToDictionary(Function(mo) mo.Properties("Name").Value, Function(mo) Math.Truncate(mo.Properties("PercentProcessorTime").Value))
        Thread.Sleep(1000)
        Dim run2 = mos.Get().Cast(Of ManagementObject)().ToDictionary(Function(mo) mo.Properties("Name").Value, Function(mo) Math.Truncate(mo.Properties("PercentProcessorTime").Value))

        Dim total = run2("_Total") - run1("_Total")

        For Each kvp In run1
            Dim proc = kvp.Key
            Dim p1 = kvp.Value
            If run2.ContainsKey(proc) Then
                Dim p2 = run2(proc)
                Debug.WriteLine("{0:P}:{1}", (p2 - p1) / total, proc)
                status.Add(String.Format("{0:P}", (p2 - p1) / total) & " (CPU占用)  " & "进程名称: " & proc.ToString())
            End If
        Next

        status.Sort()
        status.Reverse()
        Return status.Take(10).ToList()
    End Function

    Shared Sub DoMemoryWork()
        Do While Not _MemoryWorkStop
            If RamUsage < MaxMemory Then
                Dim process As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
                Dim cpu = New PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName)
                Dim ram = New PerformanceCounter("Process", "Private Bytes", process.ProcessName, True)
                cpu.NextValue()
                ram.NextValue()
                System.Threading.Thread.Sleep(500)
                RamUsage = ram.NextValue() / 1024 / 1024
                Debug.WriteLine(RamUsage.ToString)
            Else
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
                API.restart(Pinvoke.plugin_key)
            End If
        Loop
    End Sub
    Shared Sub DoCpuWork()
        Do While Not _CpuWorkStop
            If CpuUsage < MaxCPU Then
                Dim process As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
                Dim cpu = New PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName)
                Dim ram = New PerformanceCounter("Process", "Private Bytes", process.ProcessName, True)
                cpu.NextValue()
                ram.NextValue()
                System.Threading.Thread.Sleep(500)
                CpuUsage = cpu.NextValue() / Environment.ProcessorCount
                Debug.WriteLine(CpuUsage.ToString)
            Else
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
                API.restart(Pinvoke.plugin_key)
            End If
        Loop
    End Sub
    Shared Sub RequestMemoryStop()
        _MemoryWorkStop = True
    End Sub

    Shared Sub RequestCpuStop()
        _CpuWorkStop = True
    End Sub
    Shared Sub RestartMemoryWork()
        _MemoryWorkStop = False
    End Sub
    Shared Sub RestartCpuWork()
        _CpuWorkStop = False
    End Sub
End Class


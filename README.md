1、vspd添加虚拟串口对

![image](https://github.com/user-attachments/assets/62f757a8-8f6e-488e-9c56-bef6ceb05050)

2、串口调试软件连接串口对的一个串口 此处为COM3

![image](https://github.com/user-attachments/assets/ae72a679-d6b5-4eeb-afcb-b680ec19e96a)

3、开启串口对的自动回复

指令匹配模板：3C 28 00 01 00 12 02 F2 13 03 3F 55 00 00 BC EF 29 3E 

指令应答模板：3C 28 00 01 01 A2 02 13 F2 03 3F AA 01 90 00 00 00 00 00 00 00 00 00 00 00 00 01 01 01 C1 FA D7 60 BF DC B6 1A C0 23 4E C0 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 01 03 E8 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 42 20 00 00 00 00 11 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 6B 42 29 3E 

匹配和应答都选择HEX，编辑完成后确定，勾选新增的模板

![image](https://github.com/user-attachments/assets/e4f9651b-f12f-46cc-bc27-45b08555ec18)

4、运行demo连接串口对的另一个串口 此处为COM2
![image](https://github.com/user-attachments/assets/582839d5-60e7-4f81-9f7d-355231dcfdb3)

5、运行一段时间后报异常

System.InvalidOperationException:“Operation is not valid due to the current state of the object.”

![1744099649272](https://github.com/user-attachments/assets/1a76dcbf-6b8f-4613-9276-6b33b7e94800)

异常堆栈：
```
在 System.ThrowHelper.ThrowInvalidOperationException() 在 /_/src/libraries/System.Private.CoreLib/src/System/ThrowHelper.cs 中: 第 343 行
在 System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1.SignalCompletion() 在 /_/src/libraries/System.Private.CoreLib/src/System/Threading/Tasks/Sources/ManualResetValueTaskSourceCore.cs 中: 第 214 行
在 System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1.SetException(Exception error) 在 /_/src/libraries/System.Private.CoreLib/src/System/Threading/Tasks/Sources/ManualResetValueTaskSourceCore.cs 中: 第 76 行
在 TouchSocket.SerialPorts.SerialCore.SerialCore_DataReceived(Object sender, SerialDataReceivedEventArgs e)
在 System.IO.Ports.SerialPort.CatchReceivedEvents(Object src, SerialDataReceivedEventArgs e)
在 System.IO.Ports.SerialStream.EventLoopRunner.CallReceiveEvents(Object state)
在 System.Threading.ThreadPoolWorkQueue.Dispatch() 在 /_/src/libraries/System.Private.CoreLib/src/System/Threading/ThreadPoolWorkQueue.cs 中: 第 1010 行
在 System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart() 在 /_/src/libraries/System.Private.CoreLib/src/System/Threading/PortableThreadPool.WorkerThread.NonBrowser.cs 中: 第 102 行
```

两个类似的runtime的bug

https://github.com/dotnet/runtime/issues/55249

https://github.com/dotnet/runtime/issues/68623

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace SerialPortCrashDemo;

public class ComPort
{
    private FrameSequenceGenerator _frameSequenceGenerator = new FrameSequenceGenerator();
    private SerialPortClient _client;
    private readonly FrameParser _parser;
    private const int HeartbeatIntervalMs = 2000; // 2 秒
    private Task _heartbeatTask;
    public bool ClientOnline => _client.Online;

    public ComPort(FrameParser parser)
    {
        _client = new SerialPortClient();
        _parser = parser;
    }

    public async Task InitializeAsync(string portName, int baudRate)
    {
        // 设置事件回调
        _client.Connecting = (client, e) => { return EasyTask.CompletedTask; }; // 即将连接到端口
        _client.Connected = async (client, e) =>
        {
            // 连接成功后启动心跳任务
            _heartbeatTask = Task.Run(HeartbeatLoop);
            await EasyTask.CompletedTask;
        };
        _client.Closing = (client, e) => { return EasyTask.CompletedTask; };   // 即将从端口断开连接
        _client.Closed = async (client, e) =>
        {
            // 断开时等待心跳任务停止
            if (_heartbeatTask != null)
            {
                await _heartbeatTask;
                _heartbeatTask = null;
            }
            await EasyTask.CompletedTask;
        };

        // 接收数据事件
        _client.Received = async (c, e) =>
        {
            if (e.RequestInfo is SvtRequestInfo myRequest)
            {
                Debug.WriteLine($"已从{BitConverter.ToString(myRequest.FrameNo)}接收到：CMDID={string.Join(" ", myRequest.CMDID.Select(b => b.ToString("X2")))}," +
                    $"FrameType=0x{myRequest.FrameType:X2},消息={string.Join(" ", myRequest.Data.Select(b => b.ToString("X2")))}");
                await ProcessReceivedDataAsync(myRequest);
            }
        };

        // 配置串口参数
        await _client.SetupAsync(new TouchSocketConfig()
            .SetSerialPortOption(new SerialPortOption()
            {
                PortName = portName,       // 串口号
                BaudRate = baudRate,       // 波特率
                DataBits = 8,              // 数据位
                Parity = System.IO.Ports.Parity.None, // 校验位
                StopBits = System.IO.Ports.StopBits.One // 停止位
            })
            .SetSerialDataHandlingAdapter(() => new SvtDataHandlingAdapter()) // 数据适配器
            .ConfigurePlugins(a =>
            {
                a.Add<MyConnectingPlugin>();
                a.Add<MyConnectedPlugin>();
                a.Add<MyReceivedPlugin>();
                a.Add<MyClosedPlugin>();
            }));

        // 连接串口
        //await _client.ConnectAsync();
        //Debug.WriteLine("串口连接成功");
        try
        {
            // 连接串口
            await _client.ConnectAsync();
            Debug.WriteLine($"串口 {portName} 连接成功");
        }
        catch (System.IO.FileNotFoundException ex)
        {
            Debug.WriteLine($"串口 {portName} 不存在: {ex.Message}");
            //throw new Exception($"找不到串口 {portName}", ex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"串口连接失败: {ex.Message}");
            throw;
        }
    }

    public async Task ClientSafeCloseAsync()
    {
        await _client.SafeCloseAsync();
    }

    public IWaitingClient<ISerialPortClient, IReceiverResult> CreateWaitingClient(WaitingOptions options)
    {
        var client = _client!.CreateWaitingClient(options);
        return client;
    }

    // 异步处理数据的示例方法
    private async Task ProcessReceivedDataAsync(SvtRequestInfo request)
    {
        // 模拟异步操作，如存储数据或发送响应
        //await Task.Delay(100); // Simulate I/O or processing delay
        await this._parser.Route(request);
        Debug.WriteLine($"处理完成：来自{request.MasterAddress}的消息已处理。");
    }


    internal class MyConnectingPlugin : PluginBase, ISerialConnectingPlugin
    {
        public async Task OnSerialConnecting(ISerialPortSession client, ConnectingEventArgs e)
        {
            Debug.WriteLine("准备连接串口");
            await e.InvokeNext();
        }
    }

    internal class MyConnectedPlugin : PluginBase, ISerialConnectedPlugin
    {
        public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    internal class MyReceivedPlugin : PluginBase, ISerialReceivedPlugin
    {
        public async Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e)
        {
            //这里处理数据接收
            //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
            var byteBlock = e.ByteBlock;
            var requestInfo = e.RequestInfo;

            //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。

            await e.InvokeNext();
        }
    }

    internal class MyClosedPlugin : PluginBase, ISerialClosedPlugin
    {
        public async Task OnSerialClosed(ISerialPortSession client, ClosedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    public byte[] GetFrameNumber()
    {
        return _frameSequenceGenerator.GenerateFrameSequence();
    }

    public async Task SendPacketAsync(SvtRequestInfo packet)
    {
        //await Task.Delay(200);
        packet.FrameNo = GetFrameNumber();
        Debug.WriteLine($"SendPacketAsync: {string.Join(" ", packet.DataFrame().Select(b => b.ToString("X2")))}");
        await this._client.SendAsync(packet);
    }

    /// <summary>
    /// 心跳循环
    /// </summary>
    /// <returns></returns>
    private async Task HeartbeatLoop()
    {
        while (_client.Online)
        {
            var heartbeat = new HeartBeatRead();
            await SendPacketAsync(heartbeat);
            try
            {
                await Task.Delay(HeartbeatIntervalMs);
            }
            catch (TaskCanceledException)
            {
                // Task.Delay 可能会在程序关闭时抛出这个异常
                Debug.WriteLine("心跳循环中的延迟被取消。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"心跳循环发生异常: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5)); // 发生错误后等待一段时间再重试
            }
        }
        Debug.WriteLine("心跳循环已停止（连接断开）。");
    }
}

public class FrameSequenceGenerator
{
    //private int _counter = 0; // 计数器，用于生成序号
    //private const int MaxSequence = 65535; // 最大序号值

    //// 生成下一个序号
    //public byte[] GenerateFrameSequence()
    //{
    //    // 计算下一个序号
    //    _counter = (_counter % MaxSequence) + 1;

    //    // 将序号转换为大端序的字节数组
    //    byte[] sequenceBytes = BitConverter.GetBytes((ushort)_counter);
    //    if (BitConverter.IsLittleEndian)
    //    {
    //        Array.Reverse(sequenceBytes); // 如果是小端序，反转字节数组
    //    }

    //    return sequenceBytes;
    //}
    public byte[] GenerateFrameSequence()
    {
        byte[] sequenceBytes = new byte[] { 0x00, 0x01 };
        return sequenceBytes;
    }
}

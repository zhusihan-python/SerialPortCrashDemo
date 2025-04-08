using System;
using System.Collections.Generic;
using System.Linq;
using TouchSocket.Core;

namespace SerialPortCrashDemo;

public class SvtDataHandlingAdapter : CustomDataHandlingAdapter<SvtRequestInfo>
{
    // 数据为空的包 最小长度为18
    public int HeaderLength = 18;
    public override bool CanSendRequestInfo => true;

    /// <summary>
    /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
    /// </summary>
    /// <param name="byteBlock">字节块</param>
    /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
    /// <param name="request">对象。</param>
    /// <param name="tempCapacity">缓存容量指导，指示当需要缓存时，应该申请多大的内存。</param>
    /// <returns></returns>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref SvtRequestInfo request, ref int tempCapacity)
    {
        if (this.HeaderLength > byteBlock.CanReadLength)
        {
            this.SurLength = this.HeaderLength - byteBlock.CanReadLength;
            return FilterResult.Cache;
        }

        var span = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength);
        // 先找包尾，有可能有多个包尾，取第一个
        var tailIndex = span.IndexOfFirst(0, span.Length, Svt.FullTail);
        if (tailIndex > 0)
        {
            // 从包尾往前找包头
            var headIndexes = span.IndexOfInclude(0, tailIndex - Svt.FullTail.Length, Svt.FullHead);
            // 找到取第一个
            if (headIndexes.Count > 0)
            {
                // 取距离包尾最近的包头
                var lastHeadIndex = headIndexes[^1];
                //var pos = byteBlock.Position;//记录初始游标位置，防止本次无法解析时，回退游标。
                // 去掉包头包尾
                var package = span.Slice(lastHeadIndex + 1, tailIndex + 1 - Svt.FullTail.Length - Svt.FullHead.Length);
                // 去掉转义字符，才能校验包长度和CRC
                var cleanPackage = RemoveInsertedBytes(package);
                var myRequestInfo = new SvtRequestInfo();
                // 清理之后包长度，不含包头包尾最少是14
                if (cleanPackage.Length >= this.HeaderLength - 4)
                {
                    int packetPos = 0;

                    // Read FrameNo (2 byte)
                    myRequestInfo.FrameNo = cleanPackage.Slice(packetPos, 2).ToArray();
                    packetPos += 2;

                    // Read PacketLength (2 byte)
                    myRequestInfo.PacketLength = TouchSocketBitConverter.BigEndian.ToUInt16(cleanPackage.Slice(packetPos, 2).ToArray(), 0);
                    packetPos += 2;

                    // Read AddressLength (1 byte)
                    myRequestInfo.AddressLength = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.MasterAddress = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.SlaveAddress = cleanPackage[packetPos];
                    packetPos += 1;

                    // Read CMDID (2 bytes)
                    myRequestInfo.CMDID = cleanPackage.Slice(packetPos, 2).ToArray();
                    packetPos += 2;

                    // Read FrameType (1 byte)
                    myRequestInfo.FrameType = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.DataLength = (ushort)((cleanPackage[packetPos] << 8) | cleanPackage[packetPos + 1]);
                    packetPos += 2;

                    // 数据长度校验，从包尾往前推，该校验可以去掉
                    if (packetPos + myRequestInfo.DataLength + 2 > cleanPackage.Length)
                    {
                        byteBlock.Position += tailIndex;
                        return FilterResult.GoOn;
                    }

                    myRequestInfo.Data = cleanPackage.Slice(packetPos, myRequestInfo.DataLength).ToArray();
                    packetPos += myRequestInfo.DataLength;

                    // Read CRC (1 byte)
                    myRequestInfo.CRC16 = cleanPackage.Slice(packetPos, 2).ToArray();
                    packetPos += 2;

                    if (Crc16.ComputeCrc(cleanPackage, cleanPackage.Length - 2) ==
                            BitConverter.ToUInt16(myRequestInfo.CRC16.Reverse().ToArray(), 0))
                    {
                        request = myRequestInfo;
                        byteBlock.Position += tailIndex + 1;
                        return FilterResult.Success;
                    }
                    else
                    {
                        // CRC校验失败，无效包，移动Position到该包尾的位置
                        byteBlock.Position += tailIndex;
                        return FilterResult.GoOn;
                    }
                }
                else
                {
                    // 包有效长度不够，移动Position到该包尾的位置
                    byteBlock.Position += tailIndex;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                // 找不到则移动Position到该包尾的位置
                byteBlock.Position += tailIndex;
                return FilterResult.GoOn;
            }
        }
        else
        {
            //byteBlock.Position += this.HeaderLength;
            return FilterResult.Cache;
        }
    }

    private ReadOnlySpan<byte> RemoveInsertedBytes(ReadOnlySpan<byte> data)
    {
        var cleanedData = new List<byte>();

        for (int i = 0; i < data.Length; i++)
        {
            // 添加当前字节
            cleanedData.Add(data[i]);

            // 检查是否需要跳过下一个字节
            if (i + 1 < data.Length)
            {
                if (data[i] == Svt.StartByte && data[i + 1] == Svt.InsertStartByte)
                {
                    // 跳过 0x82
                    i++;
                }
                else if (data[i] == Svt.EndByte && data[i + 1] == Svt.InsertEndByte)
                {
                    // 跳过 0x83
                    i++;
                }
            }
        }

        // 将列表转换为字节数组
        return cleanedData.ToArray().AsSpan();
    }
}
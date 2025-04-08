using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace SerialPortCrashDemo;

public class FrameParser
{
    //private readonly IServiceProvider _serviceProvider;
    //public FrameParser(
    //    IServiceProvider serviceProvider
    //)
    //{
    //    this._serviceProvider = serviceProvider;
    //}
    public async Task Route(SvtRequestInfo request)
    {
        if (Enumerable.SequenceEqual(request.CMDID, Svt.HeartBeat) && request.FrameType == Svt.ReadSuccess)
        {
            await Task.Run(() => HeartBeatParse(request));
        }
    }

    private void HeartBeatParse(SvtRequestInfo request)
    {
        //var device = this._serviceProvider.GetRequiredService<Device>();
        //var span = new ReadOnlySpan<byte>(request.Data);
        //if (span.Length >= 400)
        //{
        //    device!.DeviceResetState.Value = (DeviceResetType)span[0];
        //    device.SealMotorResetState.Value = (SealMotorResetType)span[1];
        //    device.SortMotorResetState.Value = (SortMotorResetType)span[2];
        //    device.SealMotorFlowState.Value = (SealMotorFlowType)span[3];
        //    device.SortMotorFlowState.Value = (SortMotorFlowType)span[4];
        //    device.DeviceActionState.Value = (DeviceActionType)span[5];
        //    device.ScanTargetIndex.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(6, 2).ToArray(), 0);
        //    device.ActionPackNumber.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(8, 2).ToArray(), 0);
        //    device.ActionPackCount.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(10, 2).ToArray(), 0);
        //    device.MotorBoardOneState.Value = span[12] != 0;
        //    device.MotorBoardTwoState.Value = span[13] != 0;
        //    device.EnvironBoardState.Value = span[14] != 0;
        //    device.GasTankPressure.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(15, 4).ToArray(), 0);
        //    device.SuckerOnePressure.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(19, 4).ToArray(), 0);
        //    device.SuckerTwoPressure.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(23, 4).ToArray(), 0);
        //    device.BakeState.Value = span[27] != 0;
        //    device.BakeTargetTemp.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(28, 4).ToArray(), 0);
        //    device.BakeRealTemp.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(32, 4).ToArray(), 0);
        //    device.BakeTargetDuration.Value = TouchSocketBitConverter.BigEndian.ToUInt32(span.Slice(36, 4).ToArray(), 0);
        //    device.BakeLeftDuration.Value = TouchSocketBitConverter.BigEndian.ToUInt32(span.Slice(40, 4).ToArray(), 0);
        //    device.WasteBoxInPlace.Value = span[44] != 0;
        //    device.CoverBoxInPlace.Value = span[45] != 0;
        //    device.CoverBoxLeftCount.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(46, 2).ToArray(), 0);
        //    var alarmCodesArray = span.Slice(48, 20).ToArray();
        //    var alarmCodes = Enumerable.Range(0, 10)
        //             .Select(i => TouchSocketBitConverter.BigEndian.ToUInt16(alarmCodesArray.AsSpan().Slice(i * 2, 2).ToArray(), 0));
        //    device.AlarmCodes.AddLastRange(alarmCodes);
        //    var slideBoxInPlaceBits = BitsConverter.BytesToBits(span.Slice(68, 10).ToArray(), 75);
        //    for (int i = 0; i < slideBoxInPlaceBits.Length; i++)
        //    {
        //        device.SlideBoxInPlace[i] = slideBoxInPlaceBits[i];
        //    }
        //    var SlideInPlaceBits = BitsConverter.BytesToBits(span.Slice(78, 188).ToArray(), 1500);
        //    for (int j = 0; j < SlideInPlaceBits.Length; j++)
        //    {
        //        device.SlideInPlace[j] = SlideInPlaceBits[j];
        //    }
        //    for (int k = 0; k < 75; k++)
        //    {
        //        device.SlideBoxActions[k] = (SlideBoxActionType)span[k + 266];
        //    }
        //}
        Debug.WriteLine("Finish HeartBeatParse");
    }
}


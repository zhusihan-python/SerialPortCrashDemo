namespace SerialPortCrashDemo;

public class HeartBeatRead : SvtRequestInfo
{
    public HeartBeatRead()
    {
        this.CMDID = Svt.HeartBeat;
        this.FrameType = Svt.Read;
        this.DataLength = 0;
    }
}

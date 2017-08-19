using System;

namespace uOSC
{

public struct Timestamp
{
    public static readonly Timestamp Immediate = new Timestamp(0x1u);

    public UInt64 value;

    public Timestamp(UInt64 value)
    {
        this.value = value;
    }

    public DateTime ToUtcTime()
    {
        var integerPart = (UInt32)((value >> 32) & 0xFFFFFFFF); 
        var decimalPart = (UInt32)(value & 0xFFFFFFFF); 
        var msec = (UInt32)(((Double)decimalPart / UInt32.MaxValue) * 1000); 
        var baseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return baseDate.AddSeconds(integerPart).AddMilliseconds(msec);
    }

    public DateTime ToLocalTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(ToUtcTime(), TimeZoneInfo.Local);
    }
}

}

using System;

namespace uOSC
{

public struct Timestamp
{
    public UInt64 value;

    public Timestamp(UInt64 value)
    {
        this.value = value;
    }

    public static readonly Timestamp Immediate = new Timestamp(0x1u);

    public static Timestamp Now
    {
        get { return Timestamp.CreateFromDateTime(DateTime.UtcNow); }
    }

    public static Timestamp CreateFromDateTime(DateTime time)
    {
        var span = time - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sec = span.TotalSeconds;
        var msec = span.TotalMilliseconds - sec * 1000;
        var integerPart = (UInt32)sec;
        var decimalPart = (UInt32)msec;
        var timestamp = ((UInt64)integerPart << 32) | (UInt64)decimalPart;
        return new Timestamp(timestamp);
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
#if NETFX_CORE
        return TimeZoneInfo.ConvertTime(ToUtcTime(), TimeZoneInfo.Local);
#else
        var utc = ToUtcTime();
        var timeZone = System.TimeZone.CurrentTimeZone;
        return utc + timeZone.GetUtcOffset(DateTime.Now);
#endif
    }
}

}

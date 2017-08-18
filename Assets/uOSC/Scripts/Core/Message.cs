using System;

namespace uOSC
{

public struct NtpTimestamp
{
    public UInt64 value;

    public NtpTimestamp(UInt64 value = 0x1u)
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

public struct Message
{
    public string address;
    public NtpTimestamp timestamp;
    public object[] values;

    public static Message none
    {
        get 
        { 
            return new Message() { 
                address = "", 
                timestamp = new NtpTimestamp(),
                values = null
            };
        }
    }
}

}
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace uOSC
{

public struct Message
{
    public string address;
    public Timestamp timestamp;
    public object[] packet;

    public static Message none
    {
        get 
        { 
            return new Message() { 
                address = "", 
                timestamp = new Timestamp(),
                packet = null
            };
        }
    }

    public void Write(MemoryStream stream)
    {
        WriteAddress(stream);
        WriteTypes(stream);
        WriteValues(stream);
    }

    void FillZeros(MemoryStream stream, int preBufferSize, bool isString)
    {
        var bufferSize = Util.GetStringOffset(preBufferSize);

        var size = bufferSize - preBufferSize;
        if (isString && size == 0)
        {
            size = 4;
        }

        if (size > 0)
        {
            stream.Write(Util.Zeros, 0, size);
        }
    }

    void WriteAddress(MemoryStream stream)
    {
        var byteAddress = Encoding.UTF8.GetBytes(address);
        stream.Write(byteAddress, 0, byteAddress.Length);
        FillZeros(stream, byteAddress.Length, true);
    }

    void WriteTypes(MemoryStream stream)
    {
        string types = ",";
        for (int i = 0; i < packet.Length; ++i)
        {
            var value = packet[i];
            if      (value is int)    types += Identifier.Int;
            else if (value is float)  types += Identifier.Float;
            else if (value is string) types += Identifier.String;
            else if (value is byte[]) types += Identifier.Blob;
        }

        var byteTypes = Encoding.UTF8.GetBytes(types);
        stream.Write(byteTypes, 0, byteTypes.Length);
        FillZeros(stream, byteTypes.Length, true);
    }

    void WriteValues(MemoryStream stream)
    {
        for (int i = 0; i < packet.Length; ++i)
        {
            var value = packet[i];
            if      (value is int)    Write(stream, value.AsInt());
            else if (value is float)  Write(stream, value.AsFloat());
            else if (value is string) Write(stream, value.AsString());
            else if (value is byte[]) Write(stream, value.AsBlob());
        }
    }

    void Write(MemoryStream stream, int value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    void Write(MemoryStream stream, float value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    void Write(MemoryStream stream, string value)
    {
        var byteValue = Encoding.UTF8.GetBytes(value);
        stream.Write(byteValue, 0, byteValue.Length);
        FillZeros(stream, byteValue.Length, true);
    }

    void Write(MemoryStream stream, byte[] value)
    {
        var byteValue = value.AsBlob();
        var size = byteValue.Length;
        Write(stream, size);
        stream.Write(byteValue, 0, byteValue.Length);
        FillZeros(stream, byteValue.Length, false);
    }
}

}
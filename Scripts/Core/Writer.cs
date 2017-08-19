using System;
using System.IO;
using System.Text;

namespace uOSC
{

public static class Writer
{
    static void FillZeros(MemoryStream stream, int preBufferSize, bool isString)
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

    public static void WriteString(MemoryStream stream, string str)
    {
        var byteStr = Encoding.UTF8.GetBytes(str);
        stream.Write(byteStr, 0, byteStr.Length);
        FillZeros(stream, byteStr.Length, true);
    }

    public static void Write(MemoryStream stream, int value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    public static void Write(MemoryStream stream, float value)
    {
        var byteValue = BitConverter.GetBytes(value);
        Array.Reverse(byteValue);
        stream.Write(byteValue, 0, byteValue.Length);
    }

    public static void Write(MemoryStream stream, string value)
    {
        var byteValue = Encoding.UTF8.GetBytes(value);
        stream.Write(byteValue, 0, byteValue.Length);
        FillZeros(stream, byteValue.Length, true);
    }

    public static void Write(MemoryStream stream, byte[] value)
    {
        var byteValue = value.AsBlob();
        var size = byteValue.Length;
        Write(stream, size);
        stream.Write(byteValue, 0, byteValue.Length);
        FillZeros(stream, byteValue.Length, false);
    }
}

}
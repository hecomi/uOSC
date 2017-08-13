using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace uOSC
{

public class OscClient : MonoBehaviour
{
    private const int BufferSize = 8192;
    public static readonly byte zero = System.Convert.ToByte('\0');
    public static readonly byte[] zeros = { zero, zero, zero, zero };

    [SerializeField]
    string address = "127.0.0.1";

    [SerializeField]
    int port = 3333;

    UdpClient udpClient_;
    IPEndPoint endPoint_;

    void OnEnable()
    {
        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
    }

    void OnDisable()
    {
        udpClient_.Close();
    }

    void FillZeros(MemoryStream stream, int preBufferSize, bool isString)
    {
        var bufferSize = OscUtil.ConvertOffsetToMultipleOfFour(preBufferSize);

        var size = bufferSize - preBufferSize;
        if (isString && size == 0)
        {
            size = 4;
        }

        if (size > 0)
        {
            stream.Write(zeros, 0, size);
        }
    }

    void WriteAddress(MemoryStream stream, string address)
    {
        var byteAddress = Encoding.UTF8.GetBytes(address);
        stream.Write(byteAddress, 0, byteAddress.Length);
        FillZeros(stream, byteAddress.Length, true);
    }

    void WriteTypes(MemoryStream stream, object[] values)
    {
        string types = ",";
        for (int i = 0; i < values.Length; ++i)
        {
            var type = values[i].GetType();
            if      (type == typeof(int))    types += "i";
            else if (type == typeof(float))  types += "f";
            else if (type == typeof(string)) types += "s";
            else if (type == typeof(byte[])) types += "b";
        }

        var byteTypes = Encoding.UTF8.GetBytes(types);
        stream.Write(byteTypes, 0, byteTypes.Length);
        FillZeros(stream, byteTypes.Length, true);
    }

    void WriteValues(MemoryStream stream, object[] values)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            var value = values[i];
            var type = values[i].GetType();
            if (type == typeof(int))
            {
                var byteValue = BitConverter.GetBytes(value.AsInt());
                Array.Reverse(byteValue);
                stream.Write(byteValue, 0, byteValue.Length);
            }
            else if (type == typeof(float))
            {
                var byteValue = BitConverter.GetBytes(value.AsFloat());
                Array.Reverse(byteValue);
                stream.Write(byteValue, 0, byteValue.Length);
            }
            else if (type == typeof(string))
            {
                var byteValue = Encoding.UTF8.GetBytes(value.AsString());
                stream.Write(byteValue, 0, byteValue.Length);
                FillZeros(stream, byteValue.Length, true);
            }
            else if (type == typeof(byte[]))
            {
                var byteValue = value.AsBlob();
                stream.Write(byteValue, 0, byteValue.Length);
                FillZeros(stream, byteValue.Length, false);
            }
        }
    }

    public void Send(string address, params object[] values)
    {
        using (var stream = new MemoryStream(BufferSize))
        {
            WriteAddress(stream, address);
            WriteTypes(stream, values);
            WriteValues(stream, values);

            var buffer = stream.GetBuffer();
            udpClient_.Send(buffer, buffer.Length, endPoint_);
        }
    }
}

}
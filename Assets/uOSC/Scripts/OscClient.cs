using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace uOSC
{

public class OscClient : MonoBehaviour
{
    private const int BufferSize = 8192;

    [SerializeField]
    string address = "127.0.0.1";

    [SerializeField]
    int port = 3333;

    UdpClient udpClient_;
    IPEndPoint endPoint_;
    OscThread thread_ = new OscThread();
    Queue<OscMessage> messages_ = new Queue<OscMessage>();

    void OnEnable()
    {
        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
        thread_.Start(UpdateSend);
    }

    void OnDisable()
    {
        thread_.Stop();
        udpClient_.Close();
    }

    void UpdateSend()
    {
        while (messages_.Count > 0)
        {
            var message = messages_.Dequeue();
            var address = message.address;
            var values = message.values;

            using (var stream = new MemoryStream(BufferSize))
            {
                WriteAddress(stream, address);
                WriteTypes(stream, values);
                WriteValues(stream, values);
                Send(stream);
            }
        }
    }

    public void Send(string address, params object[] values)
    {
        messages_.Enqueue(new OscMessage() 
        {
            address = address,
            values = values
        });
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
            stream.Write(OscUtil.zeros, 0, size);
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

    void Send(MemoryStream stream)
    {
        var buffer = stream.GetBuffer();
        udpClient_.Send(buffer, buffer.Length, endPoint_);
    }
}

}
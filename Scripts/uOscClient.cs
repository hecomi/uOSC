using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace uOSC
{

public class uOscClient : MonoBehaviour
{
    private const int BufferSize = 8192;

    [SerializeField]
    string address = "127.0.0.1";

    [SerializeField]
    int port = 3333;

#if UNITY_UWP
    // TODO: implement
#else
    Udp udp_ = new UdpDotNet();
#endif
    Thread thread_ = new Thread();
    Queue<Message> messages_ = new Queue<Message>();
    object lockObject_ = new object();

    void OnEnable()
    {
        udp_.StartClient(address, port);
        thread_.Start(UpdateSend);
    }

    void OnDisable()
    {
        thread_.Stop();
        udp_.Stop();
    }

    void UpdateSend()
    {
        while (messages_.Count > 0)
        {
            Message message;
            lock (lockObject_)
            {
                message = messages_.Dequeue();
            }
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
        var message = new Message() 
        {
            address = address,
            values = values
        };
        lock (lockObject_)
        {
            messages_.Enqueue(message);
        }
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
            stream.Write(Util.zeros, 0, size);
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
            var value = values[i];
            if      (value is int)    types += "i";
            else if (value is float)  types += "f";
            else if (value is string) types += "s";
            else if (value is byte[]) types += "b";
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

    void Send(MemoryStream stream)
    {
        var buffer = stream.GetBuffer();
        udp_.Send(buffer, (int)stream.Position);
    }
}

}
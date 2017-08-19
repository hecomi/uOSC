using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace uOSC
{

public class Parser
{
    object lockObject_ = new object();
    Queue<Message> messages_ = new Queue<Message>();

    public int messageCount
    {
        get { return messages_.Count; }
    }

    public void Parse(byte[] buf, ref int pos, int endPos, ulong timestamp = 0x1u)
    {
        var first = ParseString(buf, ref pos);

        if (first == Identifier.Bundle)
        {
            ParseBundle(buf, ref pos, endPos);
        }
        else
        {
            var packet = ParseData(buf, ref pos);
            lock (lockObject_)
            {
                messages_.Enqueue(new Message() 
                {
                    address = first,
                    timestamp = new Timestamp(timestamp),
                    packet = packet
                });
            }
        }

        if (pos != endPos)
        {
            Debug.LogError("The parsed data size is inconsitent with given size.");
        }
    }

    public Message Dequeue()
    {
        if (messageCount == 0)
        {
            return Message.none;
        }

        lock (lockObject_)
        {
            return messages_.Dequeue();
        }
    }

    void ParseBundle(byte[] buf, ref int pos, int endPos)
    {
        var time = ParseTimetag(buf, ref pos);

        while (pos < endPos)
        {
            var contentSize = ParseInt(buf, ref pos);
            if (Util.IsMultipleOfFour(contentSize))
            {
                Parse(buf, ref pos, pos + contentSize, time);
            }
            else
            {
                Debug.LogErrorFormat("Given data is invalid (bundle size ({0}) is not a multiple of 4).", contentSize);
                pos += contentSize;
            }
        }
    }

    object[] ParseData(byte[] buf, ref int pos)
    {
        // remove ','
        var types = ParseString(buf, ref pos).Substring(1);

        var n = types.Length;
        if (n == 0) return Util.EmptyObjectArray;

        var data = new object[n];

        for (int i = 0; i < n; ++i)
        {
            switch (types[i])
            {
                case Identifier.Int    : data[i] = ParseInt(buf, ref pos);    break;
                case Identifier.Float  : data[i] = ParseFloat(buf, ref pos);  break;
                case Identifier.String : data[i] = ParseString(buf, ref pos); break;
                case Identifier.Blob   : data[i] = ParseBlob(buf, ref pos);   break;
                default:
                    // Add more types here if you want to handle them.
                    break;
            }
        }

        return data;
    }

    string ParseString(byte[] buf, ref int pos)
    {
        int size = 0;
        int bufSize = buf.Length;
        for (; buf[pos + size] != 0; ++size);
        var value = Encoding.UTF8.GetString(buf, pos, size);
        pos += Util.GetStringOffset(size);
        return value;
    }

    int ParseInt(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToInt32(buf, pos);
        pos += 4;
        return value;
    }

    float ParseFloat(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToSingle(buf, pos);
        pos += 4;
        return value;
    }

    byte[] ParseBlob(byte[] buf, ref int pos)
    {
        var size = ParseInt(buf, ref pos);
        var value = new byte[size];
        Buffer.BlockCopy(buf, pos, value, 0, size);
        pos += Util.GetBufferOffset(size);
        return value;
    }

    ulong ParseTimetag(byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 8);
        var value = BitConverter.ToUInt64(buf, pos);
        pos += 8;
        return value;
    }
}

}
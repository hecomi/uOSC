using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace uOSC
{

public class OscParser
{
    object lockObject_ = new object();
    Queue<OscMessage> messages_ = new Queue<OscMessage>();

    public int messageCount
    {
        get { return messages_.Count; }
    }

    public void Parse(Byte[] buf, bool clearOldData = false)
    {
        if (clearOldData)
        {
            messages_.Clear();
        }

        lock (lockObject_)
        {
            int pos = 0;
            messages_.Enqueue(new OscMessage() 
            {
                address = ParseAddress(buf, ref pos),
                values = ParseData(buf, ref pos)
            });
        }
    }

    public OscMessage Dequeue()
    {
        if (messageCount == 0)
        {
            return OscMessage.none;
        }

        lock (lockObject_)
        {
            return messages_.Dequeue();
        }
    }

    string ParseAddress(Byte[] buf, ref int pos)
    {
        return ParseString(buf, ref pos);
    }

    object[] ParseData(Byte[] buf, ref int pos)
    {
        var types = ParseString(buf, ref pos).Substring(1);

        var n = types.Length;
        if (n == 0) return null;

        var data = new object[n];

        for (int i = 0; i < n; ++i)
        {
            switch (types[i])
            {
                case 'i': data[i] = ParseInt(buf, ref pos);    break;
                case 'f': data[i] = ParseFloat(buf, ref pos);  break;
                case 's': data[i] = ParseString(buf, ref pos); break;
                case 'b': data[i] = ParseBlob(buf, ref pos);   break;
                default:
                    // Add more types here if you want to handle them.
                    break;
            }
        }

        return data;
    }

    string ParseString(Byte[] buf, ref int pos)
    {
        int size = 0;
        for (; buf[pos + size] != 0; ++size);
        var value = Encoding.UTF8.GetString(buf, pos, size);
        pos += OscUtil.ConvertOffsetToMultipleOfFour(size);
        return value;
    }

    int ParseInt(Byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToInt32(buf, pos);
        pos += 4;
        return value;
    }

    float ParseFloat(Byte[] buf, ref int pos)
    {
        Array.Reverse(buf, pos, 4);
        var value = BitConverter.ToSingle(buf, pos);
        pos += 4;
        return value;
    }

    Byte[] ParseBlob(Byte[] buf, ref int pos)
    {
        var size = ParseInt(buf, ref pos);
        var tmp = new Byte[size];
        Buffer.BlockCopy(buf, pos, tmp, 0, size);
        pos += OscUtil.ConvertOffsetToMultipleOfFour(size);
        return tmp;
    }
}

}
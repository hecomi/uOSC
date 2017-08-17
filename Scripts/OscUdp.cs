using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public abstract class OscUdp
{
    protected Queue<byte[]> messageQueue_ = new Queue<byte[]>();
    protected object lockObject_ = new object();

    public int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public abstract void StartServer(int port);
    public abstract void StartClient(string address, int port);
    public abstract void UpdateServer();
    public abstract void Send(byte[] data, int size);
    public abstract void Stop();

    public byte[] Receive()
    {
        byte[] buffer;
        lock (lockObject_)
        {
            buffer = messageQueue_.Dequeue();
        }
        return buffer;
    }
}

}
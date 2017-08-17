using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public class OscUdp
{
    Queue<byte[]> messageQueue_ = new Queue<byte[]>();

    UdpClient udpClient_;
    IPEndPoint endPoint_;
    object lockObject_ = new object();

    public int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public void StartServer(int port)
    {
        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);
    }

    public void UpdateServer()
    {
        while (udpClient_.Available > 0) 
        {
            var buffer = udpClient_.Receive(ref endPoint_);
            lock (lockObject_)
            {
                messageQueue_.Enqueue(buffer);
            }
        }
    }

    public byte[] Receive()
    {
        byte[] buffer;
        lock (lockObject_)
        {
            buffer = messageQueue_.Dequeue();
        }
        return buffer;
    }

    public void StartClient(string address, int port)
    {
        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
    }

    public void Send(byte[] data, int size)
    {
        udpClient_.Send(data, size, endPoint_);
    }

    public void Stop()
    {
        udpClient_.Close();
    }
}

}
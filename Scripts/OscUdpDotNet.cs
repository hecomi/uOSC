using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public class OscUdpDotNet : OscUdp
{
    Queue<byte[]> messageQueue_ = new Queue<byte[]>();
    object lockObject_ = new object();

    UdpClient udpClient_;
    IPEndPoint endPoint_;
    OscThread thread_ = new OscThread();

    public override int messageCount
    {
        get { return messageQueue_.Count; }
    }

    public override void StartServer(int port)
    {
        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);

        thread_.Start(() => 
        {
            while (udpClient_.Available > 0) 
            {
                var buffer = udpClient_.Receive(ref endPoint_);
                lock (lockObject_)
                {
                    messageQueue_.Enqueue(buffer);
                }
            }
        });
    }

    public override void StartClient(string address, int port)
    {
        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
    }

    public override void Stop()
    {
        udpClient_.Close();
        thread_.Stop();
    }

    public override void Send(byte[] data, int size)
    {
        udpClient_.Send(data, size, endPoint_);
    }

    public override byte[] Receive()
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
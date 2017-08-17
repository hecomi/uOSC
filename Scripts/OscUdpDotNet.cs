using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public class OscUdpDotNet : OscUdp
{
    UdpClient udpClient_;
    IPEndPoint endPoint_;

    public override void StartServer(int port)
    {
        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);
    }

    public override void UpdateServer()
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

    public override void StartClient(string address, int port)
    {
        var ip = IPAddress.Parse(address);
        endPoint_ = new IPEndPoint(ip, port);
        udpClient_ = new UdpClient();
    }

    public override void Send(byte[] data, int size)
    {
        udpClient_.Send(data, size, endPoint_);
    }

    public override void Stop()
    {
        udpClient_.Close();
    }
}

}
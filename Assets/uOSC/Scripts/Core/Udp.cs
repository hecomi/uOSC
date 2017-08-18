using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public abstract class Udp
{
    public abstract int messageCount { get; }
    public abstract void StartServer(int port);
    public abstract void StartClient(string address, int port);
    public abstract void Stop();
    public abstract void Send(byte[] data, int size);
    public abstract byte[] Receive();
}

}
#if NETFX_CORE

using UnityEngine;

namespace uOSC
{

public class UdpUwp : Udp
{
    public override int messageCount
    {
        get 
        { 
            // TODO: implement
            return 0; 
        }
    }

    public override void StartServer(int port)
    {
        // TODO: implement
    }

    public override void StartClient(string address, int port)
    {
        // TODO: implement
    }

    public override void Stop()
    {
        // TODO: implement
    }

    public override void Send(byte[] data, int size)
    {
        // TODO: implement
    }

    public override byte[] Receive()
    {
        // TODO: implement
        return null;
    }
}

}

#endif
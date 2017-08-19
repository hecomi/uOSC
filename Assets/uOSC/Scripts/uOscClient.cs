using UnityEngine;
using System.IO;
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

#if NETFX_CORE
    Udp udp_ = new UdpUwp();
#else
    Udp udp_ = new UdpDotNet();
#endif
    Thread thread_ = new Thread();
    Queue<Message> messages_ = new Queue<Message>();
    Queue<Bundle> bundles_ = new Queue<Bundle>();
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
        UpdateSendMessages();
        UpdateSendBundles();
    }

    void UpdateSendMessages()
    {
        while (messages_.Count > 0)
        {
            Message message;
            lock (lockObject_)
            {
                message = messages_.Dequeue();
            }

            using (var stream = new MemoryStream(BufferSize))
            {
                message.Write(stream);
                udp_.Send(stream.GetBuffer(), (int)stream.Position);
            }
        }
    }

    void UpdateSendBundles()
    {
        while (bundles_.Count > 0)
        {
            Bundle bundle;
            lock (lockObject_)
            {
                bundle = bundles_.Dequeue();
            }

            using (var stream = new MemoryStream(BufferSize))
            {
                bundle.Write(stream);
                udp_.Send(stream.GetBuffer(), (int)stream.Position);
            }
        }
    }

    public void Send(string address, params object[] values)
    {
        Send(new Message() 
        {
            address = address,
            packet = values
        });
    }

    public void Send(Message message)
    {
        lock (lockObject_)
        {
            messages_.Enqueue(message);
        }
    }

    public void Send(Bundle bundle)
    {
        lock (lockObject_)
        {
            bundles_.Enqueue(bundle);
        }
    }
}

}
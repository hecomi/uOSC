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
    Udp udp_ = new Uwp.Udp();
    Thread thread_ = new Uwp.Thread();
#else
    Udp udp_ = new DotNet.Udp();
    Thread thread_ = new DotNet.Thread();
#endif
    Queue<object> elements_ = new Queue<object>();
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
        while (elements_.Count > 0)
        {
            object element;
            lock (lockObject_)
            {
                element = elements_.Dequeue();
            }

            using (var stream = new MemoryStream(BufferSize))
            {
                if (element is Message)
                {
                    ((Message)element).Write(stream);
                }
                else if (element is Bundle)
                {
                    ((Bundle)element).Write(stream);
                }
                else
                {
                    return;
                }
                udp_.Send(Util.GetBuffer(stream), (int)stream.Position);
            }
        }
    }

    public void Send(string address, params object[] values)
    {
        Send(new Message() 
        {
            address = address,
            values = values
        });
    }

    public void Send(Message message)
    {
        lock (lockObject_)
        {
            elements_.Enqueue(message);
        }
    }

    public void Send(Bundle bundle)
    {
        lock (lockObject_)
        {
            elements_.Enqueue(bundle);
        }
    }
}

}
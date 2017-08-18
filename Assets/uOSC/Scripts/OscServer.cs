using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace uOSC
{

public class OscServer : MonoBehaviour
{
    [SerializeField]
    int port = 3333;

#if UNITY_UWP
    // TODO: implement
#else
    OscUdp udp_ = new OscUdpDotNet();
#endif
    OscParser parser_ = new OscParser();
    OscThread thread_ = new OscThread();

    public class DataReceiveEvent : UnityEvent<OscMessage> {};
    public DataReceiveEvent onDataReceived { get; private set; }

    void Awake()
    {
        onDataReceived = new DataReceiveEvent();
    }

    void OnEnable()
    {
        udp_.StartServer(port);
        thread_.Start(UpdateMessage);
    }

    void OnDisable()
    {
        thread_.Stop();
        udp_.Stop();
    }

    void Update()
    {
        while (parser_.messageCount > 0)
        {
            var message = parser_.Dequeue();
            onDataReceived.Invoke(message);
        }
    }

    void UpdateMessage()
    {
        while (udp_.messageCount > 0) 
        {
            var buf = udp_.Receive();
            int pos = 0;
            parser_.Parse(buf, ref pos, buf.Length);
        }
    }
}

}
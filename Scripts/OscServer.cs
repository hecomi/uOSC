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

    UdpClient udpClient_;
    IPEndPoint endPoint_;
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
        endPoint_ = new IPEndPoint(IPAddress.Any, port);
        udpClient_ = new UdpClient(endPoint_);
        thread_.Start(UpdateMessage);
    }

    void OnDisable()
    {
        thread_.Stop();
        udpClient_.Close();
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
        while (udpClient_.Available > 0) 
        {
            var buffer = udpClient_.Receive(ref endPoint_);
            parser_.Parse(buffer);
        }
    }
}

}
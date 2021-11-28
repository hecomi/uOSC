using UnityEngine;

namespace uOSC
{

public class uOscServer : MonoBehaviour
{
    [SerializeField]
    public int port = 3333;

    [SerializeField]
    public bool autoStart = true;

#if NETFX_CORE
    Udp udp_ = new Uwp.Udp();
    Thread thread_ = new Uwp.Thread();
#else
    Udp udp_ = new DotNet.Udp();
    Thread thread_ = new DotNet.Thread();
#endif
    Parser parser_ = new Parser();

    public DataReceiveEvent onDataReceived = new DataReceiveEvent();
    public ServerStartEvent onServerStarted = new ServerStartEvent();
    public ServerStopEvent onServerStopped = new ServerStopEvent();

    int port_ = 0;
    bool isStarted_ = false;

    bool isRunning
    {
        get { return udp_.isRunning; }
    }

    void Awake()
    {
        port_ = port;
    }

    void OnEnable()
    {
        if (autoStart) 
        {
            StartServer();
        }
    }

    void OnDisable()
    {
        StopServer();
    }

    public void StartServer()
    {
        if (isStarted_) return;

        udp_.StartServer(port);
        thread_.Start(UpdateMessage);

        isStarted_ = true;

        onServerStarted.Invoke(port);
    }

    public void StopServer()
    {
        if (!isStarted_) return;

        thread_.Stop();
        udp_.Stop();

        isStarted_ = false;

        onServerStopped.Invoke(port);
    }

    void Update()
    {
        UpdateReceive();
        UpdateChangePort();
    }

    void UpdateReceive()
    {
        while (parser_.messageCount > 0)
        {
            var message = parser_.Dequeue();
            onDataReceived.Invoke(message);
        }
    }

    void UpdateChangePort()
    {
        if (port_ == port) return;

        StopServer();
        StartServer();
        port_ = port;
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
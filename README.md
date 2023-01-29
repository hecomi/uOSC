uOSC
====

**uOSC** is an OSC implementation for Unity.

Install
-------

- Unity Package
  - Download the latest .unitypackage from [Release page](https://github.com/hecomi/uOSC/releases).
- Git URL (UPM)
  - Add `https://github.com/hecomi/uOSC.git#upm` to Package Manager.
- Scoped Registry (UPM)
  - Add a scoped registry to your project.
    - URL: `https://registry.npmjs.com`
    - Scope: `com.hecomi`
  - Install uOSC in Package Manager.

How to use
----------

### Server

1. Attach `uOscServer` component to a GameObject.
2. Set the port you want to listen on.
3. Register the `uOscServer.onDataReceived` event (from code or inspector).

```cs
using UnityEngine;
using uOSC;

public class ServerTest : MonoBehaviour
{
    void Start()
    {
        var server = GetComponent<uOscServer>();
        server.onDataReceived.AddListener(OnDataReceived);
    }

    void OnDataReceived(Message message)
    {
        // address (e.g. /uOSC/hoge)
        var msg = message.address + ": ";

        // arguments (object array)
        foreach (var value in message.values)
        {
            if      (value is int)    msg += (int)value;
            else if (value is float)  msg += (float)value;
            else if (value is string) msg += (string)value;
            else if (value is bool)   msg += (bool)value;
            else if (value is byte[]) msg += "byte[" + ((byte[])value).Length + "]";
        }

        Debug.Log(msg);
    }
}
```

`onDataReceived` is called from the main Unity thread, so you can use the Unity APIs in it.

Since the OSC Bundle is automatically expanded and each message comes directly to the `onDataReceived` event handler, there is no need to check whether the value is an OSC Message or an OSC Bundle.

### Client

1. Add `uOscClient` component to GameObject.
2. Send data (`int`, `float`, `string`, `bool`, and `byte[]`, are supported now).

```cs
using UnityEngine;
using uOSC;

public class ClientTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/test", 10, "hoge", 1.234f, new byte[] { 1, 2, 3 });
    }
}
```


UI
--

<img src="https://raw.githubusercontent.com/wiki/hecomi/uOSC/uOSC-UI.png" width="720" />

Tips
----

### Start / Stop

If `autoStart` of `uOscServer` is true, the server will start automatically at runtime. If you want to start it manually, you can set it to false and call `StartServer()`; calling `StopServer()` will stop the server. If you want to start and stop from the inspector, toggle the checkbox of the component.

### Change address and port dynamically

If the `port` or `address` of `uOscClient` / `uOscServer` is changed, the server or client will be restarted automatically. If you want to detect when a server or client starts or stops, register listeners to `uOscServer.onServerStarted` / `uOscServer.onServerStopped` / `uOscClient.onClientStarted` / `uOscClient.onClientStopped` events.

### Queue Size

`uOscClient.Send()` is not executed immediately, but is registered in a queue, and is retrieved from the queue and sent in a background thread. If a large number of `Send()` are called at the same time, messages that exceed `uOscClient.maxQueueSize` will be automatically discarded. In this case, you should change `uOscClient.maxQueueSize` to a larger value. The interval of the background thread processing is about 1 ms by default.

### Data transmission interval

If you need to send multiple large `byte[]` data at the same time, there are cases where you will lose packets frequently if you do not allow an interval. In this case, set `uOscClient.dataTransmissionInterval` (milliseconds). The background thread waits for the interval between each time a message is sent.

### Maximum data size

The maximum packet size that can be sent is determined by the UDP spec, which is 65535 bytes minus the size of the UDP and OSC headers. If you want to divide packets to send larger data, please refer to [uPacketDivision](https://github.com/hecomi/uPacketDivision).

### No message from other devices

Please check your firewall settings.

### Backward compatibility

Various features have been added since v2, but backward compatibility with v1 has been maintained.

Examples
--------

### Send texture

This is an example to send texture data to a remote host.

#### Client

```cs
using UnityEngine;
using uOSC;

public class ClientBlobTest : MonoBehaviour
{
    [SerializeField]
    Texture2D texture;

    byte[] byteTexture_;

    void Start()
    {
#if UNITY_2017
        byteTexture_ = ImageConversion.EncodeToPNG(texture);
#else
        byteTexture_ = texture.EncodeToPNG();
#endif
    }

    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/blob", byteTexture_);
    }
}
```

#### Server

```cs
using UnityEngine;
using uOSC;

namespace uOSC
{

public class ServerBlobTest : MonoBehaviour
{
    Texture2D texture_;

    void Start()
    {
        var server = GetComponent<uOscServer>();
        server.onDataReceived.AddListener(OnDataReceived);

        texture_ = new Texture2D(256, 256, TextureFormat.ARGB32, true);

        var renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture_;
    }

    void OnDataReceived(Message message)
    {
        if (message.address == "/uOSC/blob")
        {
            var byteTexture = (byte[])message.values[0];
#if UNITY_2017
            ImageConversion.LoadImage(texture_, byteTexture, true);
#else
            texture_.LoadImage(byteTexture);
#endif
        }
    }
}
```


### Timestamp

OSC Bundle has timestamp, and you can use it as `DateTime` as below:

```cs
using UnityEngine;
using uOSC;

public class ServerTest : MonoBehaviour
{
    ...

    void OnDataReceived(Message message)
    {
        var dateTime = message.timestamp.ToLocalTime();
    }
}
```

### Send bundle

You can merge multiple OSC messages into a bundle as below:

```cs
using UnityEngine;
using uOSC;

public class ClientBundleTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<uOscClient>();

        // Bundle (root)
        //   - Bundle 1 -> Now
        //     - Message 1-1
        //     - Message 1-2
        //     - Message 1-3
        //   - Bundle 2 -> 10 sec after
        //     - Message 2-1
        //     - Message 2-2
        //     - Message 2-3
        //   - Message 3 -> Immediate

        var bundle1 = new Bundle(Timestamp.Now);
        bundle1.Add(new Message("/uOSC/root/bundle1/message1", 123, "hoge", new byte[] { 1, 2, 3, 4 }));
        bundle1.Add(new Message("/uOSC/root/bundle1/message2", 1.2345f));
        bundle1.Add(new Message("/uOSC/root/bundle1/message3", "abcdefghijklmn"));

        var date2 = System.DateTime.UtcNow.AddSeconds(10);
        var timestamp2 = Timestamp.CreateFromDateTime(date2);
        var bundle2 = new Bundle(timestamp2);
        bundle2.Add(new Message("/uOSC/root/bundle2/message1", 234, "fuga", new byte[] { 2, 3, 4 }));
        bundle2.Add(new Message("/uOSC/root/bundle2/message2", 2.3456f));
        bundle2.Add(new Message("/uOSC/root/bundle2/message3", "opqrstuvwxyz"));

        var root = new Bundle(Timestamp.Immediate);
        root.Add(bundle1);
        root.Add(bundle2);
        root.Add(new Message("/uOSC/root/message3"));

        client.Send(root);

        // Then, client will get 7 messages in onDataReceived event.
    }
}
```

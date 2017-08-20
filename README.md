uOSC
====

**uOSC** is an OSC implementation for Unity.

Platforms
---------

- Windows
- Mac
- HoloLens

I've not checked Android and iOS yet...

Install
-------

Download the latest `.unitypackage` from [Release page](https://github.com/hecomi/uOSC/releases)
and import it to your Unity project.

How to use
----------

### Server

1. Attach `uOscServer` component to GameObject.
2. Register `uOscServer.onDataReceived` event.

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
            else if (value is byte[]) msg += "byte[" + ((byte[])value).Length + "]";
        }

        Debug.Log(msg);
    }
}
```

`onDataReceived` is called from the Unity main thread, so you can use Unity APIs inside it.

OSC Bundle is automatically expanded, and each message comes to
the `onDataReceived` event handler directly, so you don't have to check
whether the value is a OSC Message or OSC Bundle.

### Client

1. Add `uOscClient` component to GameObject.
2. Send data (`int`, `float`, `string`, and `byte[]`, are supported now).

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

Tips
----

### Start / Stop manually

If you want to start and stop uOSC in runtime, please toggle the `enable` flag of `uOscServer` or `uOscClient`.

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

### Increase buffer size

If you want to increaset the size of buffer, please edit the `uOscClient.BufferSize` directly (default is 8192).


License
-------
The MIT License (MIT)

Copyright (c) 2017 hecomi

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

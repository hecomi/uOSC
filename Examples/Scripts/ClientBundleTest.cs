using UnityEngine;

namespace uOSC
{

[RequireComponent(typeof(uOscClient))]
public class ClientBundleTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<uOscClient>();

        var bundle1 = new Bundle(Timestamp.Immediate);
        bundle1.Add(new Message("/uOSC/root/bundle1/message1", 123, "hoge"));
        bundle1.Add(new Message("/uOSC/root/bundle1/message2", 1.2345f));
        bundle1.Add(new Message("/uOSC/root/bundle1/message3", "abcdefghijklmn"));
        bundle1.Add(new Message("/uOSC/root/bundle1/message4", new byte[] { 1, 2, 3, 4 }));

        var bundle2 = new Bundle(Timestamp.Immediate);
        bundle2.Add(new Message("/uOSC/root/bundle2/message1", 234, "fuga"));
        bundle2.Add(new Message("/uOSC/root/bundle2/message2", 2.3456f));
        bundle2.Add(new Message("/uOSC/root/bundle2/message3", "opqrstuvwxyz"));

        var root = new Bundle(Timestamp.Immediate);
        root.Add(bundle1);
        root.Add(bundle2);
        root.Add(new Message("/uOSC/root/message2"));

        client.Send(root);
    }
}

}
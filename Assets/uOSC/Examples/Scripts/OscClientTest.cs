using UnityEngine;

namespace uOSC
{

[RequireComponent(typeof(OscClient))]
public class OscClientTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<OscClient>();
        client.Send("/uOSC/test", 10, "hoge", "hogehoge", 1.234f, 123);
    }
}

}
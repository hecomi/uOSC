using UnityEngine;

namespace uOSC
{

public class OscClientTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<OscClient>();
        client.Send("/message", 10, "hoge", "hogehoge", 1.234f, 123);
    }
}

}
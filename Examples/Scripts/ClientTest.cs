using UnityEngine;

namespace uOSC
{

[RequireComponent(typeof(uOscClient))]
public class ClientTest : MonoBehaviour
{
    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/test", 10, "hoge", "hogehoge", 1.234f, 123f);
    }
}

}
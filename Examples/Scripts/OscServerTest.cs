using UnityEngine;

namespace uOSC
{

[RequireComponent(typeof(OscServer))]
public class OscServerTest : MonoBehaviour
{
    void Start()
    {
        var server = GetComponent<OscServer>();
        server.onDataReceived.AddListener(OnDataReceived);
    }

    void OnDataReceived(OscMessage message)
    {
        if (message.address == "/uOSC/test")
        {
            var val1 = message.values[0].AsInt();
            var val2 = message.values[1].AsString();
            var val3 = message.values[2].AsString();
            var val4 = message.values[3].AsFloat();
            var val5 = message.values[4].AsFloat();
            Debug.LogFormat("{0}: {1} {2} {3} {4} {5}", message.address, val1, val2, val3, val4, val5);
        }
        else
        {
            var msg = message.address + ": ";
            msg += "(" + message.timestamp.ToLocalTime() + ") ";
            foreach (var value in message.values)
            {
                msg += value.GetString() + " ";
            }
            Debug.Log(msg);
        }
    }
}

}
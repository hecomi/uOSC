using UnityEngine;

namespace uOSC
{

[RequireComponent(typeof(uOscClient))]
public class OscClientBlobTest : MonoBehaviour
{
    [SerializeField]
    Texture2D texture;

    byte[] byteTexture_;

    void Start()
    {
        byteTexture_ = ImageConversion.EncodeToPNG(texture);
    }

    void Update()
    {
        var client = GetComponent<uOscClient>();
        client.Send("/uOSC/blob", byteTexture_);
    }
}

}
using System.IO;
using System.Collections.Generic;

namespace uOSC
{

public class Bundle
{
    public Timestamp timestamp;
    private List<object> elements_ = new List<object>();

    public void AddMessage(Message message)
    {
        elements_.Add(message);
    }

    public void AddBundle(Bundle bundle)
    {
        elements_.Add(bundle);
    }

    public void Write(MemoryStream stream)
    {
    }
}

}
using System.IO;

namespace uOSC
{

public struct Message
{
    public string address;
    public Timestamp timestamp;
    public object[] packet;

    public static Message none
    {
        get 
        { 
            return new Message() 
            { 
                address = "", 
                timestamp = new Timestamp(),
                packet = null
            };
        }
    }

    public Message(string address, params object[] packet)
    {
        this.address = address;
        this.timestamp = new Timestamp();
        this.packet = packet;
    }

    public void Write(MemoryStream stream)
    {
        WriteAddress(stream);
        WriteTypes(stream);
        WriteValues(stream);
    }

    void WriteAddress(MemoryStream stream)
    {
        Writer.Write(stream, address);
    }

    void WriteTypes(MemoryStream stream)
    {
        string types = ",";
        for (int i = 0; i < packet.Length; ++i)
        {
            var value = packet[i];
            if      (value is int)    types += Identifier.Int;
            else if (value is float)  types += Identifier.Float;
            else if (value is string) types += Identifier.String;
            else if (value is byte[]) types += Identifier.Blob;
        }
        Writer.Write(stream, types);
    }

    void WriteValues(MemoryStream stream)
    {
        for (int i = 0; i < packet.Length; ++i)
        {
            var value = packet[i];
            if      (value is int)    Writer.Write(stream, value.AsInt());
            else if (value is float)  Writer.Write(stream, value.AsFloat());
            else if (value is string) Writer.Write(stream, value.AsString());
            else if (value is byte[]) Writer.Write(stream, value.AsBlob());
        }
    }
}

}
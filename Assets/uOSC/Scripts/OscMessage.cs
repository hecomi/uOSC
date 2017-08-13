namespace uOSC
{

public struct OscMessage
{
    public string address;
    public object[] values;

    public static OscMessage none
    {
        get { return new OscMessage() { address = "", values = null }; }
    }
}

}
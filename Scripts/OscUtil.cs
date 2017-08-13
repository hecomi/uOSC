using UnityEngine;
using System;

namespace uOSC
{

public static class OscUtil
{
    public static int ConvertOffsetToMultipleOfFour(int pos)
    {
        return (pos + 4) & ~0x3;
    }

    static bool CheckType(object value, System.Type type)
    {
        if (value.GetType() != type)
        {
            Debug.LogErrorFormat("\"{0}\" cannot be casted into \"{1}\".", value.GetType(), type);
            return false;
        }
        return true;
    }

    public static int AsInt(this object value)
    {
        return CheckType(value, typeof(int)) ? (int)value : 0;
    }

    public static float AsFloat(this object value)
    {
        return CheckType(value, typeof(float)) ? (float)value : 0f;
    }

    public static string AsString(this object value)
    {
        return CheckType(value, typeof(string)) ? (string)value : "";
    }

    public static byte[] AsBlob(this object value)
    {
        return CheckType(value, typeof(byte[])) ? (byte[])value : null;
    }
}

}
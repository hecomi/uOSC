using UnityEngine;
using System;

namespace uOSC
{

public static class Identifier
{
    public const string bundle = "#bundle";
}

public static class Util
{
    public static readonly byte zero = Convert.ToByte('\0');
    public static readonly byte[] zeros = { zero, zero, zero, zero };

    public static bool IsMultipleOfFour(int num)
    {
        return num == (num & ~0x3);
    }

    public static int GetStringOffset(int size)
    {
        return (size + 4) & ~0x3;
    }

    public static int GetBufferOffset(int size)
    {
        var offset = size & ~0x3;
        return (offset == size) ? size : (offset + 4);
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

    public static string GetString(this object value)
    {
        if (value is int)    return value.AsInt().ToString();
        if (value is float)  return value.AsFloat().ToString();
        if (value is string) return value.AsString();
        if (value is byte[]) return "Byte[" + value.AsBlob().Length + "]";

        return value.ToString();
    }
}

}
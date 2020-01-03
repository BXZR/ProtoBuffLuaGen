using System;
using System.Collections.Generic;

public static class TypeExtentions
{

    static TypeExtentions()
    {
        TYPE_TO_STRING.Add(typeof(sbyte), "sbyte");     //sbyte
        TYPE_TO_STRING.Add(typeof(byte), "byte");       //byte
        TYPE_TO_STRING.Add(typeof(char), "char");       //char 
        TYPE_TO_STRING.Add(typeof(int), "int");         //int     Int32
        TYPE_TO_STRING.Add(typeof(uint), "uint");       //uint    UInt32
        TYPE_TO_STRING.Add(typeof(Int16), "Int16");     //short   Int16
        TYPE_TO_STRING.Add(typeof(Int64), "Int64");     //long    Int64
        TYPE_TO_STRING.Add(typeof(UInt16), "UInt16");   //ushort  UInt16
        TYPE_TO_STRING.Add(typeof(UInt64), "UInt64");   //ulong   UInt64
        TYPE_TO_STRING.Add(typeof(float), "float");     //float
        TYPE_TO_STRING.Add(typeof(double), "double");   //double
        TYPE_TO_STRING.Add(typeof(decimal), "decimal"); //decimal
        TYPE_TO_STRING.Add(typeof(string), "string");   //string 

        TYPE_TO_STRING.Add(typeof(sbyte[]), "sbyte[]");
        TYPE_TO_STRING.Add(typeof(byte[]), "byte[]");
        TYPE_TO_STRING.Add(typeof(char[]), "char[]");
        TYPE_TO_STRING.Add(typeof(int[]), "int[]");
        TYPE_TO_STRING.Add(typeof(uint[]), "uint[]");
        TYPE_TO_STRING.Add(typeof(Int16[]), "Int16[]");
        TYPE_TO_STRING.Add(typeof(Int64[]), "Int64[]");
        TYPE_TO_STRING.Add(typeof(UInt16[]), "UInt16[]");
        TYPE_TO_STRING.Add(typeof(UInt64[]), "UInt64[]");
        TYPE_TO_STRING.Add(typeof(string[]), "string[]");
        TYPE_TO_STRING.Add(typeof(float[]), "float[]");
        TYPE_TO_STRING.Add(typeof(double[]), "double[]");
        TYPE_TO_STRING.Add(typeof(decimal[]), "decimal[]");

        TYPE_TO_STRING.Add(typeof(List<sbyte>), "List<sbyte>");
        TYPE_TO_STRING.Add(typeof(List<byte>), "List<byte>");
        TYPE_TO_STRING.Add(typeof(List<char>), "List<char>");
        TYPE_TO_STRING.Add(typeof(List<int>), "List<int>");
        TYPE_TO_STRING.Add(typeof(List<uint>), "List<uint>");
        TYPE_TO_STRING.Add(typeof(List<Int16>), "List<Int16>");
        TYPE_TO_STRING.Add(typeof(List<Int64>), "List<Int64>");
        TYPE_TO_STRING.Add(typeof(List<UInt16>), "List<UInt16>");
        TYPE_TO_STRING.Add(typeof(List<UInt64>), "List<UInt64>");
        TYPE_TO_STRING.Add(typeof(List<string>), "List<string>");
        TYPE_TO_STRING.Add(typeof(List<float>), "List<float>");
        TYPE_TO_STRING.Add(typeof(List<double>), "List<double>");
        TYPE_TO_STRING.Add(typeof(List<decimal>), "List<decimal>");
    }

    public static bool Is(Type type, Type typeCompare)
    {
        return type.Equals(typeCompare);
    }

    public static bool IsInt16(this Type type)
    {
        return Is(type, typeof(Int16));
    }

    public static bool IsUInt16(this Type type)
    {
        return Is(type, typeof(UInt16));
    }

    public static bool IsInt32(this Type type)
    {
        return Is(type, typeof(Int32));
    }

    public static bool IsUInt32(this Type type)
    {
        return Is(type, typeof(UInt32));
    }

    public static bool IsInt64(this Type type)
    {
        return Is(type, typeof(Int64));
    }

    public static bool IsUInt64(this Type type)
    {
        return Is(type, typeof(UInt64));
    }

    public static bool IsByte(this Type type)
    {
        return Is(type, typeof(byte));
    }

    public static bool IsSByte(this Type type)
    {
        return Is(type, typeof(sbyte));
    }

    public static bool IsChar(this Type type)
    {
        return Is(type, typeof(char));
    }

    public static bool IsString(this Type type)
    {
        return Is(type, typeof(string));
    }

    public static bool IsFloat(this Type type)
    {
        return Is(type, typeof(float));
    }

    public static bool IsDouble(this Type type)
    {
        return Is(type, typeof(double));
    }

    public static bool IsDecimal(this Type type)
    {
        return Is(type, typeof(decimal));
    }

    public static bool IsBoolean(this Type type)
    {
        return Is(type, typeof(bool));
    }

    public static bool IsDateTime(this Type type)
    {
        return Is(type, typeof(DateTime));
    }

    #region Array
    public static bool IsInt16Array(this Type type)
    {
        return Is(type, typeof(Int16[]));
    }

    public static bool IsUInt16Array(this Type type)
    {
        return Is(type, typeof(UInt16[]));
    }

    public static bool IsInt32Array(this Type type)
    {
        return Is(type, typeof(Int32[]));
    }

    public static bool IsUInt32Array(this Type type)
    {
        return Is(type, typeof(UInt32[]));
    }

    public static bool IsInt64Array(this Type type)
    {
        return Is(type, typeof(Int64[]));
    }

    public static bool IsUInt64Array(this Type type)
    {
        return Is(type, typeof(UInt64[]));
    }

    public static bool IsByteArray(this Type type)
    {
        return Is(type, typeof(byte[]));
    }

    public static bool IsSByteArray(this Type type)
    {
        return Is(type, typeof(sbyte[]));
    }

    public static bool IsCharArray(this Type type)
    {
        return Is(type, typeof(char[]));
    }

    public static bool IsStringArray(this Type type)
    {
        return Is(type, typeof(string[]));
    }

    public static bool IsFloatArray(this Type type)
    {
        return Is(type, typeof(float[]));
    }

    public static bool IsDoubleArray(this Type type)
    {
        return Is(type, typeof(double[]));
    }

    public static bool IsDecimalArray(this Type type)
    {
        return Is(type, typeof(decimal[]));
    }

    public static bool IsBooleanArray(this Type type)
    {
        return Is(type, typeof(bool[]));
    }
    #endregion

    #region List
    public static bool IsInt16List(this Type type)
    {
        return Is(type, typeof(List<Int16>));
    }

    public static bool IsUInt16List(this Type type)
    {
        return Is(type, typeof(List<UInt16>));
    }

    public static bool IsInt32List(this Type type)
    {
        return Is(type, typeof(List<Int32>));
    }

    public static bool IsUInt32List(this Type type)
    {
        return Is(type, typeof(List<UInt32>));
    }

    public static bool IsInt64List(this Type type)
    {
        return Is(type, typeof(List<Int64>));
    }

    public static bool IsUInt64List(this Type type)
    {
        return Is(type, typeof(List<UInt64>));
    }

    public static bool IsByteList(this Type type)
    {
        return Is(type, typeof(List<byte>));
    }

    public static bool IsSByteList(this Type type)
    {
        return Is(type, typeof(List<sbyte>));
    }

    public static bool IsCharList(this Type type)
    {
        return Is(type, typeof(List<char>));
    }

    public static bool IsStringList(this Type type)
    {
        return Is(type, typeof(List<string>));
    }

    public static bool IsFloatList(this Type type)
    {
        return Is(type, typeof(List<float>));
    }

    public static bool IsDoubleList(this Type type)
    {
        return Is(type, typeof(List<double>));
    }

    public static bool IsDecimalList(this Type type)
    {
        return Is(type, typeof(List<decimal>));
    }

    public static bool IsBooleanList(this Type type)
    {
        return Is(type, typeof(List<bool>));
    }
    #endregion

    public static bool IsGenericList(this Type type)
    {
        var genericList = typeof(List<>);
        return type.Name.StartsWith(genericList.Name);
    }

    #region Type To String
    private static Dictionary<Type, string> TYPE_TO_STRING = new Dictionary<Type, string>();

    public static string TypeToString(Type type)
    {
        var str = string.Empty;
        TYPE_TO_STRING.TryGetValue(type, out str);
        return str;
    }
    #endregion
}


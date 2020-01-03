
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Google.Protobuf.Reflection;


public class ProtoClassToLuaTableConverter3 : BaseProtoConverter
{
    const string DEFAULT_EXTRA_NAME = "_Ex";
    const string DEFAULT_EXT = "cs";
    const string METHOD_NAME = "PushToLua";
    const string METHOD_POP_NAME = "PopFromLua";
    const string LUA_API = "LuaAPI";

    protected DescriptorProto m_protoDesc;
    protected string m_ns;

    public ProtoClassToLuaTableConverter3(DescriptorProto protoDesc, string ns, string saveDir, string extraName = DEFAULT_EXTRA_NAME, string ext = DEFAULT_EXT) : base(typeof(DescriptorProto), saveDir, extraName, ext)
    {
        m_protoDesc = protoDesc;
        m_ns = ns;
    }

    protected override Type[] GetAttributes()
    {
        return new Type[] { typeof(ProtoMemberAttribute) };
    }

    protected override string TypeToClassName()
    {
        string full = string.IsNullOrEmpty(m_ns) ? m_protoDesc.Name : m_ns + "." + m_protoDesc.Name;
        full = full.Replace(".", "_");
        return full;
    }

    protected override string RealClassName()
    {
        return string.IsNullOrEmpty(m_ns) ? "Magic.Cougar." + m_protoDesc.Name : m_ns + "." + m_protoDesc.Name;
    }

    protected override void MakeDescription(ref StringBuilder sb)
    {
        sb.Append("/**");
        sb.Append("\r\n*");
        sb.Append(" Proto Class = ");
        sb.Append(m_protoDesc.Name);
        sb.Append("\r\n*/");

        m_headerUsings.Add("System");
        m_headerUsings.Add("System.Collections");
        m_headerUsings.Add("System.Collections.Generic");
        m_headerUsings.Add("XLua");
        m_headerUsings.Add(LUA_API + " = XLua.LuaDLL.Lua");
    }

    protected override void MakeHeader(ref StringBuilder sb)
    {
        sb.Append("\r\n");
        AppendUsing(ref sb);
        sb.Append("\r\n");
        sb.Append("namespace ");
        sb.Append("Magic.GameLogic");
        sb.Append("\r\n{\r\n");
        sb.Append("\tpublic static class ");
        sb.Append(TypeToClassName());
        sb.Append(m_extraName);
        sb.Append("\r\n\t{\r\n");
    }

    private void AppendUsing(ref StringBuilder sb)
    {
        if (m_headerUsings != null && m_headerUsings.Count > 0)
        {
            foreach (var item in m_headerUsings)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    sb.Append("using ");
                    sb.Append(item);
                    sb.Append(";\r\n");
                }
            }
        }
    }

    protected override void MakeFooter(ref StringBuilder sb)
    {
        sb.Append("\r\n\t}\r\n");
        sb.Append("}");
    }

    protected override void MakeMiddle(ref StringBuilder sb)
    {
        sb.Append("\r\n");
        sb.Append("\t\tpublic static void ");
        sb.Append(METHOD_NAME);
        sb.Append("(this ");
        sb.Append(RealClassName());
        sb.Append(" msg, ");
        sb.Append("IntPtr L)");
        sb.Append("\r\n\t\t{\r\n");
        sb.Append("\t\t\t");
        sb.Append(LUA_API);
        sb.Append(".lua_newtable(L);");
        sb.Append("\r\n");

        if (m_protoDesc.Fields.Count > 0)
        {
            foreach (var item in m_protoDesc.Fields)
            {
                ItemToLuaTableKVP(ref sb, item);
            }
        }

        sb.Append("\t\t}");
    }

    private Type ProtoTypeCSharType(FieldDescriptorProto fieldDesc)
    {
        if (fieldDesc.label == FieldDescriptorProto.Label.LabelRequired ||
            fieldDesc.label == FieldDescriptorProto.Label.LabelOptional)
        {
            switch (fieldDesc.type)
            {
                case FieldDescriptorProto.Type.TypeBool:
                    return typeof(Boolean);
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeFixed32:
                    return typeof(Int32);
                case FieldDescriptorProto.Type.TypeEnum:
                    return typeof(Enum);
                case FieldDescriptorProto.Type.TypeFloat:
                    return typeof(Single);
                case FieldDescriptorProto.Type.TypeUint32:
                    return typeof(UInt32);
                case FieldDescriptorProto.Type.TypeDouble:
                    return typeof(Double);
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint64:
                    return typeof(Int64);
                case FieldDescriptorProto.Type.TypeUint64:
                    return typeof(UInt64);
                case FieldDescriptorProto.Type.TypeGroup:
                    return typeof(List<>);
                case FieldDescriptorProto.Type.TypeString:
                    return typeof(String);
                case FieldDescriptorProto.Type.TypeBytes:
                    return typeof(byte[]);
                default:
                    return typeof(Object);
            }
        }
        else
        {
            switch (fieldDesc.type)
            {
                case FieldDescriptorProto.Type.TypeBool:
                    return typeof(Boolean[]);
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeFixed32:
                    return typeof(Int32[]);
                case FieldDescriptorProto.Type.TypeEnum:
                    return typeof(Enum[]);
                case FieldDescriptorProto.Type.TypeFloat:
                    return typeof(Single[]);
                case FieldDescriptorProto.Type.TypeUint32:
                    return typeof(UInt32[]);
                case FieldDescriptorProto.Type.TypeDouble:
                    return typeof(Double[]);
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint64:
                    return typeof(Int64[]);
                case FieldDescriptorProto.Type.TypeUint64:
                    return typeof(UInt64[]);
                case FieldDescriptorProto.Type.TypeGroup:
                case FieldDescriptorProto.Type.TypeMessage:
                    return typeof(List<>);
                case FieldDescriptorProto.Type.TypeString:
                    return typeof(List<String>);
                case FieldDescriptorProto.Type.TypeBytes:
                    return typeof(byte[]);
                default:
                    return typeof(Object);
            }
        }
        
    }
    private void ItemToLuaTableKVP(ref StringBuilder sb, FieldDescriptorProto infoAttr)
    {
        var itemType = ProtoTypeCSharType(infoAttr);
        var itemName = infoAttr.Name;

        sb.Append("\t\t\t");
        sb.Append(LUA_API);
        sb.Append(".lua_pushstring(L, \"");
        sb.Append(itemName);
        sb.Append("\");\r\n");

        //TypeToPushCode(ref sb, itemType, itemName);
        sb.Append("\t\t\t");
        var code = PushByType(itemType, "msg." + itemName, 
            infoAttr.type == FieldDescriptorProto.Type.TypeGroup || infoAttr.label == FieldDescriptorProto.Label.LabelRepeated);
        if (string.IsNullOrEmpty(code))
        {
            sb.Append(LUA_API);
            sb.Append(".lua_pushnil(L);\r\n");
        }
        else
        {
            sb.Append(code);
        }
        sb.Append("\t\t\t");
        sb.Append(LUA_API);
        sb.Append(".xlua_psettable(L, -3);\r\n");
    }

    private string PushByType(Type itemType, string valueExpress, bool fiveTab = false)
    {
        if (itemType.IsEnum || itemType.Name == "Enum")
        {
            return LUA_API + ".xlua_pushinteger(L, (int)" + valueExpress + ");\r\n";
        }
        else
        {
            if (itemType.IsUInt32() || itemType.IsUInt16())                              //LuaAPI.xlua_pushuint(L, value);
            {
                return LUA_API + ".xlua_pushuint(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsInt32() || itemType.IsInt16())                           //LuaAPI.xlua_pushinteger(L, value);
            {
                return LUA_API + ".xlua_pushinteger(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsInt64())                                                 //LuaAPI.lua_pushint64(L, value);
            {
                return LUA_API + ".lua_pushint64(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsUInt64())                                                 //LuaAPI.lua_pushuint64(L, value);
            {
                return LUA_API + ".lua_pushuint64(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsBoolean())                                               //LuaAPI.lua_pushboolean(L, value);
            {
                return LUA_API + ".lua_pushboolean(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsString())                                                //LuaAPI.lua_pushstring(L, value);
            {
                return LUA_API + ".lua_pushstring(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsFloat() || itemType.IsDouble() || itemType.IsDecimal())  //LuaAPI.lua_pushnumber(L, value);
            {
                return LUA_API + ".lua_pushnumber(L, " + valueExpress + ");\r\n";
            }
            else if (itemType.IsUInt32Array() || itemType.IsUInt16Array())
            {
                return ArrayCode(fiveTab, valueExpress, ".xlua_pushuint");
            }
            else if (itemType.IsInt32Array() || itemType.IsInt16Array())
            {
                return ArrayCode(fiveTab, valueExpress, ".xlua_pushinteger");
            }
            else if (itemType.IsStringArray())
            {
                return ArrayCode(fiveTab, valueExpress, ".lua_pushstring");
            }
            else if (itemType.IsInt64Array())
            {
                return ArrayCode(fiveTab, valueExpress, ".lua_pushint64");
            }
            else if (itemType.IsUInt64Array())
            {
                return ArrayCode(fiveTab, valueExpress, ".lua_pushuint64");
            }
            else if (itemType.IsBooleanArray())
            {
                return ArrayCode(fiveTab, valueExpress, ".lua_pushboolean");
            }
            else if (itemType.IsFloatArray() || itemType.IsDoubleArray())
            {
                return ArrayCode(fiveTab, valueExpress, ".lua_pushnumber");
            }
            else if (itemType.IsArray)
            {
                TipError("类型 {0}, type name = {1}, 是数组。", itemType.FullName, itemType.Name);
                return string.Empty;
            }
            else if (itemType.IsGenericList())
            {
                var genericArguments = itemType.GetGenericArguments();
                if (genericArguments != null && genericArguments.Length == 1)
                {
                    var arg = genericArguments[0];
                    var code = PushByType(arg, "item", true);
                    code = string.Format(CodeTemplates.TPL_LINE_PUSH_KVP_VALUE_IS_GENERIC_LIST, valueExpress, LUA_API, code);
                    code = code.Replace("[[", "{");
                    code = code.Replace("]]", "}");
                    return code;
                }
                else
                {
                    TipError("{0}, List泛型超过参数超过一个 .. 暂不支持转换。", m_type.FullName);
                    return string.Empty;
                }
                //TipError("{0}, Generic List .. 暂不支持转换。", m_type.FullName);
                //return string.Empty;
            }
            else if (itemType.IsClass)
            {
                //if (Has<ProtoContractAttribute>(itemType))
                //{
                var code = string.Format(fiveTab ? CodeTemplates.TPL_LINE_PUSH_KVP_VALUE_IS_CLASS_2 : CodeTemplates.TPL_LINE_PUSH_KVP_VALUE_IS_CLASS, valueExpress, METHOD_NAME, LUA_API);
                code = code.Replace("[[", "{");
                code = code.Replace("]]", "}");
                return code;
                //}
                //else
                //{
                //    var attrs = itemType.GetCustomAttributes(typeof(ProtoContractAttribute), true);
                //    if (attrs == null)
                //    {
                //        TipError("{0}, 不含 ProtoContractAttribute length = 0 .. 不支持转换。", m_type.FullName);
                //    } else
                //    {
                //        TipError("{0}, 不含 ProtoContractAttribute length = {1} .. 不支持转换。", m_type.FullName, attrs.Length);
                //    }
                //    TipError("{0}, 不含 ProtoContractAttribute .. 不支持转换。", m_type.FullName);
                //    return string.Empty;
                //}
            }
            else
            {
                TipError("类型 {0},  不支持转换。", m_protoDesc.Name);
                return string.Empty;
            }
        }
    }

    private string ArrayCode(bool fiveTab, string valueExpress, string pushMethod)
    {
        var code = string.Format(fiveTab ? CodeTemplates.TPL_LINE_ARRAY : CodeTemplates.TPL_LINE_ARRAY_2, valueExpress, LUA_API, pushMethod);
        code = code.Replace("[[", "{");
        code = code.Replace("]]", "}");
        return code;
    }
    

    /// <summary>
    /// 合并成文件的名称
    /// </summary>
    /// <returns></returns>
    protected override string MergePath()
    {
        var fullName = RealClassName();
        fullName = fullName.Replace(".", "_");  //(避免重复) 例如 : Magic.Cougar.Xxx+xxx  替换成 Magic_Cougar_Xxx_xxx
        fullName = fullName.Replace("+", "_");
        string fileName = string.Format("{0}{1}.{2}", fullName, m_extraName, m_ext);
        return Path.Combine(m_saveDir, fileName);
    }
}


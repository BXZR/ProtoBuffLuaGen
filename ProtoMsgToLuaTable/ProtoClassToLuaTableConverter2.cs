
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

public class ProtoClassToLuaTableConverter2 : BaseProtoConverter
{
    const string DEFAULT_EXTRA_NAME = "_Ex";
    const string DEFAULT_EXT = "cs";
    const string METHOD_NAME = "PushToLua";
    const string METHOD_POP_NAME = "PopFromLua";
    const string LUA_API = "LuaAPI";

    public ProtoClassToLuaTableConverter2(Type type, string saveDir, string extraName = DEFAULT_EXTRA_NAME, string ext = DEFAULT_EXT) : base(type, saveDir, extraName, ext)
    {
    }

    protected override Type[] GetAttributes()
    {
        return new Type[] { typeof(ProtoMemberAttribute) };
    }

    protected override void MakeDescription(ref StringBuilder sb)
    {
        sb.Append("/**");
        sb.Append("\r\n*");
        sb.Append(" Proto Class = ");
        sb.Append(m_type.FullName);
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
        sb.Append(m_type.Namespace);
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

        if (m_infoAndAttributes.Count > 0)
        {
            foreach (var item in m_infoAndAttributes)
            {
                ItemToLuaTableKVP(ref sb, item);
            }
        }

        sb.Append("\t\t}");

        if (m_type.Namespace.StartsWith("Lpb") && !m_type.Name.EndsWith("Ack"))
        {
            sb.Append("\r\n");
            LuaTableToInstace(ref sb);
        }
    }


    private void ItemToLuaTableKVP(ref StringBuilder sb, BaseInfoAndAttribute infoAttr)
    {
        var itemType = infoAttr.InfoType;
        var itemName = infoAttr.InfoName;

        sb.Append("\t\t\t");
        sb.Append(LUA_API);
        sb.Append(".lua_pushstring(L, \"");
        sb.Append(itemName);
        sb.Append("\");\r\n");

        //TypeToPushCode(ref sb, itemType, itemName);
        sb.Append("\t\t\t");
        var code = PushByType(itemType, "msg." + itemName);
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
        if (itemType.IsEnum)
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
                TipError("类型 {0},  不支持转换。", m_type.FullName);
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


    private void ItemToLuaTableKVP2(ref StringBuilder sb, BaseInfoAndAttribute infoAttr)
    {
        var itemType = infoAttr.InfoType;
        var itemName = infoAttr.InfoName;
        sb.Append("\t\t\t");

        var code = PushByType2(itemType, itemName, "msg." + itemName);
        sb.Append(code);

        sb.Append("\t\t\t");
    }

    private string PushByType2(Type itemType, string itemName, string valueExpress, bool fiveTab = false)
    {
        if (itemType.IsEnum)
        {
            var code = "var val" + itemName + " = table.Get<int>(\"" + itemName + "\");\r\n";
            code = "\t\t\t" + code + valueExpress + " = (" + TypeToClassName(itemType) + ")val" + itemName + ";\r\n";
            return code;
        }
        else
        {
            if (itemType.IsUInt32())
            {
                return valueExpress + " = table.Get<uint>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsUInt16())
            {
                return valueExpress + " = table.Get<UInt16>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsInt32())
            {
                return valueExpress + " = table.Get<int>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsInt16())
            {
                return valueExpress + " = table.Get<Int16>(\"" + itemName + "\");\r\n";
            }
            //if (itemType.IsUInt32() ||)                              //LuaAPI.xlua_pushuint(L, value);
            //{
            //    return LUA_API + ".xlua_pushuint(L, " + valueExpress + ");\r\n";
            //}
            //else if ( || )                           //LuaAPI.xlua_pushinteger(L, value);
            //{
            //    return LUA_API + ".xlua_pushinteger(L, " + valueExpress + ");\r\n";
            //}
            else if (itemType.IsInt64())                                                 //LuaAPI.lua_pushint64(L, value);
            {
                return valueExpress + " = table.Get<Int64>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsUInt64())                                                 //LuaAPI.lua_pushuint64(L, value);
            {
                return valueExpress + " = table.Get<UInt64>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsBoolean())                                               //LuaAPI.lua_pushboolean(L, value);
            {
                return valueExpress + " = table.Get<bool>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsString())                                                //LuaAPI.lua_pushstring(L, value);
            {
                return valueExpress + " = table.Get<string>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsFloat())
            {
                return valueExpress + " = table.Get<float>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsDouble())
            {
                return valueExpress + " = table.Get<double>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsDecimal())
            {
                return valueExpress + " = table.Get<decimal>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsUInt32Array() || itemType.IsUInt16Array())
            {
                return valueExpress + " = table.Get<uint[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsInt32Array() || itemType.IsInt16Array())
            {
                return valueExpress + " = table.Get<int[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsStringArray())
            {
                return valueExpress + " = table.Get<string[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsInt64Array())
            {
                return valueExpress + " = table.Get<long[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsUInt64Array())
            {
                return valueExpress + " = table.Get<ulong[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsBooleanArray())
            {
                return valueExpress + " = table.Get<bool[]>(\"" + itemName + "\");\r\n";
            }
            else if (itemType.IsArray)
            {
                TipError("类型 {0}, type name = {1}, 是数组。", itemType.FullName, itemType.Name);
                return string.Empty;
            }
            //else if (itemType.IsFloat() || itemType.IsDouble() || itemType.IsDecimal())  //LuaAPI.lua_pushnumber(L, value);
            //{
            //    return LUA_API + ".lua_pushnumber(L, " + valueExpress + ");\r\n";
            //}
            else if (itemType.IsGenericList())
            {
                var genericArguments = itemType.GetGenericArguments();
                if (genericArguments != null && genericArguments.Length == 1)
                {
                    var arg = genericArguments[0];

                    if (arg.IsUInt16() || arg.IsInt32())
                    {
                        return valueExpress + " = table.Get<List<uint>>(\"" + itemName + "\");\r\n";
                    }
                    else if (arg.IsInt16() || arg.IsInt32())
                    {
                        return valueExpress + " = table.Get<List<int>>(\"" + itemName + "\");\r\n";
                    }
                    else if (arg.IsString())
                    {
                        return valueExpress + " = table.Get<List<string>>(\"" + itemName + "\");\r\n";
                    }
                    else if (arg.IsUInt64())
                    {
                        return valueExpress + " = table.Get<List<ulong>>(\"" + itemName + "\");\r\n";
                    }
                    else if (arg.IsInt64())
                    {
                        return valueExpress + " = table.Get<List<long>>(\"" + itemName + "\");\r\n";
                    }
                    else if (arg.IsBoolean())
                    {
                        return valueExpress + " = table.Get<List<bool>>(\"" + itemName + "\");\r\n";
                    }
                    else
                    {
                        return valueExpress + " = table.Get<List<" + TypeToClassName(arg) + ">>(\"" + itemName + "\");\r\n";
                    }
                }
                else
                {
                    TipError("{0}, List泛型超过参数超过一个 .. 暂不支持转换。", m_type.FullName);
                    return string.Empty;
                }
            }
            else if (itemType.IsClass)
            {
                //if (Has<ProtoContractAttribute>(itemType))
                //{
                var code = "var val" + itemName + " = table.Get<LuaTable>(\"" + itemName + "\");\r\n";
                code = code + "var inst" + itemName + " = new " + TypeToClassName(itemType) + "();\r\n";
                code = code + valueExpress + " = inst" + itemName + "." + METHOD_POP_NAME + "(" + "val" + itemName + ");\r\n";
                return code;
                //}
                //else
                //{
                //    TipError("{0}, 不含 ProtoContractAttribute .. 不支持转换。", m_type.FullName);
                //    return string.Empty;
                //}
            }
            else
            {
                return string.Empty;
            }
        }
    }

    //private string PushList(string valueExpress, string methodName, string luaApi)
    //{

    //}

    private void LuaTableToInstace(ref StringBuilder sb)
    {
        sb.Append("\r\n");
        sb.Append("\t\tpublic static void ");
        //sb.Append(TypeToClassName(this.m_type));
        //sb.Append(" ");
        sb.Append(METHOD_POP_NAME);
        sb.Append("(this ");
        sb.Append(RealClassName());
        sb.Append(" msg, ");
        sb.Append("LuaTable table)");
        sb.Append("\r\n\t\t{\r\n");
        sb.Append("\t\t\t");
        //sb.Append(LUA_API);
        //sb.Append(".lua_newtable(L);");
        //sb.Append("\r\n");

        //sb.Append("var inst = new ");
        //sb.Append(TypeToClassName(this.m_type));
        //sb.Append("();\r\n");

        if (m_infoAndAttributes.Count > 0)
        {
            foreach (var item in m_infoAndAttributes)
            {
                ItemToLuaTableKVP2(ref sb, item);
            }
        }

        //sb.Append("return inst;\r\n");
        sb.Append("\t\t}");
    }
}


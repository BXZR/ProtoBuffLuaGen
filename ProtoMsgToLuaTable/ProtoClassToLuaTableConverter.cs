using System;
using System.Text;
using ProtoBuf;

public class ProtoClassToLuaTableConverter : BaseProtoConverter
{

    const string DEFAULT_EXTRA_NAME = "_LuaTableConverter";
    const string DEFAULT_EXT = "cs";
    const string METHOD_NAME = "ToLuaTable";

    public ProtoClassToLuaTableConverter(Type type, string saveDir, string extraName = DEFAULT_EXTRA_NAME, string ext = DEFAULT_EXT) : base(type, saveDir, extraName, ext)
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
        //sb.Append(m_type.Name);
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

    protected override void MakeMiddle(ref StringBuilder sb)
    {
        sb.Append("\r\n");
        sb.Append("\t\tpublic static LuaTable ");
        sb.Append(METHOD_NAME);
        sb.Append("(this ");
        //TipError("MakeMiddle type's fullname = {0}", m_type.FullName.ToString());
        sb.Append(RealClassName());
        sb.Append(" msg, ");
        sb.Append("LuaEnv luaEnv)");
        sb.Append("\r\n\t\t{\r\n");
        sb.Append("\t\t\tvar table = luaEnv.NewTable();");
        sb.Append("\r\n");

        if (m_infoAndAttributes != null && m_infoAndAttributes.Count > 0)
        {
            foreach (var item in m_infoAndAttributes)
            {
                if (item == null)
                {
                    continue;
                }
                ProtoMemberToTableKV(ref sb, item);
            }
        }
        sb.Append("\t\t\treturn table;");
        sb.Append("\r\n\t\t}");
    }

    private void ProtoMemberToTableKV(ref StringBuilder sb, BaseInfoAndAttribute infoAndAttr)
    {
        var itemType = infoAndAttr.InfoType;
        var itemName = infoAndAttr.InfoName;

        // 将枚举转成int 传入， lua中有对应的 lua enum
        if (itemType.IsEnum)
        {
            CodeLine(ref sb, "string", "int", itemName, "(int)", "枚举转化成 Int 进行传递");
        }
        else
        {
            var typeToStr = TypeExtentions.TypeToString(itemType);
            if (!string.IsNullOrEmpty(typeToStr))
            {
                CodeLine(ref sb, "string", typeToStr, itemName, string.Empty);
            }
            else
            {
                //Tip("Type = {0} 在 TypeExtentions 中预定义类型中找不到。", itemType.FullName);
                if (itemType.IsClass)
                {
                    //Tip("{0}  是 Class 类型  {1}", itemName, itemType);
                    if (itemType.IsGenericList())
                    {
                        var genericArguments = itemType.GetGenericArguments();
                        if (genericArguments != null)
                        {
                            if (genericArguments.Length == 1)
                            {
                                var arg = genericArguments[0];
                                if (Has<ProtoContractAttribute>(arg))
                                {
                                    var argNamespace = arg.Namespace;
                                    // 如果这个类的命名空间不在同一个命名空间中就将引入它的命名空间
                                    if (!string.IsNullOrEmpty(argNamespace) && !argNamespace.Equals(this.m_type.Namespace))
                                    {
                                        m_headerUsings.Add(argNamespace);
                                    }
                                    CodeLineLuaTableList(ref sb, itemName);
                                }
                                else
                                {
                                    TipError("{0} GenericList arg's type = {1} 无 ProtoContactAttribute ", itemName, arg);
                                }
                            }
                            else
                            {
                                TipError("注意： GenericList  泛型参数个数 = {0}, 暂时不支持解析！" + genericArguments.Length);
                            }
                        }
                        //TipError("||| ********* " + itemName + "  是 GenericList 类型 " + itemType);
                    }
                    else if (itemType.IsArray)
                    {
                        TipError("**********   注意：Type = {0} is Array 类型, 暂时不支持解析！" + itemType.FullName + "   ********");
                    }
                    else
                    {
                        TypeToLuaTable(ref sb, itemType, itemName);
                    }
                }
                else if (itemType.IsArray)
                {
                    TipError(itemName + "  是 Array 类型 " + itemType);
                }
            }
        }
    }



    private void TypeToLuaTable(ref StringBuilder sb, Type type, string itemName)
    {
        var attrs = type.GetCustomAttributes(typeof(ProtoContractAttribute), false);
        if (attrs != null && attrs.Length > 0)
        {
            var annotation = string.Format("Type = {0} 消息的LuaTable", type.FullName);
            CodeLineLuaTable(ref sb, itemName, annotation);
        }
    }

    private void CodeLine(ref StringBuilder sb, string keyTypeStr, string valueTypeStr, string itemName, string cast = null, string annotation = null)
    {
        if (!string.IsNullOrEmpty(annotation) && !annotation.StartsWith("//"))
        {
            annotation = "//" + annotation;
        }
        var code = string.Format(CodeTemplates.TPL_LINE_SET_TABLE_KVP_WITH_CAST_AND_ANNOTIONS, keyTypeStr, valueTypeStr, itemName, cast, annotation);
        code = code.Replace("'", "\"");
        sb.Append(code).Append("\r\n");
    }

    private void CodeLineLuaTable(ref StringBuilder sb, string itemName, string annotation = null)
    {
        if (!string.IsNullOrEmpty(annotation) && !annotation.StartsWith("//"))
        {
            annotation = "//" + annotation;
        }
        var code = string.Format(CodeTemplates.TPL_LINE_SET_TABLE_VALUE_IS_LUA_TABLE, itemName, METHOD_NAME, annotation);
        code = code.Replace("'", "\"");
        sb.Append(code).Append("\r\n");
    }

    private void CodeLineLuaTableList(ref StringBuilder sb, string itemName)
    {
        var code = string.Format(CodeTemplates.TPL_LINE_SET_TABLE_VALUE_IS_LUA_TABLES, itemName);
        code = code.Replace("'", "\"");
        code = code.Replace("[[", "{");
        code = code.Replace("]]", "}");
        sb.Append(code).Append("\r\n");
    }

    protected override void MakeFooter(ref StringBuilder sb)
    {
        sb.Append("\r\n\t}\r\n");
        sb.Append("}");
    }
}
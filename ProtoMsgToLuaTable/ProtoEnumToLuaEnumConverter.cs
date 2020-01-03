using System;
using System.Reflection;
using System.Text;
using ProtoBuf;

/// <summary>
/// 将Proto枚举转换成Lua Table
/// </summary>
public class ProtoEnumToLuaEnumConverter : BaseProtoConverter
{

    public const string DEFAULT_EXTRA_NAME = "";
    public const string DEFAULT_EXT = "lua";

    public ProtoEnumToLuaEnumConverter(Type type, string saveDir, string extraName = DEFAULT_EXTRA_NAME, string ext = DEFAULT_EXT) : base(type, saveDir, extraName, ext)
    {
    }

    protected override Type[] GetAttributes()
    {
        return new Type[] { typeof(ProtoEnumAttribute) };
    }

    protected override void MakeDescription(ref StringBuilder sb)
    {
        sb.Append("--- C# Proto Enum : ");
        sb.Append(m_type.FullName);
        //sb.Append("\r\n--- 的 LUA 枚举");
        sb.Append("\r\n");
    }

    protected override void MakeHeader(ref StringBuilder sb)
    {
        //sb.Append("\r\n");
        sb.Append("local ");
        sb.Append(m_type.Name);
        sb.Append(" = {\r\n");
    }

    protected override void MakeMiddle(ref StringBuilder sb)
    {
        if (m_infoAndAttributes != null && m_infoAndAttributes.Count > 0)
        {
            var count = 0;
            var attrCount = m_infoAndAttributes.Count;
            foreach (var item in m_infoAndAttributes)
            {
                count++;
                if (item == null || item.Attribute == null)
                {
                    continue;
                }
                ProtoEnumToLuaEnum(ref sb, m_type, item.InfoName, item.Attribute as ProtoEnumAttribute, count == attrCount);
            }
        }
    }
    private void ProtoEnumToLuaEnum(ref StringBuilder sb, Type itemType, string itemName, ProtoEnumAttribute protoEnumAttribute, bool isLastOne = false)
    {
        if (protoEnumAttribute != null)
        {
            sb.Append("\t");
            sb.Append(protoEnumAttribute.Name);
            sb.Append(" = ");
            // proto 2+
            //sb.Append(protoEnumAttribute.Value.ToString());
            // proto 3+ 
            var enums = Enum.GetValues(itemType);
            foreach (var item in enums)
            {
                if (item.ToString().Equals(itemName))
                {
                    sb.Append((int)item);
                    break;
                }
            }
            if (isLastOne)
            {
                sb.Append("\r\n");
            }
            else
            {
                sb.Append(",\r\n");
            }
        }
    }

    protected override void MakeFooter(ref StringBuilder sb)
    {
        sb.Append("}\r\n");
        sb.Append("return ");
        sb.Append(m_type.Name);
    }

    protected override BindingFlags GetBindingFlags()
    {
        return BindingFlags.Static | BindingFlags.Public;
    }
}

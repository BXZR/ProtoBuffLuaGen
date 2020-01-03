using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

public abstract class BaseProtoConverter : IDisposable
{

    protected static bool TIP_ENABLE = true;

    protected Type m_type;
    protected Type[] m_attributeTypes;
    protected string m_saveDir;
    protected string m_extraName;
    protected string m_ext;
    protected List<BaseInfoAndAttribute> m_infoAndAttributes;

    protected StringBuilder m_headerStringBuilder;
    protected StringBuilder m_middleStringBuilder;
    protected StringBuilder m_footerStringBuilder;

    protected HashSet<string> m_headerUsings = new HashSet<string>();

    protected BaseProtoConverter(Type type, string saveDir, string extraName, string ext)
    {
        this.m_type = type;
        this.m_saveDir = saveDir;
        this.m_extraName = extraName;
        this.m_ext = ext;
    }

    /// <summary>
    /// 获取标签类型
    /// </summary>
    /// <returns></returns>
    protected abstract Type[] GetAttributes();

    /// <summary>
    /// 转化
    /// </summary>
    public virtual void Convert()
    {
        try
        {
            m_attributeTypes = GetAttributes();

            this.m_headerStringBuilder = new StringBuilder();
            this.m_middleStringBuilder = new StringBuilder();
            this.m_footerStringBuilder = new StringBuilder();

            m_infoAndAttributes = new List<BaseInfoAndAttribute>();

            FilterProperties(ref m_infoAndAttributes);
            FilterFieldes(ref m_infoAndAttributes);

            MakeDescription(ref this.m_headerStringBuilder);
            MakeMiddle(ref this.m_middleStringBuilder);
            MakeFooter(ref this.m_footerStringBuilder);
            MakeHeader(ref this.m_headerStringBuilder);

            SaveFile();
        }
        catch (Exception ex)
        {
            Tip("转换工具{0}, 转化 类型 = {1}, 失败。\r\n {2}\r\n {3}", GetType().Name, m_type == null ? "Null" : m_type.Name, ex.Message, ex.StackTrace);
        }
    }

    /// <summary>
    /// 创建头部描述
    /// </summary>
    /// <param name="sb"></param>
    protected abstract void MakeDescription(ref StringBuilder sb);

    /// <summary>
    /// 创建代码头部
    /// </summary>
    /// <param name="sb"></param>
    protected abstract void MakeHeader(ref StringBuilder sb);

    /// <summary>
    /// 创建代码中间部分
    /// </summary>
    /// <param name="sb"></param>
    protected abstract void MakeMiddle(ref StringBuilder sb);

    /// <summary>
    /// 创建代码尾部
    /// </summary>
    /// <param name="sb"></param>
    protected abstract void MakeFooter(ref StringBuilder sb);

    protected virtual BindingFlags GetBindingFlags()
    {
        return BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public;
    }

    /// <summary>
    /// 根据属性的标签属性获取属性信息
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="bindingFlags"></param>
    protected virtual void FilterProperties(ref List<BaseInfoAndAttribute> infos)
    {
        if (m_attributeTypes != null)
        {
            var bindingFlags = GetBindingFlags();
            foreach (var attrTypeItem in m_attributeTypes)
            {
                var properties = m_type.GetProperties(bindingFlags);
                if (properties != null)
                {
                    foreach (var item in properties)
                    {
                        var attrs = item.GetCustomAttributes(attrTypeItem, false);
                        if (attrs.Length > 0)
                        {
                            infos.Add(new PropertyAndAttribute(item, attrs[0]));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 根据成员变量标签属性获取成员变量信息
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="bindingFlags"></param>
    protected virtual void FilterFieldes(ref List<BaseInfoAndAttribute> infos)
    {
        if (m_attributeTypes != null)
        {
            var bindingFlags = GetBindingFlags();
            foreach (var attrTypeItem in m_attributeTypes)
            {
                var fields = m_type.GetFields(bindingFlags);
                if (fields != null)
                {
                    foreach (var item in fields)
                    {
                        var attrs = item.GetCustomAttributes(attrTypeItem, false);
                        if (attrs.Length > 0)
                        {
                            infos.Add(new FieldAndAttribute(item, attrs[0]));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 合并成文件的名称
    /// </summary>
    /// <returns></returns>
    protected virtual string MergePath()
    {
        var fullName = m_type.FullName;
        fullName = fullName.Replace(".", "_");  //(避免重复) 例如 : Magic.Cougar.Xxx+xxx  替换成 Magic_Cougar_Xxx_xxx
        fullName = fullName.Replace("+", "_");
        string fileName = string.Format("{0}{1}.{2}", fullName, m_extraName, m_ext);
        return Path.Combine(m_saveDir, fileName);
    }

    /// <summary>
    /// 保存成文件
    /// </summary>
    /// <param name="sb"></param>
    protected virtual void SaveFile()
    {
        var realSavePath = MergePath();
        if (File.Exists(realSavePath))
        {
            File.Delete(realSavePath);
        }
        var fileInfo = new FileInfo(realSavePath);
        var dir = fileInfo.Directory;
        if (!dir.Exists)
        {
            dir.Create();
        }
        using (var fs = new FileStream(realSavePath, FileMode.OpenOrCreate))
        {
            this.m_headerStringBuilder.Append(this.m_middleStringBuilder.ToString());
            this.m_headerStringBuilder.Append(this.m_footerStringBuilder.ToString());
            var str = this.m_headerStringBuilder.ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
        }
        Tip("转化类型 = {0} , 转换后文件 = {1} 。成功!", m_type.FullName.ToString(), realSavePath);
    }

    /// <summary>
    /// Type 转 class的名称
    /// </summary>
    /// <returns></returns>
    protected virtual string TypeToClassName()
    {
        var full = m_type.FullName;
        var ns = m_type.Namespace;
        if (!string.IsNullOrEmpty(ns))
        {
            full = full.Replace(ns, "");
            full = full.Substring(1);
        }
        full = full.Replace("+", "_");
        return full;
    }

    /// <summary>
    /// 类的名称
    /// </summary>
    /// <returns></returns>
    protected virtual string RealClassName()
    {
        var className = m_type.FullName;
        className = className.Replace("+", "."); //内部类替换
        return className;
    }


    protected virtual string TypeToClassName(Type itemType)
    {
        var full = itemType.FullName;
        return full.Replace("+", ".");
    }
    /// <summary>
    /// 是否有属性标签
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    protected virtual bool Has<E>(Type type) where E : Attribute
    {
        var attrs = type.GetCustomAttributes(typeof(E), false);
        return attrs != null && attrs.Length > 0;
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="tip"></param>
    /// <param name="formats"></param>
    protected virtual void Tip(string tip, params object[] formats)
    {
        if (TIP_ENABLE)
        {
           Console.WriteLine(tip, formats);
        }
    }

    protected virtual void TipError(string tip, params object[] formats)
    {
        Console.WriteLine(tip, formats);
    }

    public void Dispose()
    {
        if (m_infoAndAttributes != null)
        {
            foreach (var item in m_infoAndAttributes)
            {
                item.Dispose();
            }
            m_infoAndAttributes.Clear();
            m_infoAndAttributes = null;
        }
        this.m_headerStringBuilder = null;
        this.m_middleStringBuilder = null;
        this.m_footerStringBuilder = null;
    }
}


public class PropertyAndAttribute : BaseInfoAndAttribute
{
    public PropertyInfo PropertyInfo
    {
        protected set;
        get;
    }

    public override Type InfoType
    {
        get
        {
            return PropertyInfo.PropertyType;
        }

        protected set
        {
            base.InfoType = value;
        }
    }

    public override string InfoName
    {
        get
        {
            return PropertyInfo.Name;
        }

        protected set
        {
            base.InfoName = value;
        }
    }

    public PropertyAndAttribute(PropertyInfo propertyInfo, object attribute)
    {
        this.PropertyInfo = propertyInfo;
        this.Attribute = attribute;
    }
}

public class FieldAndAttribute : BaseInfoAndAttribute
{
    public FieldInfo FieldInfo
    {
        protected set;
        get;
    }

    public override Type InfoType
    {
        get
        {
            return FieldInfo.FieldType;
        }

        protected set
        {
            base.InfoType = value;
        }
    }

    public override string InfoName
    {
        get
        {
            return FieldInfo.Name;
        }

        protected set
        {
            base.InfoName = value;
        }
    }

    public FieldAndAttribute(FieldInfo fieldInfo, object attribute)
    {
        this.FieldInfo = fieldInfo;
        this.Attribute = attribute;
    }
}

public class BaseInfoAndAttribute : IDisposable
{
    public object Attribute
    {
        protected set;
        get;
    }

    public virtual Type InfoType
    {
        protected set;
        get;
    }

    public virtual string InfoName
    {
        protected set;
        get;
    }

    public virtual void Dispose()
    {
        Attribute = null;
    }
}
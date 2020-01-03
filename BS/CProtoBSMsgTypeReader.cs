using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;
namespace Magic.GameEditor
{
    public class CProtoBSMsgTypeReader
    {
        private string m_strProtoPath;
        private string m_strInputFile;

        private Dictionary<uint, string> m_typeToMsg = new Dictionary<uint, string>();
        private Dictionary<uint, string> m_typeToExcludeLuaMsg = new Dictionary<uint, string>();
        private Dictionary<uint, string> m_customTypeMsg = new Dictionary<uint, string>();
        //private Dictionary<string, DescriptorProto> m_customTypeMsg = new Dictionary<uint, string>();
        public Dictionary<string, DescriptorProto> Messages
        {
            get;
            private set;
        }
        public static Dictionary<string, Dictionary<string, DescriptorProto>> NameSapceMessages
        {
            get;
            private set;
        }
        public Dictionary<string, DescriptorProto> MessagesReceive
        {
            get;
            private set;
        }
        private FileDescriptorSet allProtoSet;
        private FileDescriptorSet excludeLuaProtoSet;
        public Dictionary<uint, string> GetTypeToMsg()
        {
            return m_typeToMsg;
        }
        public Dictionary<uint, string> GetTypeToExcludeLuaMsg()
        {
            return m_typeToExcludeLuaMsg;
        }
        public Dictionary<uint, string> GetCustomMsgType()
        {
            return m_customTypeMsg;
        }
        public void AddCustomMessage(uint type, string name)
        {
            if (m_customTypeMsg.ContainsKey(type))
                return;

            m_customTypeMsg.Add(type, name);
        }
        public FileDescriptorSet GetProtoFileSet()
        {
            return allProtoSet;
        }
        public FileDescriptorSet GetExcludeLuaProtoFileSet()
        {
            return excludeLuaProtoSet;
        }
        public bool ReadFile()
        {
            //Read
            using (StreamReader sr = new StreamReader(m_strInputFile, new System.Text.UTF8Encoding(false)))
            {
                string str = sr.ReadToEnd();

                //匹配所有得到MSGTYPE_DECLARE
                string regstr = @"MSGTYPE_DECLARE[\s\S]*";
                Match m = Regex.Match(str, regstr, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                //匹配所有的MSGTYPE_DECLARE()中的内容
                string regstr1 = @"^MSGTYPE_DECLARE\((?<MsgDefine>[\s\S]*?)\)";
                MatchCollection mCollect = Regex.Matches(m.Value, regstr1, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                //去空格、制表符
                List<string> lstParse = new List<string>();
                foreach(Match v in mCollect)
                {
                    string sv = Regex.Replace(v.Groups["MsgDefine"].Value, @"[\s]|[\t]*", "");
                    lstParse.Add(sv);
                }

                //解析每一行
                Dictionary<string, uint> dctNameToType = new Dictionary<string, uint>();
                Dictionary<uint, List<string>> dctTypeToName = new Dictionary<uint, List<string>>();

                //这个对象存储去掉_的消息号
                Dictionary<uint, List<string>> dctTypeToNameParse = new Dictionary<uint, List<string>>();

                Action<uint, string> actAddMsg = (uint stype, string sname) =>
                {
                    dctNameToType[sname] = stype;
                    List<string> lst = null;

                    if (dctTypeToName.TryGetValue(stype, out lst))
                    {
                        lst.Add(sname);
                    }
                    else
                    {
                        lst = new List<string>();
                        lst.Add(sname);
                        dctTypeToName[stype] = lst;
                    }

                    lst = null;
                    if (dctTypeToNameParse.TryGetValue(stype, out lst))
                    {
                        lst.Add(sname.Replace("_", ""));
                    }
                    else
                    {
                        lst = new List<string>();
                        lst.Add(sname.Replace("_", ""));
                        dctTypeToNameParse[stype] = lst;
                    }
                };

                foreach(var v in lstParse)
                {
                    //_MSG_CREATE_GAME,_MSG_LP_SERVER_BASE+1
                    string[] a1 = v.Split(',');
                    if (a1.Length == 2)
                    {
                        //a1[0] = _MSG_CREATE_GAME
                        string name = a1[0];
                        //a1[1] = _MSG_LP_SERVER_BASE + 1
                        string type = a1[1];

                        //_MSG_LP_SERVER_BASE+1
                        string[] atype = type.Split('+');
                        if (atype.Length == 2)
                        {
                            uint basetype = 0;
                            if (dctNameToType.TryGetValue(atype[0], out basetype))
                            {
                                uint stype = 0;
                                if(uint.TryParse(atype[1], out stype))
                                {
                                    stype += basetype;
                                    actAddMsg(stype, name);
                                }
                                else
                                {
                                    
                                }
                            }
                        }
                        else
                        {
                            uint stype = 0;
                            if (uint.TryParse(atype[0], out stype))
                            {
                                actAddMsg(stype, name);
                            }
                            else
                            {
                                //重定义的消息，如MSGTYPE_DECLARE( _MSG_BOOTH , _MSG_ACTION)
                                if (dctNameToType.TryGetValue(atype[0], out stype))
                                {
                                    actAddMsg(stype, name);
                                }
                            }
                        }
                    }
                }

                //解析Proto
                Func<DirectoryInfo,string,FileDescriptorSet> lambdaFunc = (DirectoryInfo scanDir,string exclude_suffix) =>
                {
                    FileDescriptorSet lset = new FileDescriptorSet();

                    lset.AddImportPath(Directory.GetCurrentDirectory());
                    lset.AddImportPath(scanDir.FullName);

                    var files = GenLuaForProto.GetFilesExclude(scanDir, exclude_suffix);// scanDir.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
                    foreach (FileInfo file in files)
                    {
                        string proto_file = file.FullName.Replace(scanDir.FullName + Path.DirectorySeparatorChar, "");
                        //int exitCode = 0;
                        if (!lset.Add(proto_file, true))
                        {
                            //exitCode = 1;
                        }
                    }
                    lset.Process();

                    return lset;
                };
                //拿到所有解析完的Proto文件做分析对比
                allProtoSet = lambdaFunc(new DirectoryInfo(m_strProtoPath),null);
                excludeLuaProtoSet = lambdaFunc(new DirectoryInfo(m_strProtoPath), GenLuaForProto.LUASuffix);
                //dctTypeToNameParse = dctTypeToNameParse.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                #region forcs
                foreach (FileDescriptorProto v in allProtoSet.Files)
                {
                    foreach (DescriptorProto vi in v.MessageTypes)
                    {
                        string src = vi.Name;
                        bool bFind = false;
                        foreach (var vj in dctTypeToNameParse)
                        {
                            foreach (var vk in vj.Value)
                            {
                                //先一次精确匹配，再一次模糊搜索
                                string key = vk;
                                if (src.Equals(key, StringComparison.OrdinalIgnoreCase))
                                {
                                    bFind = true;

                                    if (v.Package.Length == 0)
                                        m_typeToMsg[vj.Key] = vi.Name;
                                    else
                                        m_typeToMsg[vj.Key] = v.Package + "." + vi.Name;

                                    break;
                                }
                            }
                        }

                        if (!bFind)
                        {
                            foreach (var vj in dctTypeToNameParse)
                            {
                                foreach (var vk in vj.Value)
                                {
                                    string key = vk;
                                    string key2 = "(?is)";
                                    for (int i = 0; i < key.Length; ++i)
                                    {
                                        key2 += key[i].ToString();
                                        if (i < key.Length - 1)
                                            key2 += @"\s*";
                                    }

                                    Match mj = Regex.Match(src, key2, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                    if (mj.Success && !m_typeToMsg.ContainsKey(vj.Key))
                                    {
                                        bFind = true;

                                        if (v.Package.Length == 0)
                                            m_typeToMsg[vj.Key] = vi.Name;
                                        else
                                            m_typeToMsg[vj.Key] = v.Package + "." + vi.Name;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion forcs
                foreach (FileDescriptorProto v in excludeLuaProtoSet.Files)
                {
                    foreach (DescriptorProto vi in v.MessageTypes)
                    {
                        AddMessages(vi, v.Package);//把所有message缓存起来
                    }
                }
                foreach (var kv in Messages)
                {
                    string ns = "Magic.Cougar";
                    string cls_name = kv.Key;
                    Dictionary<string, DescriptorProto> dic;
                    if (!cls_name.StartsWith(ns))
                    {
                        ns = kv.Key.Substring(0, cls_name.IndexOf("."));
                    }
                    if (NameSapceMessages.ContainsKey(ns))
                    {
                        NameSapceMessages.TryGetValue(ns, out dic);
                    }
                    else
                    {
                        dic = new Dictionary<string, DescriptorProto>();
                        NameSapceMessages.Add(ns, dic);
                    }
                    dic.Add(cls_name, kv.Value);
                }
                #region forexcludelua
                foreach (FileDescriptorProto v in excludeLuaProtoSet.Files)
                {
                    foreach (DescriptorProto vi in v.MessageTypes)
                    {
                        string src = vi.Name;
                        bool bFind = false;
                        foreach (var vj in dctTypeToNameParse)
                        {
                            foreach (var vk in vj.Value)
                            {
                                //先一次精确匹配，再一次模糊搜索
                                string key = vk;
                                if (src.Equals(key, StringComparison.OrdinalIgnoreCase))
                                {
                                    bFind = true;

                                    if (v.Package.Length == 0)
                                        m_typeToExcludeLuaMsg[vj.Key] = vi.Name;
                                    else
                                        m_typeToExcludeLuaMsg[vj.Key] = v.Package + "." + vi.Name;

                                    break;
                                }
                            }
                        }

                        if (!bFind)
                        {
                            foreach (var vj in dctTypeToNameParse)
                            {
                                foreach (var vk in vj.Value)
                                {
                                    string key = vk;
                                    string key2 = "(?is)";
                                    for (int i = 0; i < key.Length; ++i)
                                    {
                                        key2 += key[i].ToString();
                                        if (i < key.Length - 1)
                                            key2 += @"\s*";
                                    }

                                    Match mj = Regex.Match(src, key2, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                    if (mj.Success && !m_typeToExcludeLuaMsg.ContainsKey(vj.Key))
                                    {
                                        bFind = true;

                                        if (v.Package.Length == 0)
                                            m_typeToExcludeLuaMsg[vj.Key] = vi.Name;
                                        else
                                            m_typeToExcludeLuaMsg[vj.Key] = v.Package + "." + vi.Name;

                                        break;
                                    }
                                }
                            }
                        }
                        if (bFind)
                        {
                            DescriptorProto proto_msg;
                            Messages.TryGetValue(GetMessageCSharpName(vi, v.Package), out proto_msg);
                            AddMessagesToReset(proto_msg, v);
                            continue;
                        }
                    }
                }
                #endregion forexcludelua

                //foreach(var kv in MessagesReceive)
                //{
                //    Console.WriteLine(kv.Key + "=======" + kv.Value.Name);
                //}
                //排序
                m_typeToMsg = m_typeToMsg.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                m_typeToExcludeLuaMsg = m_typeToExcludeLuaMsg.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            }

            return true;
        }
        public static string GetMessageCSharpName(DescriptorProto proto_msg, string ns)
        {
            return string.IsNullOrEmpty(ns) ? "Magic.Cougar." + proto_msg.Name : ns + "." + proto_msg.Name;
        }
        public static string GetMessageCSharpName(string proto_msg_name, string ns)
        {
            return string.IsNullOrEmpty(ns) ? "Magic.Cougar." + proto_msg_name : ns + "." + proto_msg_name;
        }
        public void AddMessages(DescriptorProto proto_msg, string ns)
        {
            foreach (DescriptorProto innerProto in proto_msg.NestedTypes)
            {
                AddMessages(innerProto, GetMessageCSharpName(proto_msg,ns));
            }
            Messages.Add(GetMessageCSharpName(proto_msg,ns), proto_msg);
        }
        public void AddMessagesToReset(DescriptorProto proto_msg,FileDescriptorProto fileDescriptor)
        {
            var key = GetMessageCSharpName(proto_msg.Name, fileDescriptor.Package);
            var fields_list = proto_msg.Fields;
            foreach (var field in fields_list)
            {
                if (field.type == FieldDescriptorProto.Type.TypeMessage)//todo
                {
                    //var key_nested = GetMessageCSharpName(origin_typename, ns);
                    //DescriptorProto depend_proto_msg;
                    //if(MessagesReceive.TryGetValue(key_nested, out depend_proto_msg))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    Messages.TryGetValue(key_nested, out depend_proto_msg);
                    //    Console.WriteLine("key_nested " + key_nested);
                    //    AddMessagesToReset(depend_proto_msg, ns);
                    //}
                }
            }
            MessagesReceive.Add(key, proto_msg);
        }
        public CProtoBSMsgTypeReader(string inputFile, string protoPath)
        {
            this.m_strInputFile = inputFile;
            this.m_strProtoPath = protoPath;
            this.Messages = new Dictionary<string, DescriptorProto>();
            NameSapceMessages = new Dictionary<string, Dictionary<string, DescriptorProto>>();
            this.MessagesReceive = new Dictionary<string, DescriptorProto>();
        }
    }
}
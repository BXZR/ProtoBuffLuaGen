using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Magic.GameEditor
{
    public enum EPROTOLPGEN
    {
        GEN_TYPE_DEFINE = 0,
        GEN_TYPE_MAP = 1,
    }

    public class CProtoLPMsgTypeNode
    {
        public CProtoLPMsgTypeNode beginNode;
        public List<CProtoLPMsgTypeNode> childNode = new List<CProtoLPMsgTypeNode>();
        public CProtoLPMsgTypeNode endNode;
        public int msgType;
        public string msgName;
        public string msgNs;
        public string ToString(EPROTOLPGEN type)
        {
            string str = "";
            
            if (type == EPROTOLPGEN.GEN_TYPE_DEFINE)
            {
                str += System.Environment.NewLine;
                str += string.Format("LPMsgDefine.{0} = {1}{2}", beginNode.msgName, beginNode.msgType, System.Environment.NewLine);
                foreach (var v in childNode)
                {
                    str += string.Format("LPMsgDefine.{0} = {1}{2}", v.msgName, v.msgType, System.Environment.NewLine);
                }
                str += string.Format("LPMsgDefine.{0} = {1}{2}", endNode.msgName, endNode.msgType, System.Environment.NewLine);
                str += System.Environment.NewLine;
                str += System.Environment.NewLine;


            }
            else
            {
                str += System.Environment.NewLine;
                str += string.Format("    [{0}] = \"{1}.{2}\",{3}", beginNode.msgType, msgNs, beginNode.msgName, System.Environment.NewLine);
                foreach (var v in childNode)
                {
                    str += string.Format("    [{0}] = \"{1}.{2}\",{3}", v.msgType, msgNs, v.msgName, System.Environment.NewLine);
                }
                str += string.Format("    [{0}] = \"{1}.{2}\",{3}", endNode.msgType, msgNs, endNode.msgName, System.Environment.NewLine);

                str += System.Environment.NewLine;
                str += System.Environment.NewLine;
            }
            

            return str;
        }

        public CProtoLPMsgTypeNode(string ns, string name)
        {
            msgName = name;
            msgNs = ns;
        }

        public CProtoLPMsgTypeNode(string ns, string name, int type)
        {
            msgName = name;
            msgNs = ns;
            msgType = type;
        }
    }

    public class CProtoLPMsgTypeReader
    {
        private string m_ns;
        private string m_strInputFile;
        private List<CProtoLPMsgTypeNode> m_lstNode = new List<CProtoLPMsgTypeNode>();

        public List<CProtoLPMsgTypeNode> GetListNode()
        {
            return m_lstNode;
        }

        public bool ReadFile()
        {
            //Read
            using (StreamReader sr = new StreamReader(m_strInputFile, new System.Text.UTF8Encoding(false)))
            {
                string str = sr.ReadToEnd();
                //string regstr = "enum EProtoMsgs\\r\\n{(?<ProtoMsgs>[\\s\\S]*?)\r\n};";
                string regstr = @"enum EProtoMsg[\s\S]*{(?<ProtoMsgs>[\s\S]*)};";
                Match m = Regex.Match(str, regstr, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                string ss1 = m.Groups["ProtoMsgs"].Value;
                ss1 = ss1.Replace("\r", "");
                ss1 = ss1.Replace("\n", "");
                ss1 = ss1.Replace("\t", "");
                ss1 = ss1.Replace(" ", "");
                string[] ss2 = ss1.Split(',');

                string iRegStr = @"EProtoMsg_PROTO_(?<MsgName>[\s\S]*)|EProtoMsg_(?<MsgName>[\s\S]*)";
                
                
                CProtoLPMsgTypeNode node = null;

                Func<string, string> regexMsgNameFunc = msgName =>
                {
                    Match im = Regex.Match(msgName, iRegStr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    bool b = im.Success;

                    msgName = im.Groups["MsgName"].Value;
                    msgName = "Msg" + msgName;

                    return msgName;
                };

                foreach (var v in ss2)
                {
                    if (v.Length == 0)
                        continue;

                    string[] av = v.Split('=');
                    if (av.Length == 2)
                    {
                        if (node == null)
                        {
                            node = new CProtoLPMsgTypeNode(m_ns, "NodeRoot", 0);

                            string msgName = regexMsgNameFunc(av[0]);
                            int msgType = int.Parse(av[1]);

                            node.beginNode = new CProtoLPMsgTypeNode(m_ns, msgName, msgType);
                            
                            m_lstNode.Add(node);
                        }
                        else
                        {
                            if (node.beginNode != null)
                            {
                                string msgName = regexMsgNameFunc(av[0]);
                                int msgType = node.beginNode.msgType + node.childNode.Count + 1;

                                node.endNode = new CProtoLPMsgTypeNode(m_ns, msgName, msgType);

                                node = null;
                            }
                        }
                    }
                    else
                    {
                        string msgName = regexMsgNameFunc(av[0]);
                        Console.WriteLine("msgName=" + msgName);
                        int msgType = node.beginNode.msgType + node.childNode.Count + 1;
                        CProtoLPMsgTypeNode childNode = new CProtoLPMsgTypeNode(m_ns, msgName, msgType);
                        node.childNode.Add(childNode);
                    }
                }
            }

            return true;
        }

        public CProtoLPMsgTypeReader(string inputFile, string ns)
        {
            this.m_ns = ns;
            this.m_strInputFile = inputFile;
        }
    }
}
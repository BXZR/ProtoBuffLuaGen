using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace Magic.GameEditor
{
    public class CProtoBSMsgTypeLuaTableWriter
    {
        private string m_strOutputPath;
        private CProtoBSMsgTypeReader m_reader;
        public bool WriteLuaTableFile()
        {
            GenCSProtoHandle();
            GenCSLuaTable();
            return true;
        }

        public CProtoBSMsgTypeLuaTableWriter(CProtoBSMsgTypeReader reader, string outputPath)
        {
            this.m_strOutputPath = outputPath;
            this.m_reader = reader;

            if (!Directory.Exists(m_strOutputPath))
                Directory.CreateDirectory(m_strOutputPath);
            else
            {
                DirectoryInfo directInfo = new DirectoryInfo(m_strOutputPath);
                FileInfo[] fileInfo = directInfo.GetFiles("*.cs", SearchOption.AllDirectories);
                foreach (var file in fileInfo)
                {
                    file.Delete();
                }
            }
        }

        private void GenCSProtoHandle()
        {
            //Write
            string strOutFile = Path.GetFullPath(m_strOutputPath + "/" + "PartialProtoMsgHandler.cs");
            using (FileStream fs = new FileStream(strOutFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("///////////////////////////////");
                    sw.WriteLine("//  Generate By UnityEditor  //");
                    sw.WriteLine("// PartialProtoMsgHandler.cs //");
                    sw.WriteLine("///////////////////////////////");

                    sw.WriteLine();
                    sw.WriteLine();

                    sw.WriteLine("using System;");
                    sw.WriteLine("using ProtoBuf;");
                    sw.WriteLine("using Magic.Cougar;");
                    sw.WriteLine();

                    sw.WriteLine("namespace Magic.GameLogic");
                    sw.WriteLine("{");


                    sw.WriteLine("    public partial class ProtoMsgHandler");
                    sw.WriteLine("    {");

                    sw.WriteLine("        private void PushToLua(IntPtr L, ushort uType, IExtensible msg, Action<IntPtr, ushort> beforAction, Action<IntPtr> afterAction)");
                    sw.WriteLine("        {");
                    sw.WriteLine();
                    sw.WriteLine("            switch ((MessageType)uType)");
                    sw.WriteLine("            {");

                    Dictionary<uint, string> dctTypeToMsg = m_reader.GetTypeToExcludeLuaMsg();
                    foreach(var v in dctTypeToMsg)
                    {
                        string msgType = v.Value;
                        int nIndex = v.Value.IndexOf('.');
                        if (nIndex != -1)
                            msgType = v.Value.Substring(nIndex+1);

                        sw.WriteLine("                case MessageType.{0}:", msgType);
                        sw.WriteLine("                    {");
                        sw.WriteLine("                        var cmsg = msg as {0};", v.Value);
                        sw.WriteLine("                        if (cmsg != null)");
                        sw.WriteLine("                        {");
                        sw.WriteLine("                            if(beforAction != null && afterAction != null)");
                        sw.WriteLine("                            {");
                        sw.WriteLine("                              beforAction.Invoke(L, uType);");
                        sw.WriteLine("                              cmsg.PushToLua(this.m_luaEnv.L);");
                        sw.WriteLine("                              afterAction.Invoke(L);");
                        sw.WriteLine("                            }");
                        sw.WriteLine("                        }");
                        sw.WriteLine("                    }");
                        sw.WriteLine("                    break;");
                    }

                    sw.WriteLine("                default:");
                    sw.WriteLine("                    break;");
                    sw.WriteLine("            }");
                    sw.WriteLine("        }");
                    sw.WriteLine("    }");
                    sw.WriteLine("}");
                }
            }
        }

        private void GenCSLuaTable()
        {
            FileDescriptorSet lset = m_reader.GetExcludeLuaProtoFileSet();

            using (var converter = new ProtoConverter(m_strOutputPath, ""))
            {
                converter.Convert(lset);
            }
        }
    }
}
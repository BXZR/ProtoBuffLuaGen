using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace Magic.GameEditor
{
    public class CProtoBSMsgClsDataResetWriter
    {
        private string m_strOutputFile;
        private CProtoBSMsgTypeReader m_reader;
        StreamWriter sw;
        public bool WriteCSharpFile()
        {
            //Write
            Dictionary<string, Dictionary<string, DescriptorProto>> ns_messages = CProtoBSMsgTypeReader.NameSapceMessages;
            using (FileStream fs = new FileStream(m_strOutputFile, FileMode.Create, FileAccess.Write))
            {
                using (sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("///////////////////////////////");
                    sw.WriteLine("//  Generate By UnityEditor  //");
                    sw.WriteLine("///////////////////////////////");
                    sw.WriteLine("using System;");
                    sw.WriteLine("using System.Collections;");
                    sw.WriteLine("using ProtoBuf;");
                    sw.WriteLine("using Magic.Cougar;");
                    foreach(var ns_kv in ns_messages)
                    {
                        string ns = ns_kv.Key;
                        Dictionary<string, DescriptorProto> messages = ns_kv.Value;
                        sw.WriteLine(StringUtility.ConcatString("namespace ", ns));
                        sw.WriteLine("{");
                        foreach (var kv in messages)
                        {
                            var cls_name = kv.Key;
                            var message = kv.Value;
                            string[] names = cls_name.Split('.');
                            int start_index = 1;
                            if (cls_name.StartsWith("Magic.Cougar"))
                            {
                                start_index = 2;
                            }
                            for (int n= start_index; n < names.Length; n++)
                            {
                                var name = names[n];
                                sw.WriteLine(StringUtility.ConcatString("   public partial class ", name, " : NetClassBase{"));
                            }
                            sw.WriteLine("       public override void ClearData()");
                            sw.WriteLine("       {");

                            //sw.WriteLine(StringUtility.ConcatString("         ",cls_name," msg = (", cls_name, ")netobj;"));
                            foreach (var field in message.Fields)
                            {
                                if (field.label == FieldDescriptorProto.Label.LabelRepeated)
                                {
                                    if (IsRefType(field.type))
                                    {
                                        sw.WriteLine(StringUtility.ConcatString("           this.", field.Name, ".Clear();"));
                                    }
                                    else
                                    {
                                        sw.WriteLine(StringUtility.ConcatString("           this.", field.Name, " = null;"));
                                    }
                                    //string length_str = "Length";
                                    //if (IsRefType(field.type))
                                    //{
                                    //    length_str = "Count";
                                    //}
                                    //sw.WriteLine(StringUtility.ConcatString("           if (this.", field.Name, " != null){"));
                                    //sw.WriteLine(StringUtility.ConcatString("               for (int i = 0; i < this.", field.Name, ".", length_str, "; i++)"));
                                    //sw.WriteLine("              {");
                                    //sw.WriteLine(StringUtility.ConcatString("                   this.", field.Name, "[i]", GetStrFromFieldType(field.type)));
                                    
                                    //sw.WriteLine("              }");
                                    //sw.WriteLine("           }");
                                }
                                else
                                {
                                    if (IsRefType(field.type))
                                        sw.WriteLine(StringUtility.ConcatString("           this.", field.Name, " = null;"));
                                    else
                                    {
                                        sw.WriteLine(StringUtility.ConcatString("           this.", field.Name, GetStrFromFieldType(field.type)));
                                    }
                                    //if (IsRefType(field.type))
                                    //    sw.WriteLine(StringUtility.ConcatString("       if (this.", field.Name, " != null)"));
                                    //sw.WriteLine(StringUtility.ConcatString("       this.", field.Name, GetStrFromFieldType(field.type)));
                                }
                            }
                            sw.WriteLine("       }");
                            string right_end = "";
                            for(int n= start_index; n < names.Length; n++)
                            {
                                right_end += "}";
                            }
                            sw.WriteLine(StringUtility.ConcatString("  ",right_end));
                        }
                        sw.WriteLine("}");
                    }
                    sw.WriteLine();
                }
            }
            return true;
        }
        bool IsRefType(FieldDescriptorProto.Type ftype)
        {
            return ftype == FieldDescriptorProto.Type.TypeMessage || ftype == FieldDescriptorProto.Type.TypeGroup || ftype == FieldDescriptorProto.Type.TypeString;
        }
        string GetStrFromFieldType(FieldDescriptorProto.Type field)
        {
             string reset_line="";
            switch (field)
            {
                case FieldDescriptorProto.Type.TypeBool:
                    reset_line = " = false;";
                    break;
                case FieldDescriptorProto.Type.TypeEnum:
                case FieldDescriptorProto.Type.TypeDouble:
                case FieldDescriptorProto.Type.TypeBytes:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeUint32:
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint64:
                case FieldDescriptorProto.Type.TypeUint64:
                    reset_line = " = 0;";
                    break;
                case FieldDescriptorProto.Type.TypeFloat:
                    reset_line = " = 0f;";
                    break;
                case FieldDescriptorProto.Type.TypeString:
                    reset_line = " = \"\";";
                    break;
                case FieldDescriptorProto.Type.TypeGroup:
                case FieldDescriptorProto.Type.TypeMessage:
                    reset_line = ".ClearData();";
                    break;
                default:
                    reset_line = "";
                    break;
            }
            return reset_line;
        }
        public CProtoBSMsgClsDataResetWriter(CProtoBSMsgTypeReader reader, string outputFile)
        {
            this.m_strOutputFile = outputFile;
            this.m_reader = reader;

            if (File.Exists(m_strOutputFile))
            {
                File.Delete(m_strOutputFile);
            }
        }
    }
}
using System.IO;
using Google.Protobuf.Reflection;

namespace Magic.GameEditor
{
    public class CProtoMsgEnumLuaWriter
    {
        private string m_strProtoPath;
        private string m_strOutputFile;
        private string m_ns;
        public bool WriteLuaFile()
        {
            DirectoryInfo scanDir = new DirectoryInfo(Path.GetFullPath(m_strProtoPath));
            FileDescriptorSet lset = new FileDescriptorSet();

            lset.AddImportPath(Directory.GetCurrentDirectory());
            lset.AddImportPath(Path.GetFullPath(m_strProtoPath));

            var files = GenLuaForProto.GetFilesExclude(scanDir);// scanDir.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
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

            string dirPath = Path.GetFullPath(m_strOutputFile);
            dirPath = Path.GetDirectoryName(dirPath);
            //Write
            using (FileStream fs = new FileStream(m_strOutputFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    WriteRequireBegin(sw);

                    string createPath = Path.GetFullPath(dirPath + "/" + m_ns);
                    if (!Directory.Exists(createPath))
                        Directory.CreateDirectory(createPath);

                    foreach(FileDescriptorProto fileDesc in lset.Files)
                    {
                        foreach(DescriptorProto desc in fileDesc.MessageTypes)
                        {
                            if (desc.EnumTypes.Count != 0)
                            {
                                string createFile = Path.GetFullPath(createPath + "/" + desc.Name + "_EnumTypes.lua");
                                using (FileStream luaFileS = new FileStream(createFile, FileMode.Create, FileAccess.Write))
                                {
                                    using (StreamWriter luaFileW = new StreamWriter(luaFileS, new System.Text.UTF8Encoding(false)))
                                    {
                                        WriteRequire(sw, desc.Name);
                                        WriteEnumLuaHeaderBegin(luaFileW, fileDesc.Name, desc.Name);
                                        foreach (EnumDescriptorProto enumDesc in desc.EnumTypes)
                                        {
                                            WriteEnumLuaBegin(luaFileW, enumDesc.Name);
                                            foreach (EnumValueDescriptorProto enumValDesc in enumDesc.Values)
                                            {
                                                WriteEnumLuaValue(luaFileW, enumValDesc.Name, enumValDesc.Number);
                                            }
                                            WriteEnumLuaEnd(luaFileW, enumDesc.Name);
                                        }
                                        WriteEnumLuaHeaderEnd(luaFileW);

                                        luaFileW.Close();
                                    }

                                    luaFileS.Close();
                                }
                                
                            }

                        }
                    }

                    WriteRequireEnd(sw);

                    sw.Close();
                }

                fs.Close();
            }

            return true;
        }

        public CProtoMsgEnumLuaWriter(string protoPath, string outputFile, string ns)
        {
            this.m_strOutputFile = outputFile;
            this.m_strProtoPath = protoPath;
            this.m_ns = ns;

            if (File.Exists(m_strOutputFile))
            {
                File.Delete(m_strOutputFile);
            }
        }

        private void WriteRequireBegin(StreamWriter sw)
        {
            sw.WriteLine("--[[");
            sw.WriteLine("--  Generate By UnityEditor");
            sw.WriteLine("--  {0} ProtoMsg Enum", m_ns);
            sw.WriteLine("--]]");

            sw.WriteLine();
        }

        //private string currentProtoName;
        private void WriteRequire(StreamWriter sw, string protoName)
        {
            //if (currentProtoName != protoName)
            //{
            //    sw.WriteLine();
            //    sw.WriteLine(string.Format("{0} = {{}}", protoName));
            //}

            //currentProtoName = protoName;

            if (m_ns.Length == 0)
                sw.WriteLine("{0} = require \"Net.Enum.{1}_EnumTypes\"", protoName, protoName);
            else
                sw.WriteLine("{0} = require \"Net.Enum.{1}.{2}_EnumTypes\"", protoName, m_ns, protoName);
        }

        private void WriteRequireEnd(StreamWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine();
        }

        private void WriteEnumLuaHeaderBegin(StreamWriter sw, string protoFile, string protoName)
        {
            sw.WriteLine("--[[");
            sw.WriteLine("--  Generate By UnityEditor");
            sw.WriteLine("--  Proto:{0}  Msg:{1} ", protoFile, protoName);
            sw.WriteLine("--]]");

            sw.WriteLine();
            sw.WriteLine("local tEnums = {}");
            sw.WriteLine();
        }
        private void WriteEnumLuaBegin(StreamWriter sw, string enumTypeName)
        {
            sw.WriteLine("-------------------- Begin {0} ---------------------", enumTypeName); 
            sw.WriteLine("tEnums.{0} = {{", enumTypeName);
        }

        private void WriteEnumLuaValue(StreamWriter sw, string name, int num)
        {
            sw.WriteLine("    {0} = {1},", name, num);
        }

        private void WriteEnumLuaEnd(StreamWriter sw, string enumTypeName)
        {
            sw.WriteLine("}");
            sw.WriteLine("-------------------- End {0} ---------------------", enumTypeName);
            sw.WriteLine();
            sw.WriteLine();
        }

        private void WriteEnumLuaHeaderEnd(StreamWriter sw)
        {
            sw.WriteLine();

            sw.WriteLine("return tEnums");
        }
    }
}
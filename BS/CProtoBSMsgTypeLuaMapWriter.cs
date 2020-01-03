using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Magic.GameEditor
{
    public class CProtoBSMsgTypeLuaMapWriter
    {
        private string m_strOutputFile;
        private CProtoBSMsgTypeReader m_reader;
        public bool WriteLuaFile()
        {
            //Write
            using (FileStream fs = new FileStream(m_strOutputFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("--[[");
                    sw.WriteLine("--  Generate By UnityEditor");
                    sw.WriteLine("--]]");

                    sw.WriteLine();
                    sw.WriteLine();

                    //begin
                    sw.WriteLine("local BSMsgMap = {");

                    var dict = m_reader.GetTypeToMsg();
                    foreach(var v in dict)
                    {
                        sw.WriteLine("    [{0}] = \"{1}\",", v.Key, v.Value);
                    }

                    sw.WriteLine("}");
                    //end

                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("return BSMsgMap");
                }
            }

            return true;
        }

        public CProtoBSMsgTypeLuaMapWriter(CProtoBSMsgTypeReader reader, string outputFile)
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
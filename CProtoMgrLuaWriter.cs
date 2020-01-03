using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Magic.GameEditor
{
    public class CProtoMgrLuaWriter
    {
        private string m_strOutputFile;
        private string m_ns;
        private List<string> m_pbFiles;

        public bool WriteLuaFile()
        {
            using (FileStream fs = new FileStream(m_strOutputFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("--[[");
                    sw.WriteLine("--  Generate By UnityEditor");
                    //sw.WriteLine(string.Format("--  ModifyTime: {0}", DateTime.Now.ToString("r")));
                    sw.WriteLine("--]]");

                    sw.WriteLine();
                    sw.WriteLine();

                    //begin
                    sw.WriteLine("local tFiles = {");

                    foreach (var v in m_pbFiles)
                    {
                        sw.WriteLine(string.Format("    \"{0}\",", v));
                    }

                    sw.WriteLine("}");
                    //end

                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("return tFiles");
                }
            }

            return true;
        }

        public CProtoMgrLuaWriter(string outputFile, string ns)
        {
            this.m_strOutputFile = outputFile;
            this.m_ns = ns;
            this.m_pbFiles = new List<string>();

            if (File.Exists(m_strOutputFile))
            {
                File.Delete(m_strOutputFile);
            }
        }

        public void AddPBFileName(string strPBFile)
        {
            this.m_pbFiles.Add(strPBFile);
        }
    }
}
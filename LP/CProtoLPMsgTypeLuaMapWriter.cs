﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Magic.GameEditor
{
    public class CProtoLPMsgTypeLuaMapWriter
    {
        private string m_strOutputFile;
        private string m_ns;
        private CProtoLPMsgTypeReader m_reader;
        public bool WriteLuaFile()
        {
            //Write
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
                    sw.WriteLine("local LPMsgMap = {");

                    List<CProtoLPMsgTypeNode> lstNode = m_reader.GetListNode();
                    foreach (var v in lstNode)
                    {
                        sw.Write(v.ToString(EPROTOLPGEN.GEN_TYPE_MAP));
                    }

                    sw.WriteLine("}");
                    //end

                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("return LPMsgMap");
                }
            }

            return true;
        }

        public CProtoLPMsgTypeLuaMapWriter(CProtoLPMsgTypeReader reader, string outputFile, string ns)
        {
            this.m_strOutputFile = outputFile;
            this.m_ns = ns;
            this.m_reader = reader;

            if (File.Exists(m_strOutputFile))
            {
                File.Delete(m_strOutputFile);
            }
        }

        private string Get_uft8(string unicodeString)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Magic.GameEditor
{
    public class CProtoBSMsg2ClsTypeWriter
    {
        private string m_strOutputFile;
        private CProtoBSMsgTypeReader m_reader;
        public bool WriteCSharpFile()
        {
            //Write
            using (FileStream fs = new FileStream(m_strOutputFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
                {
                    sw.WriteLine("///////////////////////////////");
                    sw.WriteLine("//  Generate By UnityEditor  //");
                    sw.WriteLine("///////////////////////////////");

                    sw.WriteLine();
                    sw.WriteLine();

                    sw.WriteLine("using ETModel;");
                    sw.WriteLine();

                    sw.WriteLine("namespace Magic.Cougar");
                    sw.WriteLine("{");

                    sw.WriteLine("////////////////////MessageNum2Class////////////////////");
                    Dictionary<uint, string> dctTypeToMsg = m_reader.GetTypeToExcludeLuaMsg();
                    foreach (var v in dctTypeToMsg)
                    {
                        string msgName = v.Value;
                        int index = msgName.IndexOf('.');
                        if (index != -1)
                        {
                            msgName = msgName.Substring(index + 1);
                        }
                        sw.WriteLine("[Message((ushort)MessageType.{0})]", msgName);
                        sw.WriteLine(StringUtility.ConcatString(" public partial class ",msgName," { }"));
   
                    }
                    sw.WriteLine("////////////////////MessageNum2Class////////////////////");

                    sw.WriteLine("}");
                    sw.WriteLine();
                }
            }

            return true;
        }

        public CProtoBSMsg2ClsTypeWriter(CProtoBSMsgTypeReader reader, string outputFile)
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
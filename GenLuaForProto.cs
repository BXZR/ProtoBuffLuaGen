using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
namespace Magic.GameEditor
{
    public class GenLuaForProto
    {
        public static bool ExecuteProgram(string exeFilename, string workDir, string args)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = exeFilename;
            info.WorkingDirectory = workDir;
            info.UseShellExecute = false;
            info.Arguments = args;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            //info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            System.Diagnostics.Process task = null;
            bool rt = true;
            try
            {
                Console.WriteLine(string.Format("ExecuteProgram: exeFilename={0}, workDir={1}, args={2}", exeFilename, workDir, args));

                task = System.Diagnostics.Process.Start(info);
                if (task != null)
                {
                    task.WaitForExit(100000);

                    Console.WriteLine(task.StandardOutput.ReadToEnd());
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ExecuteProgram:" + e.ToString());
                return false;
            }
            finally
            {
                if (task != null && task.HasExited)
                {
                    rt = (task.ExitCode == 0);
                }
            }

            return rt;
        }
        private static string ProjectPath
        {
            get
            {
                string strTemp = Program.applicationPath;
                strTemp = strTemp.Replace("Assets", "");
                return strTemp;
            }
        }
        public static readonly string CSharpSuffix = "CSharp.proto";
        public static readonly string LUASuffix = "LUA.proto";
        static List<string> filesIgnore = new List<string>(new string[] {});
        static List<FileInfo> fileinfos = new List<FileInfo>();
        private static bool IsFileIgnore(string fileName)
        {
            foreach (var name in filesIgnore)
            {
                if (name == fileName)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// excludeSuffix defalut null mean nothing exclude
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="excludeSuffix"></param>
        /// <param name="searchPattern"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesExclude(DirectoryInfo dir, string excludeSuffix = null, string searchPattern = "*.proto", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            fileinfos.Clear();
            foreach (var file in dir.GetFiles(searchPattern, option))
            {
                var fileName = file.Name;
                bool not_add = false;
                if (!string.IsNullOrEmpty(excludeSuffix))//excludeSuffix IsNullOrEmpty then add all
                {
                    if (fileName.EndsWith(excludeSuffix))
                    {
                        not_add = true;
                    }
                }
                if (!not_add)
                {
                    not_add = IsFileIgnore(fileName);
                }
                if (!not_add)
                {
                    fileinfos.Add(file);
                }
            }
            return fileinfos.ToArray();
        }
        public static FileInfo[] GetFilesInclude(DirectoryInfo dir, string Suffix, string searchPattern = "*.proto", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            fileinfos.Clear();
            foreach (var file in dir.GetFiles(searchPattern, option))
            {
                var fileName = file.Name;
                bool add = false;
                if (!string.IsNullOrEmpty(Suffix))
                {
                    if (fileName.EndsWith(Suffix))
                    {
                        add = true;
                    }
                }
                else
                {
                    throw new Exception("Suffix IsNullOrEmpty");
                }
                if (add)//filter ignore file
                {
                    add = !IsFileIgnore(fileName);
                }
                if (add)
                {
                    fileinfos.Add(file);
                }
            }
            return fileinfos.ToArray();
        }

        public static void Generate()
        {
            //Gen CSharp
            string gen_exe_file = Path.GetFullPath(ProjectPath + "/../Tools/proto_gen_csharp/protogen.exe");
            Action<string, string> genCSLambdaFunc = (string protoPath, string csOutDir) =>
            {
                DirectoryInfo scanDir = new DirectoryInfo(protoPath);
                var files = GetFilesExclude(scanDir, LUASuffix);// scanDir.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                {
                    string proto_file = file.FullName.Replace(scanDir.FullName + Path.DirectorySeparatorChar, "");
                    //string pb_file = Path.GetFileNameWithoutExtension(proto_file) + ".cs";
                    ExecuteProgram(gen_exe_file, Path.GetFullPath(ProjectPath + "/../Tools/proto_gen_csharp"), string.Format(" {0} --proto_path={1} --csharp_out={2} --package=Magic.Cougar +langver=3 +names=original", proto_file, protoPath, csOutDir));
                }
            };
            genCSLambdaFunc(Path.GetFullPath(ProjectPath + "/../dep/server/proto"), Path.GetFullPath(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Msg"));
            genCSLambdaFunc(Path.GetFullPath(ProjectPath + "/../dep/server/EditorProto"), Path.GetFullPath(ProjectPath + "/../MagicEditor/Assets/Script/EditorBehaviours/ColliderExporter/Editor/Proto"));
            //Gen Lua PB
            string exe_file = Path.GetFullPath(ProjectPath + "/../Tools/lua_proto/protoc.exe");

            Action<DirectoryInfo, string, string, string> lambdaFunc = (DirectoryInfo scanDir, string pbOutDir, string luaOut, string luaNS) => {
                CProtoMgrLuaWriter pbWriter = new CProtoMgrLuaWriter(luaOut, luaNS);

                var files = GetFilesExclude(scanDir); //get all proto scanDir.GetFiles("*.proto", SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                {
                    string proto_file = file.FullName.Replace(scanDir.FullName + Path.DirectorySeparatorChar, "");
                    string pb_file = Path.GetFullPath(pbOutDir + "/" + Path.GetFileNameWithoutExtension(proto_file) + ".bytes");
                    pbWriter.AddPBFileName(Path.GetFileNameWithoutExtension(proto_file));
                    ExecuteProgram(exe_file, scanDir.FullName, string.Format("-o {0} {1}", pb_file, proto_file));
                }

                pbWriter.WriteLuaFile();
            };


            if (Directory.Exists(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config") == false)
                Directory.CreateDirectory(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config");

            if (Directory.Exists(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Gen") == false)
                Directory.CreateDirectory(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Gen");

            //Generate LP
            lambdaFunc(new DirectoryInfo(ProjectPath + "/../Dep/serverlp/proto"), ProjectPath + "/Assets/Resources/Config/pb/lp", 
            Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/ProtoLPPBConfig.lua"), "lp");

            //Generate BS
            lambdaFunc(new DirectoryInfo(ProjectPath + "/../Dep/server/proto"), ProjectPath + "/Assets/Resources/Config/pb/bs",
                Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/ProtoBSPBConfig.lua"), "bs");

            //Generate LP MsgType Config
            CProtoLPMsgTypeReader lpMsgTypeR = new CProtoLPMsgTypeReader(Path.GetFullPath(ProjectPath + "/../Dep/serverlp/proto/eprotomsg.h"), "lpb");
            lpMsgTypeR.ReadFile();

            CProtoLPMsgTypeLuaWriter lpMsgTypeW = new CProtoLPMsgTypeLuaWriter(lpMsgTypeR, 
                Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/LPMessageDefine.lua"),
                "lpb");
            lpMsgTypeW.WriteLuaFile();


            CProtoLPMsgTypeLuaMapWriter lpMsgTypeMapW = new CProtoLPMsgTypeLuaMapWriter(lpMsgTypeR,
                Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/LPMessageMap.lua"),
                "lpb");
            lpMsgTypeMapW.WriteLuaFile();

            //BS
            CProtoBSMsgTypeReader bsMsgReader = new CProtoBSMsgTypeReader(Path.GetFullPath(ProjectPath + "../Dep/server/proto/message.inl"), Path.GetFullPath(ProjectPath + "../Dep/server/proto"));
            bsMsgReader.ReadFile();
            bsMsgReader.AddCustomMessage(150, "MsgServerSnapshotBegin");
            bsMsgReader.AddCustomMessage(152, "MsgSnapshotBegin");
            bsMsgReader.AddCustomMessage(153, "MsgSnapshotEnd");

            bsMsgReader.AddCustomMessage(168, "MsgConnect");

            bsMsgReader.AddCustomMessage(10030, "MsgRtsmapBegin");
            bsMsgReader.AddCustomMessage(10037, "MsgRtsmapFrame");
            bsMsgReader.AddCustomMessage(10060, "MsgRtsmapEnd");

            CProtoBSMsgTypeLuaWriter bsMsgTypeLuaW = new CProtoBSMsgTypeLuaWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/BSMessageDefine.lua"));
            bsMsgTypeLuaW.WriteLuaFile();

            CProtoBSMsgTypeLuaMapWriter bsMsgTypeLuaMapW = new CProtoBSMsgTypeLuaMapWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Config/BSMessageMap.lua"));
            bsMsgTypeLuaMapW.WriteLuaFile();

            CProtoBSMsgTypeCSharpWriter bsMsgTypeCSharpW = new CProtoBSMsgTypeCSharpWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Gen/MessageDefine.cs"));
            bsMsgTypeCSharpW.WriteCSharpFile();

            CProtoBSMsg2ClsTypeWriter bsMsgType2CSharpW = new CProtoBSMsg2ClsTypeWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Gen/CSharpMessageTypeGen.cs"));
            bsMsgType2CSharpW.WriteCSharpFile();

            CProtoBSMsgClsDataResetWriter bsMsgClsDataResetW = new CProtoBSMsgClsDataResetWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/Projects/GameCougar/Implement/Gen/CSharpMessageResetGen.cs"));
            bsMsgClsDataResetW.WriteCSharpFile();

            CProtoBSMsgTypeLuaTableWriter bsMsgTypeLuaTableW = new CProtoBSMsgTypeLuaTableWriter(bsMsgReader, Path.GetFullPath(ProjectPath + "/../MagicGameCode/Projects/GameLogic/ProtoBufToLuaGen"));
            bsMsgTypeLuaTableW.WriteLuaTableFile();

            //ENUM BS
            CProtoMsgEnumLuaWriter wBSEnum = new CProtoMsgEnumLuaWriter(Path.GetFullPath(ProjectPath + "../Dep/server/proto"),
                Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Enum/BSEnumRequire.lua"), "bs");
            wBSEnum.WriteLuaFile();

            //ENUM LP
            CProtoMsgEnumLuaWriter wLPEnum = new CProtoMsgEnumLuaWriter(Path.GetFullPath(ProjectPath + "../Dep/serverlp/proto"),
                Path.GetFullPath(ProjectPath + "/../MagicGameCode/LuaScripts/Net/Enum/LPEnumRequire.lua"), "lp");
            wLPEnum.WriteLuaFile();
        }
    }
}
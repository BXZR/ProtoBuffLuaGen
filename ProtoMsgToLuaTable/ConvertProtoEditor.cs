using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
namespace Magic.GameEditor
{
    public class ConvertProtoEditor
    {

        //[MenuItem("[ProtoBuf]/Generate Code")]
        public static void Convert()
        {
            var toLuaGen = GetProtoGenFolder();
            var pbEnumGen = GetPbEnumFolder();
            InnerClear(toLuaGen, pbEnumGen);
            using (var converter = new ProtoConverter(toLuaGen, pbEnumGen))
            {
                converter.Convert();
            }
        }

        public static string GetProtoGenFolder()
        {
            var dirInfo = new DirectoryInfo(Program.applicationPath);
            return Path.Combine(dirInfo.Parent.Parent.FullName, "MagicGameCode/Projects/GameLogic/ProtoBufToLuaGen");
        }

        public static string GetPbEnumFolder()
        {
            return Path.Combine(Program.applicationPath, "Resources/LuaScripts/PbEnum");
        }

        //[MenuItem("[ProtoBuf]/Clear Generated Code")]
        public static void Clear()
        {
            var toLuaGen = GetProtoGenFolder();
            var pbEnumGen = GetPbEnumFolder();
            InnerClear(toLuaGen, pbEnumGen);
        }

        private static void InnerClear(string csDir, string luaDir)
        {
            if (Directory.Exists(csDir))
            {
                Directory.Delete(csDir, true);
            }
            if (Directory.Exists(luaDir))
            {
                Directory.Delete(luaDir, true);
            }
        }
    }

}
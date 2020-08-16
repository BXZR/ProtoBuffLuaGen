using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.GameEditor
{
    class Program
    {
        public static string applicationPath = @"D:\NDWork\projrct008_Unity2018_Android\project008\project008\MagicClient\Assets";
        static int Main(string[] args)
        {
            try
            {
                var arguments = CommandLineArgumentParser.Parse(args);
                if (arguments.Has("-rootpath"))
                {
                    applicationPath = arguments.Get("-rootpath").Next;
                    applicationPath = applicationPath.Replace('\\', '/');
                    Magic.GameEditor.GenLuaForProto.Generate();
                    return 0;
                }
                else
                {
                    Console.WriteLine("缺少重要参数：起始路径");
                    return 1;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("错误："+E.ToString());
                return 1;
            }
          
        }

    }
}

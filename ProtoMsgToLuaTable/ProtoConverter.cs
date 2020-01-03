using System;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using Google.Protobuf.Reflection;
namespace Magic.GameEditor
{

    public class ProtoConverter : IDisposable
    {

        private const string DEFAULT_CS_DIR = "ProtobufMsgGen";
        private const string DEFAULT_LUA_DIR = "PbEnum";

        private static HashSet<string> DLL_NAMES = new HashSet<string>();

        private bool m_isworking = false;
        private string m_csDir;
        private string m_luaDir;

        private string CSDir
        {
            get
            {
                return m_csDir;
            }
        }

        private string LUADir
        {
            get
            {
                return m_luaDir;
            }
        }

        static ProtoConverter()
        {
            DLL_NAMES.Add("Assembly-CSharp");
            DLL_NAMES.Add("GameCougar");
        }

        public ProtoConverter() { }

        public ProtoConverter(string csDir, string luaDir)
        {
            this.m_csDir = csDir;
            this.m_luaDir = luaDir;
        }

        public void Convert()
        {
            if (m_isworking)
            {
                return;
            }
            m_isworking = true;
            FilterClass<ProtoContractAttribute>(GotIt);
            m_isworking = false;
        }

        public void Convert(FileDescriptorSet lset)
        {
            if (m_isworking)
                return;

            m_isworking = true;
            FilterClass(GotIt,lset);
            m_isworking = false;
        }

        public void FilterClass<E>(Action<Type, E> got) where E : Attribute
        {
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblys == null)
            {
                return;
            }
            foreach (var assemblyItem in assemblys)
            {
                var assemblyName = assemblyItem.GetName().Name;
                Console.WriteLine(assemblyName);
                if (DLL_NAMES.Contains(assemblyName))
                {
                    var types = assemblyItem.GetTypes();
                    foreach (var typeItem in types)
                    {
                        var attrs = typeItem.GetCustomAttributes(typeof(E), false);
                        if (attrs == null || attrs.Length < 1)
                        {
                            continue;
                        }
                        got(typeItem, attrs[0] as E);
                    }
                }
            }
        }

        public void FilterClass(Action<DescriptorProto, string> gotit , FileDescriptorSet lset)
        {
            //foreach (var protoDesc in CProtoBSMsgTypeReader.Messages)
            //{
            //    using (var converter = new ProtoClassToLuaTableConverter3(protoDesc.Value, protoDesc.Key, CSDir))
            //    {
            //        converter.Convert();
            //    }
            //}
            foreach (FileDescriptorProto fileDesc in lset.Files)
            {
                foreach (DescriptorProto protoDesc in fileDesc.MessageTypes)
                {
                    gotit(protoDesc, fileDesc.Package);
                }
            }
        }

        public void GotIt(Type gotType, ProtoContractAttribute gotAttr)
        {
            if (gotType.IsEnum)
            {
                using (var converter = new ProtoEnumToLuaEnumConverter(gotType, LUADir))
                {
                    converter.Convert();
                }
            }
            else
            {
                using (var converter = new ProtoClassToLuaTableConverter2(gotType, CSDir))
                {
                    converter.Convert();
                }
            }
        }

        public void GotIt(DescriptorProto gotProto, string ns)
        {
            foreach (DescriptorProto innerProto in gotProto.NestedTypes)
            {
                GotIt(innerProto, string.IsNullOrEmpty(ns) ? "Magic.Cougar." + gotProto.Name : ns + "." + gotProto.Name);
                //GotIt(innerProto, ns);
            }
            using (var converter = new ProtoClassToLuaTableConverter3(gotProto, ns, CSDir))
            {
                converter.Convert();
            }
        }

        public void Dispose()
        {
        }
    }


}

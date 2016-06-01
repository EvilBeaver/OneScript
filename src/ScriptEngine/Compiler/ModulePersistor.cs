/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace ScriptEngine.Compiler
{
    public class ModulePersistor
    {
        IFormatter _formatter;

        private struct ModuleHeader
        {
            public int NameLen;
            public long BodyLen;
            public bool IsClass;
        }

        public static void WriteStruct<T>(Stream s, T value)
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();
            s.Write(rawdata, 0, rawsize);
        }

        public static T ReadStruct<T>(Stream s)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            s.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

            GCHandle handle;
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return temp;

        }

        public ModulePersistor (IFormatter format)
	    {
            _formatter = format;
	    }

        public void Save(UserAddedScript script, Stream output)
        {
            var nameBytes = Encoding.UTF8.GetBytes(script.Symbol);
            var bodyStream = new MemoryStream(8*1024);
            _formatter.Serialize(bodyStream, FromHandle(script.Module));
            
            var header = new ModuleHeader()
            {
                NameLen = nameBytes.Length,
                BodyLen = bodyStream.Length,
                IsClass = (script.Type == UserAddedScriptType.Class)
            };

            WriteStruct(output, header);

            output.Write(nameBytes, 0, nameBytes.Length);
            bodyStream.Position = 0;
            bodyStream.WriteTo(output);
            bodyStream.Dispose();
        }

        private ModuleImage FromHandle(ScriptModuleHandle module)
        {
            return module.Module;
        }

        public UserAddedScript Read(Stream input)
        {
            var header = ReadStruct<ModuleHeader>(input);

            var userScript = new UserAddedScript();

            var nameBytes = new byte[header.NameLen];
            input.Read(nameBytes, 0, nameBytes.Length);

            userScript.Symbol = Encoding.UTF8.GetString(nameBytes);

            var moduleImage = (ModuleImage)_formatter.Deserialize(input);

            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            moduleImage.ModuleInfo = new ModuleInformation()
            {
                CodeIndexer = new CompiledCodeIndexer(),
                ModuleName = System.IO.Path.GetFileName(path),
                Origin = path
            };

            userScript.Module = new ScriptModuleHandle() { Module = moduleImage };
            userScript.Type = header.IsClass ? UserAddedScriptType.Class : UserAddedScriptType.Module;

            return userScript;
        }

    }
}

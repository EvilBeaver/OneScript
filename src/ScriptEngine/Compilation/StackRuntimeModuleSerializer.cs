/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using MessagePack;
using OneScript.Compilation;
using OneScript.Execution;
using ScriptEngine.Machine;

namespace ScriptEngine.Compilation
{
    /// <summary>
    /// Сериализатор модулей на основе StackRuntimeModule
    /// </summary>
    public class StackRuntimeModuleSerializer : IModuleSerializer
    {
        public void Serialize(IExecutableModule module, Stream stream)
        {
            var serializableModule = SerializableModule.FromExecutableModule(module);
            MessagePackSerializer.Serialize(stream, serializableModule);
        }

        public IExecutableModule Deserialize(Stream stream)
        {
            var serializableModule = MessagePackSerializer.Deserialize<SerializableModule>(stream);
            return serializableModule.ToExecutableModule();
        }

        public bool CanSerialize(IExecutableModule module)
        {
            return module is StackRuntimeModule;
        }
    }
}
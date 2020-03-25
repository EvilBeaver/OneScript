/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OneScriptDocumenter
{
    class LoadedAssembly
    {
        private readonly Assembly _library;
        private readonly AssemblyLoader _assemblyLoader;

        private Type[] _allTypes;

        public LoadedAssembly(Assembly library, AssemblyLoader assemblyLoader)
        {
            _library = library;
            _assemblyLoader = assemblyLoader;
        }

        public string Name
        {
            get
            {
                return _library.GetName().Name;
            }
        }

        public Type[] AllTypes
        {
            get
            {
                if (_allTypes == null)
                    _allTypes = _library.GetTypes();

                return _allTypes;
            }
        }

        public Type[] GetMarkedTypes(ScriptMemberType markupElement)
        {
            var attributeType = _assemblyLoader.MemberTypeToAttributeType(markupElement);
            var types = new List<Type>();
            
            foreach (var t in AllTypes)
            {
                try
                {
                    var attr = t.GetCustomAttributesData();
                    foreach (var currentAttr in attr)
                    {
                        if (currentAttr.AttributeType == attributeType)
                            types.Add(t);
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Skipping type {t} due to load error");
                }
            }
            
            return types.ToArray();
        }

        public CustomAttributeData GetMarkup(MemberInfo member, ScriptMemberType markupElement)
        {
            var type = _assemblyLoader.MemberTypeToAttributeType(markupElement);
            return member.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType == type);
        }

        public CustomAttributeData GetMarkup(Type type, ScriptMemberType markupElement)
        {
            var attributeType = _assemblyLoader.MemberTypeToAttributeType(markupElement);
            var result = type.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType == attributeType);
            return result;
        }

    }
}

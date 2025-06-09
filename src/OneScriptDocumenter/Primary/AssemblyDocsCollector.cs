/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using OneScript.Contexts;
using OneScript.Contexts.Enums;

namespace OneScriptDocumenter.Primary
{
    public class AssemblyDocsCollector
    {
        private readonly Assembly _assembly;
        private XmlDocHolder _xmlDocs;

        public AssemblyDocsCollector(Assembly assembly)
        {
            _assembly = assembly;
        }

        public bool CollectDocumentation(PrimaryDocumentation destination)
        {
            var assemblyDir = Path.GetDirectoryName(_assembly.Location);
            Trace.Assert(assemblyDir != null);

            var assemblyName = _assembly.GetName().Name;
            var xmlDoc = Path.Combine(assemblyDir, assemblyName + ".xml");

            bool docsExist = File.Exists(xmlDoc); 
            _xmlDocs = new XmlDocHolder(assemblyName);
            if (docsExist)
            {
                try
                {
                    _xmlDocs.Read(xmlDoc);
                }
                catch (Exception e)
                {
                    docsExist = false;
                    ConsoleLogger.Warning(e.ToString());
                }
            }
            
            foreach (var type in _assembly.GetTypes())
            {
                foreach (var currentAttr in type.GetCustomAttributes(false))
                {
                    if (currentAttr is GlobalContextAttribute glob)
                    {
                        AddType(destination, type, glob);
                    }
                    else if (currentAttr is ContextClassAttribute classAttribute)
                    {
                        AddType(destination, type, classAttribute);
                    }
                    else if (currentAttr is EnumerationTypeAttribute simpleEnum)
                    {
                        AddType(destination, type, simpleEnum);
                    }
                    else if (currentAttr is SystemEnumAttribute systemEnum)
                    {
                        AddType(destination, type, systemEnum);
                    }
                    else if (currentAttr is DocumentationProviderAttribute docAttr)
                    {
                        AddType(destination, type, docAttr);
                    }
                }
            }

            return docsExist;
        }

        private void AddType(PrimaryDocumentation documentation, Type type,
            DocumentationProviderAttribute classAttribute)
        {
            var doc = new PrimaryBslDocument();
            doc.Owner = type;
            doc.OwnerKind = OwnerKind.DocumentationProvider;
            doc.Title = classAttribute.Category;
            doc.XmlDocIdentifier = TypeKey(type);
            doc.SelfDoc = _xmlDocs[doc.XmlDocIdentifier];

            AddProperties(doc.Properties, type, documentation.ReferenceCollector);
            AddMethods(doc.Methods, type, documentation.ReferenceCollector);
            
            documentation.ReferenceCollector.Register(doc.XmlDocIdentifier, type);
            documentation.Add(doc);
        }

        private void AddType(PrimaryDocumentation documentation, Type type, SystemEnumAttribute classAttribute)
        {
            var doc = new PrimaryBslDocument();
            doc.Owner = type;
            doc.OwnerKind = OwnerKind.SystemEnum;
            doc.Title = classAttribute.Name;
            doc.XmlDocIdentifier = TypeKey(type);
            doc.SelfDoc = _xmlDocs[doc.XmlDocIdentifier];

            documentation.ReferenceCollector.Register(doc.XmlDocIdentifier, type);
            documentation.Add(doc);
        }

        private void AddType(PrimaryDocumentation documentation, Type type, EnumerationTypeAttribute enumAttribute)
        {
            var doc = new PrimaryBslDocument();
            doc.Owner = type;
            doc.OwnerKind = OwnerKind.SimpleEnum;
            doc.Title = enumAttribute.Name;
            doc.XmlDocIdentifier = TypeKey(type);
            doc.SelfDoc = _xmlDocs[doc.XmlDocIdentifier];

            foreach (var fieldInfo in type.GetFields().Where(f => f.GetCustomAttributes(typeof(EnumValueAttribute), false).Any()))
            {
                var fieldKey = FieldKey(fieldInfo);
                var xDoc = _xmlDocs[fieldKey];
                doc.Fields.Add(fieldInfo, xDoc);
                documentation.ReferenceCollector.Register(fieldKey, fieldInfo);
            }
            
            documentation.ReferenceCollector.Register(doc.XmlDocIdentifier, type);
            documentation.Add(doc);
        }

        private void AddType(PrimaryDocumentation documentation, Type type, ContextClassAttribute classAttribute)
        {
            var doc = new PrimaryBslDocument();
            doc.Owner = type;
            doc.OwnerKind = OwnerKind.Class;
            doc.Title = classAttribute.Name;
            doc.XmlDocIdentifier = TypeKey(type);
            doc.SelfDoc = _xmlDocs[doc.XmlDocIdentifier];

            AddProperties(doc.Properties, type, documentation.ReferenceCollector);
            AddMethods(doc.Methods, type, documentation.ReferenceCollector);
            AddConstructors(doc.Constructors, type, documentation.ReferenceCollector);
            
            documentation.ReferenceCollector.Register(doc.XmlDocIdentifier, type);
            documentation.Add(doc);
        }

        private void AddType(PrimaryDocumentation documentation, Type type, GlobalContextAttribute glob)
        {
            var doc = new PrimaryBslDocument();
            doc.Owner = type;
            doc.OwnerKind = OwnerKind.GlobalContext;
            doc.Title = glob.Category ?? type.Name;
            doc.XmlDocIdentifier = TypeKey(type);
            doc.SelfDoc = _xmlDocs[doc.XmlDocIdentifier];

            AddProperties(doc.Properties, type, documentation.ReferenceCollector);
            AddMethods(doc.Methods, type, documentation.ReferenceCollector);

            documentation.ReferenceCollector.Register(doc.XmlDocIdentifier, type);
            documentation.Add(doc);
        }

        private void AddProperties(Dictionary<PropertyInfo, XElement> propDocuments, Type type,
            ReferenceCollector documentationReferenceCollector)
        {
            foreach (var propertyInfo in type.GetProperties()
                         .Where(p => p
                             .GetCustomAttributes(false)
                             .Any(attr => attr is ContextPropertyAttribute { SkipForDocumenter: false } || attr is DocumentedMemberAttribute)))
            {
                var propertyKey = PropertyKey(propertyInfo);
                var doc = _xmlDocs[propertyKey];
                propDocuments.Add(propertyInfo, doc);
                documentationReferenceCollector.Register(propertyKey, propertyInfo);
            }
        }

        private void AddMethods(Dictionary<MethodInfo, XElement> methDocuments, Type type,
            ReferenceCollector documentationReferenceCollector)
        {
            foreach (var methodInfo in type
                         .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                         .Where(m => m.GetCustomAttributes(false)
                             .Any(attr => attr is ContextMethodAttribute { SkipForDocumenter: false } || attr is DocumentedMemberAttribute)))
            {
                var methodKey = MethodKey(methodInfo);
                var doc = _xmlDocs[methodKey];
                methDocuments.Add(methodInfo, doc);
                documentationReferenceCollector.Register(methodKey, methodInfo);
            }
        }
        
        private void AddConstructors(Dictionary<MethodInfo, XElement> docConstructors, Type type,
            ReferenceCollector documentationReferenceCollector)
        {
            foreach (var methodInfo in type
                         .GetMethods(BindingFlags.Static | BindingFlags.Public)
                         .Where(m => m.GetCustomAttributes(typeof(ScriptConstructorAttribute), false).Any()))
            {
                var methodKey = MethodKey(methodInfo);
                var doc = _xmlDocs[methodKey];
                docConstructors.Add(methodInfo, doc);
                documentationReferenceCollector.Register(methodKey, methodInfo);
            }
        }

        private static string TypeKey(Type type) => $"T:{type.FullName}";

        private static string MethodKey(MethodBase method) => $"M:{method.DeclaringType!.FullName}.{MethodId(method)}";
         
        private static string PropertyKey(PropertyInfo prop) => $"P:{prop.DeclaringType!.FullName}.{prop.Name}";
         
        private static string FieldKey(FieldInfo field) => $"F:{field.DeclaringType!.FullName}.{field.Name}";
        
        private static string MethodId(MethodBase meth)
        {
            var sb = new StringBuilder();
            sb.Append(meth.Name);
            var methParams = meth.GetParameters();
            if (methParams.Length > 0)
            {
                sb.Append('(');
                var paramInfos = methParams.Select(x => x.ParameterType).ToArray();
                string[] paramTypeNames = new string[paramInfos.Length];

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    var info = paramInfos[i];
                    if (info.GenericTypeArguments.Length > 0)
                    {
                        var genericBuilder = BuildStringGenericTypes(info);

                        paramTypeNames[i] = genericBuilder.ToString();
                    }
                    else
                    {
                        paramTypeNames[i] = info.FullName;
                    }
                }
                sb.Append(string.Join(",", paramTypeNames));
                sb.Append(')');
            }
            return sb.ToString();
        }
        
        private static StringBuilder BuildStringGenericTypes(Type info)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(info.FullName, @"([\w.]+)`\d|(\[([\w0-9.=]+)(?:,\s(?:[\w0-9.= ]+))*\]),?");

            var genericBuilder = new StringBuilder();

            if (matches.Count == 1)
            {
                return genericBuilder;
            }
            
            genericBuilder.Append(matches[0].Groups[1]);
            genericBuilder.Append('{');
            var fst = true;
            foreach (var capture in matches[1].Groups[3].Captures)
            {
                if (!fst)
                    genericBuilder.Append(", ");

                genericBuilder.Append(capture);
                fst = false;
            }
            genericBuilder.Append('}');

            return genericBuilder;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OneScriptDocumenter
{
    class AssemblyDocumenter
    {
        readonly LoadedAssembly _library;
        readonly XDocument _xmlDoc;
        readonly Dictionary<string, XElement> _memberDocumentation = new Dictionary<string, XElement>();

        readonly TypesDictionary _typesDict;

        public AssemblyDocumenter()
        { }

        public AssemblyDocumenter(string library)
        {
            var dir = Path.GetDirectoryName(library);

            var loader = new AssemblyLoader(dir);

            _library = loader.Load(Path.GetFileName(library));

            // add to dictinary

            _typesDict = new TypesDictionary();

        }

        public void FillTypesDictionary()
        {

            ScriptMemberType[] contexts = { ScriptMemberType.GlobalContext, ScriptMemberType.Class, ScriptMemberType.SystemEnum, ScriptMemberType.EnumerationType };
            foreach (ScriptMemberType context in contexts)
            {
                var conts = _library.GetMarkedTypes(context);
                foreach (var cont in conts)

                {
                    var attrib = _library.GetMarkup(cont, context);

                    if (attrib == null)
                    {
                        continue;
                    }

                    TypeInfo curTypeInfo = new TypeInfo();
                    curTypeInfo.fullName = cont.FullName;
                    curTypeInfo.ShortName = cont.Name;
                    if (attrib.ConstructorArguments.Count > 0)
                    {
                        GetNameAndAlias(attrib, cont.Name, out curTypeInfo.nameRus, out curTypeInfo.nameEng);
                    }
                    _typesDict.add(curTypeInfo);
                }
            }

            _typesDict.save();
        }

        private static void GetNameAndAlias(CustomAttributeData attrib, string clrName, out string name, out string alias)
        {
            name = (string)attrib.ConstructorArguments[0].Value;
            alias = (string)attrib.ConstructorArguments[1].Value;
            if (string.IsNullOrEmpty(alias))
            {
                alias = clrName;
            }
        }

        public AssemblyDocumenter(string library, string xmldoc)
        {
            using (var reader = new StreamReader(xmldoc))
            {
                _xmlDoc = XDocument.Load(reader);
            }

            var dir = Path.GetDirectoryName(library);

            var loader = new AssemblyLoader(dir);

            _library = loader.Load(Path.GetFileName(library));

            // add to dictinary

            _typesDict = new TypesDictionary();

            FillTypesDictionary();

        }

        public string SetRusNames(string data, bool link = true)
        {

            string linkStr = (link) ? "link::" : "";

            string str = data;

            str = str.Replace("System.String", "Строка");
            str = str.Replace("System.DateTime", "Дата");
            str = str.Replace("System.Int32", "Число");
            str = str.Replace("System.Int64", "Число");
            str = str.Replace("System.Decimal", "Число");
            str = str.Replace("System.UInt32", "Число");
            str = str.Replace("System.UInt64", "Число");
            str = str.Replace("System.Boolean", "Булево");
            str = str.Replace("System.Nullable{", "");
            str = str.Replace("ScriptEngine.Machine.Contexts.SelfAwareEnumValue{", "");
            str = str.Replace("ScriptEngine.Machine.IVariable", "Произвольный");
            str = str.Replace("}", "");
            str = str.Replace("ScriptEngine.Machine.IValue", "Произвольный");
            str = str.Replace("ScriptEngine.Machine.IRuntimeContextInstance", "ИнформацияОСценарии");

            foreach (TypeInfo curItm in _typesDict.types)
            {
                Regex regex = new Regex(@"(\b)(" + curItm.fullName + @")(\b)", RegexOptions.IgnoreCase);
                str = regex.Replace(str, linkStr + curItm.nameRus);
            }

            return str;
        }

        public XDocument CreateDocumentation()
        {
            var asmElement = _xmlDoc.Root.Element("assembly");
            if (asmElement == null)
                throw new ArgumentException("Wrong XML doc format");

            var libName = _library.Name;
            var fileLibName = asmElement.Element("name").Value;
            if (String.Compare(libName, fileLibName, true, System.Globalization.CultureInfo.InvariantCulture) != 0)
                throw new ArgumentNullException(String.Format("Mismatch assembly names. Expected {0}, found in XML {1}", libName, fileLibName));

            var members = _xmlDoc.Element("doc").Element("members").Elements();
            _memberDocumentation.Clear();
            foreach (var item in members)
            {
                string key = item.Attribute("name").Value;
                _memberDocumentation[key] = item;
            }

            XDocument output = BeginOutputDoc();

            var globalContexts = _library.GetMarkedTypes(ScriptMemberType.GlobalContext);
            foreach (var globalContext in globalContexts)
            {
                AddGlobalContextDescription(globalContext, output.Root);
            }

            var contextTypes = _library.GetMarkedTypes(ScriptMemberType.Class);
            foreach (var classType in contextTypes)
            {
                AddContextDescription(classType, output.Root);
            }

            return output;
        }

        public void CreateDocumentationJSON(DocumentBlocks textBlocks)
        {

            var asmElement = _xmlDoc.Root.Element("assembly");
            if (asmElement == null)
                throw new ArgumentException("Wrong XML doc format");

            var libName = _library.Name;
            var fileLibName = asmElement.Element("name").Value;
            if (String.Compare(libName, fileLibName, true, System.Globalization.CultureInfo.InvariantCulture) != 0)
                throw new ArgumentNullException(String.Format("Mismatch assembly names. Expected {0}, found in XML {1}", libName, fileLibName));

            var members = _xmlDoc.Element("doc").Element("members").Elements();
            _memberDocumentation.Clear();
            foreach (var item in members)
            {
                string key = item.Attribute("name").Value;
                _memberDocumentation[key] = item;
            }

            var globalContexts = _library.GetMarkedTypes(ScriptMemberType.GlobalContext);
            foreach (var globalContext in globalContexts)
            {
                AddGlobalContextDescriptionJSON(globalContext, textBlocks);
            }
            if (_library.Name == "ScriptEngine.HostedScript")
            {
                using (var layout = new StreamReader(ExtFiles.Get("BasicMethods.json")))
                {
                    var content = layout.ReadToEnd().Trim();
                    textBlocks.TextGlobalContext.Append(content.Substring(1, content.Length - 2));
                }
            }

            var contextTypes = _library.GetMarkedTypes(ScriptMemberType.Class);
            foreach (var classType in contextTypes)
            {
                AddContextDescriptionJSON(classType, textBlocks);
            }

            var systemEnums = _library.GetMarkedTypes(ScriptMemberType.SystemEnum);
            foreach (var systemEnum in systemEnums)
            {
                AddEnumsDescriptionJSON(systemEnum, textBlocks, ScriptMemberType.SystemEnum);
            }
            var enums = _library.GetMarkedTypes(ScriptMemberType.EnumerationType);
            foreach (var sysEnum in enums)
            {
                AddEnumsDescriptionJSON(sysEnum, textBlocks, ScriptMemberType.EnumerationType);
            }


        }

        private void AddEnumsDescriptionJSON(Type sysEnum, DocumentBlocks textBlocks, ScriptMemberType sysType)
        {
            var attrib = _library.GetMarkup(sysEnum, sysType);

            string name, alias;
            GetNameAndAlias(attrib, sysEnum.Name, out name, out alias);

            var childElement = new XElement(name);
            childElement.Add(new XElement("name", name));
            childElement.Add(new XElement("name_en", alias));

            AppendXmlDocsJSON(childElement, "T:" + sysEnum.FullName);

            AddValues(sysEnum, childElement);

            var JSONNode = JSon.XmlToJSON(childElement.ToString());

            textBlocks.TextEnumsDescription.Append(JSONNode.Substring(1, JSONNode.Length - 2) + ",");
        }

        private void AddGlobalContextDescription(Type globalContext, XContainer xElement)
        {

            var childElement = new XElement("global-context");

            childElement.Add(new XAttribute("clr-name", globalContext.FullName));

            var attrib = _library.GetMarkup(globalContext, ScriptMemberType.GlobalContext);
            if (attrib == null)
                return;

            string categoryName = null;

            try
            {
                if (attrib.NamedArguments != null)
                {
                    var categoryMember = attrib.NamedArguments.First(x => x.MemberName == "Category");
                    categoryName = (string)categoryMember.TypedValue.Value;
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }

            if (categoryName != null)
                childElement.Add(new XElement("category", categoryName));

            AppendXmlDocs(childElement, "T:" + globalContext.FullName);

            AddProperties(globalContext, childElement);
            AddMethods(globalContext, childElement);

            xElement.Add(childElement);
        }

        private void AddContextDescription(Type classType, XContainer xElement)
        {
            var childElement = new XElement("context");

            childElement.Add(new XAttribute("clr-name", classType.FullName));
            var attrib = _library.GetMarkup(classType, ScriptMemberType.Class);

            string name, alias;
            GetNameAndAlias(attrib, classType.Name, out name, out alias);

            childElement.Add(new XElement("name", name));
            childElement.Add(new XElement("alias", alias));

            AppendXmlDocs(childElement, "T:" + classType.FullName);

            AddProperties(classType, childElement);
            AddMethods(classType, childElement);
            AddConstructors(classType, childElement);

            xElement.Add(childElement);
        }

        private void AddGlobalContextDescriptionJSON(Type globalContext, DocumentBlocks textBlocks)
        {

            var attrib = _library.GetMarkup(globalContext, ScriptMemberType.GlobalContext);
            if (attrib == null)
                return;

            string categoryName = null;

            try
            {
                if (attrib.NamedArguments != null)
                {
                    var categoryMember = attrib.NamedArguments.First(x => x.MemberName == "Category");
                    categoryName = (string)categoryMember.TypedValue.Value;
                    var childElement = new XElement(categoryName.Replace(" ", "_"));

                    AppendXmlDocsJSON(childElement, "T:" + globalContext.FullName);

                    AddProperties(globalContext, childElement, "JSON");
                    AddMethodsJSON(globalContext, childElement);

                    if (!childElement.IsEmpty)
                    {
                        var JSONNode = JSon.XmlToJSON(childElement.ToString());
                        var separatorPos = JSONNode.IndexOf(":", StringComparison.InvariantCulture);
                        JSONNode = JSONNode.Substring(0, separatorPos).Replace("_", " ") + JSONNode.Substring(separatorPos);
                        textBlocks.TextGlobalContext.Append(JSONNode.Substring(1, JSONNode.Length - 2) + ",");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void AddContextDescriptionJSON(Type classType, DocumentBlocks textBlocks)
        {
            var attrib = _library.GetMarkup(classType, ScriptMemberType.Class);

            string name, alias;
            GetNameAndAlias(attrib, classType.Name, out name, out alias);

            var childElement = new XElement(name);
            childElement.Add(new XElement("name", name));
            childElement.Add(new XElement("name_en", alias));

            AppendXmlDocsJSON(childElement, "T:" + classType.FullName);

            AddProperties(classType, childElement, "JSON");
            AddMethodsJSON(classType, childElement);
            AddConstructorsJSON(classType, childElement);

            var JSONNode = JSon.XmlToJSON(childElement.ToString());
            var separatorPos = JSONNode.IndexOf("\"constructors\":", StringComparison.InvariantCulture);
            if (separatorPos > 0)
                JSONNode = JSONNode.Substring(0, separatorPos) + JSONNode.Substring(separatorPos).Replace("_", " ");

            textBlocks.TextContextDescription.Append(JSONNode.Substring(1, JSONNode.Length - 2) + ",");
        }

        private void AddMethods(Type classType, XContainer childElement)
        {
            var collection = new XElement("methods");
            var methodArray = classType.GetMethods();
            foreach (var meth in methodArray)
            {
                var attrib = _library.GetMarkup(meth, ScriptMemberType.Method);
                if (attrib != null)
                {
                    var fullName = classType.FullName + "." + MethodId(meth);
                    string name, alias;
                    GetNameAndAlias(attrib, meth.Name, out name, out alias);
                    var element = new XElement("method");
                    element.Add(new XAttribute("clr-name", fullName));
                    element.Add(new XElement("name", name));
                    element.Add(new XElement("alias", alias));

                    AppendXmlDocs(element, "M:" + fullName);

                    collection.Add(element);
                }
            }

            childElement.Add(collection);
        }

        private void AddMethodsJSON(Type classType, XContainer childElement)
        {
            var collection = new XElement("methods");
            var methodArray = classType.GetMethods();
            foreach (var meth in methodArray)
            {
                var attrib = _library.GetMarkup(meth, ScriptMemberType.Method);
                if (attrib != null)
                {
                    var fullName = classType.FullName + "." + MethodId(meth);
                    string name, alias;
                    GetNameAndAlias(attrib, meth.Name, out name, out alias);
                    var element = new XElement(name);
                    element.Add(new XElement("name", name));
                    element.Add(new XElement("name_en", alias));
                    var returns = "";
                    if (meth.ReturnType.FullName != "System.Void")
                    {
                        returns = SetRusNames(meth.ReturnType.FullName, false);
                        if (returns != "") returns = ": " + returns;
                    }
                    element.Add(new XElement("signature", "(" + MethodIdJSON(meth, false) + ")" + returns));
                    AppendXmlDocsJSON(element, "M:" + fullName);

                    collection.Add(element);
                }
            }

            if (!collection.IsEmpty)
                childElement.Add(collection);
        }

        private string MethodId(MethodBase meth)
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

        private StringBuilder BuildStringGenericTypes(Type info)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(info.FullName, @"([\w.]+)`\d|(\[([\w0-9.=]+)(?:,\s(?:[\w0-9.= ]+))*\]),?");

            var genericBuilder = new StringBuilder();

            if (matches.Count == 1)
            {
                return genericBuilder;
            }
            
            genericBuilder.Append(matches[0].Groups[1].ToString());
            genericBuilder.Append('{');
            bool fst = true;
            foreach (var capture in matches[1].Groups[3].Captures)
            {
                if (!fst)
                    genericBuilder.Append(", ");

                genericBuilder.Append(capture.ToString());
                fst = false;
            }
            genericBuilder.Append('}');

            return genericBuilder;

        }

        private string MethodIdJSON(MethodInfo meth, bool addLink = true)
        {
            var sb = new StringBuilder();
            var methParams = meth.GetParameters();
            if (methParams.Length > 0)
            {
                var paramInfos = methParams.Select(x => x.ParameterType).ToArray();
                string[] paramTypeNames = new string[paramInfos.Length];

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    var info = paramInfos[i];
                    var MethcodArg = "";
                    if (info.GenericTypeArguments.Length > 0)
                    {
                        var genericBuilder = BuildStringGenericTypes(info);
                        MethcodArg = genericBuilder.ToString();
                    }
                    else
                    {
                        MethcodArg = info.FullName;
                    }

                    MethcodArg = SetRusNames(MethcodArg, addLink);
                    var Optional = (methParams[i].IsOptional) ? "?" : "";
                    paramTypeNames[i] = methParams[i].Name + Optional + ": " + MethcodArg;
                }
                sb.Append(string.Join(", ", paramTypeNames));
            }
            return sb.ToString();
        }

        private void AddValues(Type classType, XContainer childElement)
        {
            var propElementCollection = new XElement("values");

            var propArray = classType.GetProperties();
            foreach (var prop in propArray)
            {
                var attrib = _library.GetMarkup(prop, ScriptMemberType.EnumerationValue);
                if (attrib != null)
                {
                    string name, alias;
                    GetNameAndAlias(attrib, prop.Name, out name, out alias);

                    var propElement = new XElement(name);
                    propElement.Add(new XElement("name", name));
                    propElement.Add(new XElement("name_en", alias));

                    AppendXmlDocsJSON(propElement, "P:" + classType.FullName + "." + prop.Name);
                    propElementCollection.Add(propElement);
                }
            }
            var fieldsArray = classType.GetFields();
            foreach (var field in fieldsArray)
            {
                var attrib = _library.GetMarkup(field, ScriptMemberType.EnumItem);
                if (attrib != null)
                {
                    string name, alias;
                    GetNameAndAlias(attrib, field.Name, out name, out alias);

                    var propElement = new XElement(name);
                    propElement.Add(new XElement("name", name));
                    propElement.Add(new XElement("name_en", alias));

                    AppendXmlDocsJSON(propElement, "P:" + classType.FullName + "." + field.Name);
                    propElementCollection.Add(propElement);
                }
            }

            if (!propElementCollection.IsEmpty)
                childElement.Add(propElementCollection);
        }

        private void AddProperties(Type classType, XContainer childElement, string mode = "")
        {
            var propElementCollection = new XElement("properties");

            var propArray = classType.GetProperties();
            foreach (var prop in propArray)
            {
                var attrib = _library.GetMarkup(prop, ScriptMemberType.Property);
                if (attrib != null)
                {
                    XElement propElement;
                    if (mode == "JSON")
                    {
                        propElement = fillPropElementJSON(prop, attrib, classType);
                    }
                    else
                    {
                        propElement = fillPropElement(prop, attrib, classType);
                    }
                    propElementCollection.Add(propElement);
                }
            }
            if (!propElementCollection.IsEmpty)
                childElement.Add(propElementCollection);
        }

        private XElement fillPropElement(System.Reflection.PropertyInfo prop, System.Reflection.CustomAttributeData attrib, Type classType)
        {
            var propElement = new XElement("property");
            string name, alias;
            GetNameAndAlias(attrib, prop.Name, out name, out alias);
            propElement.Add(new XAttribute("clr-name", classType.FullName + "." + prop.Name));
            propElement.Add(new XElement("name", name));
            propElement.Add(new XElement("alias", alias));

            var access = findAccess(attrib, prop);

            propElement.Add(new XElement("readable", access["canRead"]));
            propElement.Add(new XElement("writeable", access["canWrite"]));

            AppendXmlDocs(propElement, "P:" + classType.FullName + "." + prop.Name);
            return propElement;

        }

        private Dictionary<string, bool?> findAccess(System.Reflection.CustomAttributeData attrib, System.Reflection.PropertyInfo prop)
        {
            bool? canRead = null;
            bool? canWrite = null;

            if (attrib.NamedArguments != null)
            {
                foreach (var attributeNamedArgument in attrib.NamedArguments)
                {
                    if (attributeNamedArgument.MemberName == "CanRead")
                    {
                        canRead = (bool)attributeNamedArgument.TypedValue.Value;
                    }

                    if (attributeNamedArgument.MemberName == "CanWrite")
                    {
                        canWrite = (bool)attributeNamedArgument.TypedValue.Value;
                    }
                }
            }
            if (canRead == null)
                canRead = prop.GetMethod != null;

            if (canWrite == null)
                canWrite = prop.SetMethod != null;

            var result = new Dictionary<string, bool?>();
            result.Add("canRead", canRead);
            result.Add("canWrite", canWrite);

            return result;

        }

        private XElement fillPropElementJSON(System.Reflection.PropertyInfo prop, System.Reflection.CustomAttributeData attrib, Type classType)
        {
            string name, alias;
            GetNameAndAlias(attrib, prop.Name, out name, out alias);
            var propElement = new XElement(name);
            propElement.Add(new XElement("name", name));
            propElement.Add(new XElement("name_en", alias));

            var access = findAccess(attrib, prop);

            AppendXmlDocsJSON(propElement, "P:" + classType.FullName + "." + prop.Name);
            buildAccessProperty(access["canRead"], access["canWrite"], propElement);
            return propElement;
        }

        private void buildAccessProperty(bool? canRead, bool? canWrite, XContainer propElement)
        {
            var access = ((bool)canRead && (bool)canWrite) ? "Чтение/Запись" : (bool)canRead ? "Чтение" : "Запись";
            propElement.Add(new XElement("access", access));
        }

        private void AddConstructors(Type classType, XContainer childElement)
        {

            int itemsCount = 0;
            var collection = new XElement("constructors");
            var methodArray = classType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            foreach (var meth in methodArray)
            {
                var attrib = _library.GetMarkup(meth, ScriptMemberType.Constructor);
                if (attrib != null)
                {
                    var fullName = classType.FullName + "." + MethodId(meth);
                    var element = new XElement("ctor");
                    element.Add(new XAttribute("clr-name", fullName));

                    var namedArgsName = attrib.NamedArguments.FirstOrDefault(x => x.MemberName == "Name");
                    if (namedArgsName.MemberInfo == null)
                    {
                        element.Add(new XElement("name", "По умолчанию"));
                    }
                    else
                    {
                        var ctorName = (string)namedArgsName.TypedValue.Value;
                        if (ctorName == "")
                            continue;
                        element.Add(new XElement("name", ctorName));
                    }

                    AppendXmlDocs(element, "M:" + fullName);
                    collection.Add(element);
                    itemsCount++;
                }
            }
            if (itemsCount > 0)
                childElement.Add(collection);
        }

        private void AddConstructorsJSON(Type classType, XContainer childElement)
        {

            int itemsCount = 0;
            var collection = new XElement("constructors");
            var methodArray = classType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            foreach (var meth in methodArray)
            {
                var attrib = _library.GetMarkup(meth, ScriptMemberType.Constructor);
                if (attrib != null)
                {
                    var fullName = classType.FullName + "." + MethodId(meth);
                    var ctorName = "По умолчанию";
                    var namedArgsName = attrib.NamedArguments.FirstOrDefault(x => x.MemberName == "Name");
                    if (namedArgsName.MemberInfo != null)
                    {
                        ctorName = (string)namedArgsName.TypedValue.Value;
                        if (ctorName == "")
                            continue;
                    }
                    var element = new XElement(ctorName.Replace(" ", "_"));
                    element.Add(new XElement("name", ctorName));
                    element.Add(new XElement("signature", "(" + MethodIdJSON(meth, false) + ")"));
                    AppendXmlDocsJSON(element, "M:" + fullName);
                    collection.Add(element);
                    itemsCount++;
                }
            }
            if (itemsCount > 0)
                childElement.Add(collection);
        }

        private void AppendXmlDocs(XContainer element, string memberName)
        {
            XElement xDoc;
            if (_memberDocumentation.TryGetValue(memberName, out xDoc))
            {
                var summary = xDoc.Element("summary");
                if (summary != null)
                {
                    var descr = new XElement("description");
                    ProcessChildNodes(descr, summary);
                    element.Add(descr);
                }

                // parameters
                var paramsList = xDoc.Elements("param");
                foreach (var paramItem in paramsList)
                {
                    var param = new XElement("param");
                    ProcessChildNodes(param, paramItem);
                    element.Add(param);
                }

                // returns
                var returnNode = xDoc.Element("returns");
                if (returnNode != null)
                {
                    var node = new XElement("returns");
                    ProcessChildNodes(node, returnNode);
                    element.Add(node);
                }

                // other
                var elems = xDoc.Elements();
                foreach (var item in elems)
                {
                    if (item.Name == "summary" || item.Name == "param" || item.Name == "returns")
                        continue;

                    var node = new XElement(item.Name);
                    ProcessChildNodes(node, item);
                    element.Add(node);
                }
            }
        }

        private void AppendXmlDocsJSON(XContainer element, string memberName)
        {
            XElement xDoc;
            if (_memberDocumentation.TryGetValue(memberName, out xDoc))
            {
                foreach (var item in xDoc.Elements())
                {
                    if (item.Name == "param")
                        continue;

                    var nodeName = item.Name.ToString().Replace("summary", "description");
                    var xmlNode = new XElement(nodeName);
                    ProcessChildNodesJSON(xmlNode, item);
                    xmlNode.Value = xmlNode.Value.Replace("\r\n", " ");
                    if (xmlNode.Value != "")
                        element.Add(xmlNode);
                }

                var paramsList = xDoc.Elements("param");
                var param = new XElement("params");
                foreach (var paramItem in paramsList)
                {
                    ProcessChildNodesJSON(param, paramItem);
                }
                if (!param.IsEmpty)
                    element.Add(param);

            }
        }

        private void ProcessChildNodes(XContainer dest, XElement source)
        {
            var nodes = source.Nodes();
            StringBuilder textContent = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Text)
                {
                    textContent.Append(CollapseWhiteSpace(node.ToString()));
                }
                else if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    var newElem = new XElement(((XElement)node).Name);
                    ProcessChildNodes(newElem, (XElement)node);
                    dest.Add(newElem);
                }
            }

            foreach (var attr in source.Attributes())
            {
                dest.Add(attr);
            }

            if (textContent.Length > 0)
                dest.Add(textContent.ToString());
        }

        private void ProcessChildNodesJSON(XContainer dest, XElement source)
        {
            var nodes = source.Nodes();
            StringBuilder textContent = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Text)
                {
                    textContent.Append(new Regex("^\\s{12,13}", RegexOptions.Multiline).Replace(node.ToString(), "").Trim());
                }
                else if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (((XElement)node).Name == "code"){
                        textContent.Append(node.ToString());
                    } else {
                        var newElem = new XElement(((XElement)node).Name);
                        ProcessChildNodes(newElem, (XElement)node);
                        dest.Add(newElem);
                    }
                }
            }

            if (((XElement)dest).Name != "params" && ((XElement)dest).Name != "returns")
            {
                foreach (var attr in source.Attributes())
                {
                    dest.Add(attr);
                }
            }

            if (textContent.Length > 0) {
                if (((XElement)dest).Name == "example")
                    dest.Add(textContent.ToString().Replace("\r\n", "<br>").Replace("\n", " "));
                else if (((XElement)dest).Name == "params")
                    dest.Add(new XElement(source.FirstAttribute.Value, textContent.ToString().Replace("\r\n", " ").Replace("\n", " ")));
                else
                    dest.Add(textContent.ToString().Replace("\r\n", " ").Replace("\n", " "));
            }

        }

        private string CollapseWhiteSpace(string p)
        {
            if (p == String.Empty)
                return "";

            StringBuilder sb = new StringBuilder();
            using (var sr = new StringReader(p))
            {
                string line = null;
                do
                {
                    line = sr.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line))
                        sb.AppendLine(line.Trim());
                    else if (line != null && line.Length > 0)
                        sb.AppendLine();

                } while (line != null);
            }

            return sb.ToString();
        }

        private XDocument BeginOutputDoc()
        {
            XDocument result = new XDocument();
            result.Add(new XElement("contexts"));

            return result;
        }
    }
}

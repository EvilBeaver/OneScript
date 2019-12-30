using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneScriptDocumenter
{
    class DocumentBlocks
    {
        public StringBuilder TextGlobalContext;
        public StringBuilder TextContextDescription;
        public StringBuilder TextEnumsDescription;

        public DocumentBlocks()
        {
            TextGlobalContext = new StringBuilder();
            TextContextDescription = new StringBuilder();
            TextEnumsDescription = new StringBuilder();
        }
    }

    class Documenter
    {
        internal XDocument CreateDocumentation(List<string> assemblies)
        {
            XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("oscript-docs"));

            foreach (var assembly in assemblies)
            {
                var name = Path.GetFileNameWithoutExtension(assembly);
                var xmlName = Path.Combine(Path.GetDirectoryName(assembly), name + ".xml");
                if (!File.Exists(xmlName))
                {
                    Console.WriteLine("Missing xml-doc: {0}", xmlName);
                    continue;
                }

                Console.WriteLine("Processing: {0}", name);

                var docMaker = new AssemblyDocumenter(assembly, xmlName);
                var asmDoc = docMaker.CreateDocumentation();
                result.Root.Add(new XElement("assembly",
                    new XAttribute("name", name),
                    asmDoc.Root));
            }
            Console.WriteLine("Done");
            return result;
        }
        internal string CreateDocumentationJSON(string pathOutput, List<string> assemblies)
        {
            using (StreamWriter sbJSON = new StreamWriter(pathOutput))
            {

                DocumentBlocks textBlocks = new DocumentBlocks();
                bool isOscriptStd = false;

                var hostedScript = new AssemblyDocumenter(Path.Combine(Path.GetDirectoryName(assemblies[0]), "ScriptEngine.HostedScript.dll"));
                hostedScript.FillTypesDictionary();

                foreach (var assembly in assemblies)
                {
                    var name = Path.GetFileNameWithoutExtension(assembly);
                    var xmlName = Path.Combine(Path.GetDirectoryName(assembly), name + ".xml");
                    if (!File.Exists(xmlName))
                    {
                        Console.WriteLine("Missing xml-doc: {0}", xmlName);
                        continue;
                    }
                    if (name == "ScriptEngine.HostedScript")
                    {
                        isOscriptStd = true;
                    }

                    Console.WriteLine("Processing: {0}", name);

                    var docMaker = new AssemblyDocumenter(assembly, xmlName);
                    docMaker.CreateDocumentationJSON(textBlocks);
                }
                JObject jsonObj = new JObject();
                var list = JsonConvert.DeserializeObject<dynamic>("{" + textBlocks.TextGlobalContext.ToString() + "}");
                jsonObj.Add("structureMenu", JObject.Parse(@"{ }"));
                JObject structureMenu = jsonObj["structureMenu"] as JObject;
                if (isOscriptStd)
                {
                    using (var layout = new StreamReader(ExtFiles.Get("structureMenu.json")))
                    {
                        var content = layout.ReadToEnd();
                        var menu = JsonConvert.DeserializeObject<dynamic>(content);

                        foreach (JToken curType in menu)
                        {
                            if (((JProperty)curType).Name == "global")
                            {
                                structureMenu.Add(((JProperty)curType).Name, JObject.Parse(@"{ }"));
                            }
                            else
                            {
                                structureMenu.Add(((JProperty)curType).Name, curType.First);
                            }
                        }
                        foreach (JToken curType in list)
                        {
                            if (((JObject)structureMenu["global"]).GetValue(((JProperty)curType).Name) == null)
                            {
                                JObject elemStructure = jsonObj["structureMenu"]["global"] as JObject;
                                elemStructure.Add(((JProperty)curType).Name, JObject.Parse(@"{ }"));
                            }
                            if (((JProperty)curType).Value.SelectToken("properties") != null)
                            {
                                foreach (JToken elem in ((JProperty)curType).Value.SelectToken("properties"))
                                {
                                    JObject elemStructure = jsonObj["structureMenu"]["global"][((JProperty)curType).Name] as JObject;
                                    elemStructure.Add(((JProperty)elem).Name, "");
                                }
                            }
                            if (((JProperty)curType).Value.SelectToken("methods") != null)
                            {
                                foreach (JToken elem in ((JProperty)curType).Value.SelectToken("methods"))
                                {
                                    JObject elemStructure = jsonObj["structureMenu"]["global"][((JProperty)curType).Name] as JObject;
                                    elemStructure.Add(((JProperty)elem).Name, "");
                                }
                            }
                        }

                    }
                }
                else
                {
                    structureMenu.Add("classes", JsonConvert.DeserializeObject<dynamic>("{\n\"Прочее\": \"\"\n }"));
                }
                foreach (JToken curType in list)
                {
                    if (((JProperty)curType).Value.SelectToken("properties") != null)
                    {
                        if (jsonObj["globalvariables"] == null)
                            jsonObj.Add("globalvariables", JObject.Parse(@"{ }"));

                        JObject globalvariables = jsonObj["globalvariables"] as JObject;
                        foreach (JToken prop in curType.First["properties"])
                        {
                            globalvariables.Add(((JProperty)prop).Name, prop.First);
                        }
                    }
                    if (((JProperty)curType).Value.SelectToken("methods") != null)
                    {
                        if (jsonObj["globalfunctions"] == null)
                            jsonObj.Add("globalfunctions", JObject.Parse(@"{ }"));

                        JObject globalfunctions = jsonObj["globalfunctions"] as JObject;
                        foreach (JToken meth in curType.First["methods"])
                        {
                            JObject jsonDesc = new JObject();
                            foreach (JToken token in meth.First)
                            {
                                if (((JProperty)token).Name != "signature" && ((JProperty)token).Name != "params")
                                    jsonDesc.Add(((JProperty)token).Name, token.First);
                            }
                            if (((JProperty)meth).Value.SelectToken("signature") != null)
                            {
                                jsonDesc.Add("signature", JObject.Parse(@"{ }"));
                                JObject signature = jsonDesc["signature"] as JObject;
                                signature.Add("default", JObject.Parse(@"{ }"));
                                JObject defaultValue = signature["default"] as JObject;
                                defaultValue.Add("СтрокаПараметров", meth.First["signature"]);
                                if (((JProperty)meth).Value.SelectToken("params") != null)
                                {
                                    defaultValue.Add("Параметры", meth.First["params"]);
                                }
                                else
                                {
                                    defaultValue.Add("Параметры", JObject.Parse(@"{ }"));
                                }
                            }
                            globalfunctions.Add(((JProperty)meth).Name, jsonDesc);
                        }
                    }
                }
                jsonObj.Add("classes", JObject.Parse(@"{ }"));
                JObject classes = jsonObj["classes"] as JObject;
                var classesList = JsonConvert.DeserializeObject<dynamic>("{" + textBlocks.TextContextDescription.ToString() + "}");
                foreach (JToken classDesc in classesList)
                {
                    JObject jsonDescClass = new JObject();
                    foreach (JToken curType in classDesc.First)
                    {
                        if (((JProperty)curType).Name != "methods")
                            jsonDescClass.Add(((JProperty)curType).Name, curType.First);
                    }
                    if (((JProperty)classDesc).Value.SelectToken("methods") != null)
                    {
                        jsonDescClass.Add("methods", JObject.Parse(@"{ }"));
                        JObject methods = jsonDescClass["methods"] as JObject;
                        foreach (JToken method in classDesc.First["methods"])
                        {
                            JObject jsonDesc = new JObject();
                            if (method.First.First.Type == JTokenType.Object)
                            {
                                foreach (JToken token in method.First.First)
                                {
                                    if (((JProperty)token).Name != "signature" && ((JProperty)token).Name != "params" && ((JProperty)token).Name != "remarks")
                                        jsonDesc.Add(((JProperty)token).Name, token.First);
                                }
                                jsonDesc.Add("signature", JObject.Parse(@"{ }"));
                                JObject signature = jsonDesc["signature"] as JObject;
                                foreach (JToken varSyntax in method.First)
                                {
                                    signature.Add(varSyntax["remarks"].ToString(), JObject.Parse(@"{ }"));
                                    JObject defaultValue = signature[varSyntax["remarks"].ToString()] as JObject;
                                    if (varSyntax.SelectToken("signature") != null)
                                    {
                                        defaultValue.Add("СтрокаПараметров", varSyntax["signature"]);
                                    }
                                    else
                                    {
                                        defaultValue.Add("СтрокаПараметров", JObject.Parse(@"{ }"));
                                    }
                                    if (varSyntax.SelectToken("params") != null)
                                    {
                                        defaultValue.Add("Параметры", varSyntax["params"]);
                                    }
                                    else
                                    {
                                        defaultValue.Add("Параметры", JObject.Parse(@"{ }"));
                                    }
                                }
                            }
                            else
                            {
                                foreach (JToken token in method.First)
                                {
                                    if (((JProperty)token).Name != "signature" && ((JProperty)token).Name != "params")
                                        jsonDesc.Add(((JProperty)token).Name, token.First);
                                }
                                if (((JProperty)method).Value.SelectToken("signature") != null)
                                {
                                    jsonDesc.Add("signature", JObject.Parse(@"{ }"));
                                    JObject signature = jsonDesc["signature"] as JObject;
                                    signature.Add("default", JObject.Parse(@"{ }"));
                                    JObject defaultValue = signature["default"] as JObject;
                                    defaultValue.Add("СтрокаПараметров", method.First["signature"]);
                                    if (((JProperty)method).Value.SelectToken("params") != null)
                                    {
                                        defaultValue.Add("Параметры", method.First["params"]);
                                    }
                                    else
                                    {
                                        defaultValue.Add("Параметры", JObject.Parse(@"{ }"));
                                    }
                                }
                            }
                            methods.Add(((JProperty)method).Name, jsonDesc);
                        }
                    }
                    classes.Add(((JProperty)classDesc).Name, jsonDescClass);
                }
                jsonObj.Add("systemEnum", JObject.Parse(@"{ }"));
                JObject systemEnum = jsonObj["systemEnum"] as JObject;
                var systemEnumList = JsonConvert.DeserializeObject<dynamic>("{" + textBlocks.TextEnumsDescription.ToString() + "}");
                foreach (JToken curType in systemEnumList)
                {
                    systemEnum.Add(((JProperty)curType).Name, curType.First);
                }
                sbJSON.Write(JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
                Console.WriteLine("Done");
                return "";
            }
        }
    }
}

/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;

using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    [GlobalContext(Category = "Работа с макетами", ManualRegistration = true)]
    public class TemplateStorage : GlobalContextBase<TemplateStorage>, IDisposable
    {
        private readonly ITemplateFactory _factory;
        private readonly Dictionary<string, ITemplate> _templates = new Dictionary<string,ITemplate>();

        public TemplateStorage(ITemplateFactory factory)
        {
            _factory = factory;
        }
        
        public void RegisterTemplate(string file, string name, TemplateKind kind)
        {
            if (_templates.ContainsKey(name))
                throw RuntimeException.InvalidArgumentValue(name);
            
            var template = _factory.CreateTemplate(file, kind);
            _templates.Add(name, template);
        }
        
        public void RegisterTemplate(string name, ITemplate template)
        {
            if (_templates.ContainsKey(name))
                throw RuntimeException.InvalidArgumentValue(name);
            
            _templates.Add(name, template);
        }
        
        
        [ContextMethod("ПолучитьМакет")]
        public IValue GetTemplate(string templateName)
        {
            var template = _templates[templateName];
            if (template.Kind == TemplateKind.File)
                return ValueFactory.Create(template.GetFilename());

            return template.GetBinaryData();


        }

        public IEnumerable<KeyValuePair<string, ITemplate>> GetTemplates()
        {
            return _templates;
        }
        
        
        public void Dispose()
        {
            foreach (var template in _templates.Values)
            {
                template.Dispose();
            }
            _templates.Clear();
        }
    }

    /// <summary>
    /// Тип макета в приложении. Значением макета в типе Файл является путь к файлу с данными.
    /// </summary>
    [EnumerationType("ТипМакета", "TemplateKind")]
    public enum TemplateKind
    {
        [EnumItem("Файл")]
        File,
        [EnumItem("ДвоичныеДанные")]
        BinaryData
    }
    
}
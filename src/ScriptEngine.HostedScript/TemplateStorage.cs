/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Contexts.Enums;
using OneScript.Exceptions;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    /// <summary>
    /// Контекст позволяет обращаться к макетам приложения
    /// </summary>
    [GlobalContext(Category = "Работа с макетами", ManualRegistration = true)]
    public class TemplateStorage : GlobalContextBase<TemplateStorage>, IDisposable
    {
        private readonly ITemplateFactory _factory;
        // Макеты регистрируют библиотеки, которые могут загружаться во время работы, пока их читают другие потоки
        private readonly ConcurrentDictionary<string, ITemplate> _templates = new ConcurrentDictionary<string,ITemplate>();

        public TemplateStorage(ITemplateFactory factory)
        {
            _factory = factory;
        }

        public void RegisterTemplate(string file, string name, TemplateKind kind)
        {
            if (_templates.ContainsKey(name))
                throw RuntimeException.InvalidArgumentValue(name);

            var template = _factory.CreateTemplate(file, kind);
            if (!_templates.TryAdd(name, template))
            {
                template.Dispose();
                throw RuntimeException.InvalidArgumentValue(name);
            }
        }

        public void RegisterTemplate(string name, ITemplate template)
        {
            if (!_templates.TryAdd(name, template))
                throw RuntimeException.InvalidArgumentValue(name);
        }
        
        
        /// <summary>
        /// Получает ранее зарегистрированный макет.
        /// </summary>
        /// <param name="templateName">Имя макета</param>
        /// <returns>Строка или ДвоичныеДанные, в зависимости от типа макета.</returns>
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
        [EnumValue("Файл", "File")]
        File,
        [EnumValue("ДвоичныеДанные", "BinaryData")]
        BinaryData
    }
    
}
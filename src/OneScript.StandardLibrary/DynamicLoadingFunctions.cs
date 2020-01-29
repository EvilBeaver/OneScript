/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Динамическое подключение сценариев
    /// </summary>
    [GlobalContext(ManualRegistration = true)]
    public class DynamicLoadingFunctions : GlobalContextBase<DynamicLoadingFunctions>
    {
        private readonly ScriptingEngine _engine;

        public DynamicLoadingFunctions(ScriptingEngine engine)
        {
            _engine = engine;
        }
        
        /// <summary>
        /// Подключает сторонний файл сценария к текущей системе типов.
        /// Подключенный сценарий выступает, как самостоятельный класс, создаваемый оператором Новый
        /// </summary>
        /// <param name="path">Путь к подключаемому сценарию</param>
        /// <param name="typeName">Имя типа, которое будет иметь новый класс. Экземпляры класса создаются оператором Новый. </param>
        /// <example>ПодключитьСценарий("C:\file.os", "МойОбъект");
        /// А = Новый МойОбъект();</example>
        [ContextMethod("ПодключитьСценарий", "AttachScript")]
        public void AttachScript(string path, string typeName)
        {
            var compiler = _engine.GetCompilerService();
            _engine.AttachedScriptsFactory.AttachByPath(compiler, path, typeName);
        }

        /// <summary>
        /// Создает экземпляр объекта на основании стороннего файла сценария.
        /// Загруженный сценарий возвращается, как самостоятельный объект. 
        /// Экспортные свойства и методы скрипта доступны для вызова.
        /// </summary>
        /// <param name="code">Текст сценария</param>
        /// <param name="externalContext">Структура. Глобальные свойства, которые будут инжектированы в область видимости загружаемого скрипта. (Необязательный)</param>
        /// <example>
        /// Контекст = Новый Структура("ЧислоПи", 3.1415); // 4 знака хватит всем
        /// ЗагрузитьСценарийИзСтроки("Сообщить(ЧислоПи);", Контекст);</example>
        [ContextMethod("ЗагрузитьСценарийИзСтроки", "LoadScriptFromString")]
        public IRuntimeContextInstance LoadScriptFromString(string code, StructureImpl externalContext = null)
        {
            var compiler = _engine.GetCompilerService();
            if(externalContext == null)
                return _engine.AttachedScriptsFactory.LoadFromString(compiler, code);
            else
            {
                var extData = new ExternalContextData();

                foreach (var item in externalContext)
                {
                    extData.Add(item.Key.AsString(), item.Value);
                }

                return _engine.AttachedScriptsFactory.LoadFromString(compiler, code, extData);

            }
        }
        
        /// <summary>
        /// Создает экземпляр объекта на основании стороннего файла сценария.
        /// Загруженный сценарий возвращается, как самостоятельный объект. 
        /// Экспортные свойства и методы скрипта доступны для вызова.
        /// </summary>
        /// <param name="path">Путь к подключаемому сценарию</param>
        /// <param name="externalContext">Структура. Глобальные свойства, которые будут инжектированы в область видимости загружаемого скрипта. (Необязательный)</param>
        /// <example>
        /// Контекст = Новый Структура("ЧислоПи", 3.1415); // 4 знака хватит
        /// // В коде скрипта somescript.os будет доступна глобальная переменная "ЧислоПи"
        /// Объект = ЗагрузитьСценарий("somescript.os", Контекст);</example>
        [ContextMethod("ЗагрузитьСценарий", "LoadScript")]
        public IRuntimeContextInstance LoadScript(string path, StructureImpl externalContext = null)
        {
            var compiler = _engine.GetCompilerService();
            if(externalContext == null)
                return _engine.AttachedScriptsFactory.LoadFromPath(compiler, path);
            else
            {
                ExternalContextData extData = new ExternalContextData();

                foreach (var item in externalContext)
                {
                    extData.Add(item.Key.AsString(), item.Value);
                }

                return _engine.AttachedScriptsFactory.LoadFromPath(compiler, path, extData);

            }
        }

        /// <summary>
        /// Подключает внешнюю сборку среды .NET (*.dll) и регистрирует классы 1Script, объявленные в этой сборке.
        /// Публичные классы, отмеченные в dll атрибутом ContextClass, будут импортированы аналогично встроенным классам 1Script.
        /// Загружаемая сборка должна ссылаться на сборку ScriptEngine.dll
	    /// </summary>
        /// <example>
        /// ПодключитьВнешнююКомпоненту("C:\MyAssembly.dll");
        /// КлассИзКомпоненты = Новый КлассИзКомпоненты(); // тип объявлен внутри компоненты
        /// </example>
        /// <param name="dllPath">Путь к внешней компоненте</param>
        [ContextMethod("ПодключитьВнешнююКомпоненту", "AttachAddIn")]
        public void AttachAddIn(string dllPath)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(dllPath);
            _engine.AttachExternalAssembly(assembly, _engine.Environment);
        }
    }
}
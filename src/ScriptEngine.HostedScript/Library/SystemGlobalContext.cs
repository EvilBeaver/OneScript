/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Глобальный контекст. Представляет глобально доступные свойства и методы.
    /// </summary>
    [GlobalContext(Category="Процедуры и функции взаимодействия с системой", ManualRegistration=true)]
    public class SystemGlobalContext : IAttachableContext
    {
        private IVariable[] _state;
        private FixedArrayImpl  _args;
		private SymbolsContext _symbols;
        private readonly DynamicPropertiesHolder _propHolder = new DynamicPropertiesHolder();
        private readonly List<Func<IValue>> _properties = new List<Func<IValue>>();

        public SystemGlobalContext()
        {
            RegisterProperty("АргументыКоманднойСтроки", ()=>(IValue)CommandLineArguments);
            RegisterProperty("CommandLineArguments", () => (IValue)CommandLineArguments);

            FileStreams = new FileStreamsManager();
            RegisterProperty("ФайловыеПотоки", () => FileStreams);
            RegisterProperty("FileStreams", () => FileStreams);

			RegisterProperty("Символы", () => (IValue)Chars);
			RegisterProperty("Chars", () => (IValue)Chars);
        }

        private void RegisterProperty(string name, Func<IValue> getter)
        {
            _propHolder.RegisterProperty(name);
            _properties.Add(getter);
        }

        public ScriptingEngine EngineInstance{ get; set; }

        public void InitInstance()
        {
            InitContextVariables();
        }

        private void InitContextVariables()
        {
            _state = new IVariable[_properties.Count];

            var propNames = _propHolder.GetProperties().OrderBy(x=>x.Value).Select(x=>x.Key).ToArray();
            for (int i = 0; i < _properties.Count; i++)
            {
                _state[i] = Variable.CreateContextPropertyReference(this, i, propNames[i]);
            }
        }

        public IHostApplication ApplicationHost { get; set; }
        public ICodeSource CodeSource { get; set; }


        /// <summary>
        /// Менеджер файловых потоков.
        /// </summary>
        [ContextProperty("ФайловыеПотоки","FileStreams")]
        public FileStreamsManager FileStreams { get; }

        /// <summary>
        /// Выдает сообщение в консоль.
        /// </summary>
        /// <param name="message">Выдаваемое сообщение.</param>
        /// <param name="status">Статус сообщения. В зависимости от статуса изменяется цвет вывода сообщения.</param>
        [ContextMethod("Сообщить", "Message")]
		public void Echo(string message, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            ApplicationHost.Echo(message ?? "", status);
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
            var compiler = EngineInstance.GetCompilerService();
            EngineInstance.AttachedScriptsFactory.AttachByPath(compiler, path, typeName);
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
            var compiler = EngineInstance.GetCompilerService();
            if(externalContext == null)
                return EngineInstance.AttachedScriptsFactory.LoadFromString(compiler, code);
            else
            {
                var extData = new ExternalContextData();

                foreach (var item in externalContext)
                {
                    extData.Add(item.Key.AsString(), item.Value);
                }

                return EngineInstance.AttachedScriptsFactory.LoadFromString(compiler, code, extData);

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
            var compiler = EngineInstance.GetCompilerService();
            if(externalContext == null)
                return EngineInstance.AttachedScriptsFactory.LoadFromPath(compiler, path);
            else
            {
                ExternalContextData extData = new ExternalContextData();

                foreach (var item in externalContext)
                {
                    extData.Add(item.Key.AsString(), item.Value);
                }

                return EngineInstance.AttachedScriptsFactory.LoadFromPath(compiler, path, extData);

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
            EngineInstance.AttachExternalAssembly(assembly, EngineInstance.Environment);
        }

        /// <summary>
        /// Возвращает информацию о сценарии, который был точкой входа в программу.
        /// Можно выделить два вида сценариев: те, которые были подключены, как классы и те, которые запущены непосредственно. Метод СтартовыйСценарий возвращает информацию о сценарии, запущенном непосредственно.
        /// Для получения информации о текущем выполняемом сценарии см. метод ТекущийСценарий()
        /// </summary>
        /// <returns>Объект ИнформацияОСценарии</returns>
        [ContextMethod("СтартовыйСценарий", "EntryScript")]
        public IRuntimeContextInstance StartupScript()
        {
            return new ScriptInformationContext(CodeSource);
        }

        /// <summary>
        /// Приостанавливает выполнение скрипта.
        /// </summary>
        /// <param name="delay">Время приостановки в миллисекундах</param>
        [ContextMethod("Приостановить", "Sleep")]
        public void Sleep(int delay)
        {
            System.Threading.Thread.Sleep(delay);
        }

        /// <summary>
        /// Прерывает выполнение текущего скрипта.
        /// </summary>
        /// <param name="exitCode">Код возврата (ошибки), возвращаемый операционной системе.</param>
        [ContextMethod("ЗавершитьРаботу", "Exit")]
        public void Quit(int exitCode)
        {
            throw new ScriptInterruptionException(exitCode);
        }

        /// <summary>
        /// Ввод строки пользователем. Позволяет запросить у пользователя информацию.
        /// </summary>
        /// <param name="resut">Выходной параметр. Введенные данные в виде строки.</param>
        /// <param name="len">Максимальная длина вводимой строки. 
        /// Возможно указание неограниченной длины (длина=ноль), но данное поведение может не поддерживаться хост-приложением.</param>
        /// <returns>Булево. Истина, если пользователь ввел данные, Ложь, если отказался.</returns>
        [ContextMethod("ВвестиСтроку", "InputString")]
        public bool InputString([ByRef] IVariable resut, int len = 0)
        {
            string input;
            bool inputIsDone;

            inputIsDone = ApplicationHost.InputString(out input, len);

            if (inputIsDone)
            {
                resut.Value = ValueFactory.Create(input);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Явное освобождение ресурса через интерфейс IDisposable среды CLR.
        /// 
        /// OneScript не выполняет подсчет ссылок на объекты, а полагается на сборщик мусора CLR.
        /// Это значит, что объекты автоматически не освобождаются при выходе из области видимости. 
        /// 
        /// Метод ОсвободитьОбъект можно использовать для детерминированного освобождения ресурсов. Если объект поддерживает интерфейс IDisposable, то данный метод вызовет Dispose у данного объекта.
        /// 
        /// Как правило, интерфейс IDisposable реализуется различными ресурсами (файлами, соединениями с ИБ и т.п.)
        /// </summary>
        /// <param name="obj">Объект, ресурсы которого требуется освободить.</param>
        [ContextMethod("ОсвободитьОбъект", "FreeObject")]
        public void DisposeObject(IRuntimeContextInstance obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// OneScript не выполняет подсчет ссылок на объекты, а полагается на сборщик мусора CLR.
        /// Это значит, что объекты автоматически не освобождаются при выходе из области видимости.
        /// 
        /// С помощью данного метода можно запустить принудительную сборку мусора среды CLR.
        /// Данные метод следует использовать обдуманно, поскольку вызов данного метода не гарантирует освобождение всех объектов.
        /// Локальные переменные, например, до завершения текущего метода очищены не будут,
        /// поскольку до завершения текущего метода CLR будет видеть, что они используются движком 1Script.
        /// 
        /// </summary>
        [ContextMethod("ВыполнитьСборкуМусора", "RunGarbageCollection")]
        public void RunGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Доступ к аргументам командной строки.
        /// Объект АргументыКоманднойСтроки представляет собой массив в режиме "только чтение".
        /// </summary>
        [ContextProperty("АргументыКоманднойСтроки", "CommandLineArguments", CanWrite = false)]
        public IRuntimeContextInstance CommandLineArguments
        {
            get
            {
                if (_args == null)
                {
                    var argsArray = new ArrayImpl();
                    if (ApplicationHost != null)
                    {
                        foreach (var arg in ApplicationHost.GetCommandLineArguments())
                        {
                            argsArray.Add(ValueFactory.Create(arg));
                        }
                    }
                    _args = new FixedArrayImpl(argsArray);
                }

                return _args;
            }

        }

		/// <summary>
		/// Содержит набор системных символов.
		/// </summary>
		/// <value>Набор системных символов.</value>
		[ContextProperty("Символы")]
		public IRuntimeContextInstance Chars
		{
			get
			{
				if (_symbols == null)
				{
					_symbols = new SymbolsContext();
				}

				return _symbols;
			}
		}

        /// <summary>
        /// Запуск приложения в операционной системе
        /// </summary>
        /// <param name="cmdLine">Командная строка запуска</param>
        /// <param name="currentDir">Текущая директория запускаемого процесса (необязательно)</param>
        /// <param name="wait">Ожидать завершения (необязательно) по умолчанию Ложь</param>
        /// <param name="retCode">Выходной параметр. Код возврата процесса. Имеет смысл только если указан параметр wait=true</param>
        [ContextMethod("ЗапуститьПриложение", "RunApp")]
        public void RunApp(string cmdLine, string currentDir = null, bool wait = false, [ByRef] IVariable retCode = null)
        {
            var sInfo = ProcessContext.PrepareProcessStartupInfo(cmdLine, currentDir);

            var p = new System.Diagnostics.Process();
            p.StartInfo = sInfo;
            p.Start();

            if(wait)
            {
                p.WaitForExit();
                if(retCode != null)
                    retCode.Value = ValueFactory.Create(p.ExitCode);
            }

        }

        /// <summary>
        /// Создает процесс, которым можно манипулировать из скрипта
        /// </summary>
        /// <param name="cmdLine">Командная строка запуска</param>
        /// <param name="currentDir">Текущая директория запускаемого процесса (необязательно)</param>
        /// <param name="redirectOutput">Перехватывать стандартные потоки stdout и stderr</param>
        /// <param name="redirectInput">Перехватывать стандартный поток stdin</param>
        /// <param name="encoding">Кодировка стандартных потоков вывода и ошибок</param>
        /// <param name="env">Соответствие, где установлены значения переменных среды</param>
        [ContextMethod("СоздатьПроцесс", "CreateProcess")]
        public ProcessContext CreateProcess(string cmdLine, string currentDir = null, bool redirectOutput = false, bool redirectInput = false, IValue encoding = null, MapImpl env = null)
        {
            return ProcessContext.Create(cmdLine, currentDir, redirectOutput, redirectInput, encoding, env);
        }

        /// <summary>
        /// Выполняет поиск процесса по PID среди запущенных в операционной системе
        /// </summary>
        /// <param name="PID">Идентификатор процесса</param>
        /// <returns>Процесс. Если не найден - Неопределено</returns>
        [ContextMethod("НайтиПроцессПоИдентификатору", "FindProcessById")]
        public IValue FindProcessById(int PID)
        {
            System.Diagnostics.Process process;
            try
            {
                process = System.Diagnostics.Process.GetProcessById(PID);
            }
            catch (ArgumentException)
            {
                return ValueFactory.Create();
            }

            return ValueFactory.Create(new ProcessContext(process));

        }

        /// <summary>
        /// Выполняет поиск процессов с определенным именем
        /// </summary>
        /// <param name="name">Имя процесса</param>
        /// <returns>Массив объектов Процесс.</returns>
        [ContextMethod("НайтиПроцессыПоИмени", "FindProcessesByName")]
        public IValue FindProcessesByName(string name)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(name);
            var contextWrappers = processes.Select(x => new ProcessContext(x));

            return new ArrayImpl(contextWrappers);

        }

        /// <summary>
        /// Каталог исполняемых файлов OneScript
        /// </summary>
        /// <returns></returns>
        [ContextMethod("КаталогПрограммы","ProgramDirectory")]
        public string ProgramDirectory()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var filename = asm.Location;

            return System.IO.Path.GetDirectoryName(filename);
        }

        [ContextMethod("КраткоеПредставлениеОшибки", "BriefErrorDescription")]
        public string BriefErrorDescription(ExceptionInfoContext errInfo)
        {
            return errInfo.Description;
        }

        [ContextMethod("ПодробноеПредставлениеОшибки", "DetailErrorDescription")]
        public string DetailErrorDescription(ExceptionInfoContext errInfo)
        {
            return errInfo.DetailedDescription;
        }

        [ContextMethod("ТекущаяУниверсальнаяДата", "CurrentUniversalDate")]
        public IValue CurrentUniversalDate()
        {
            return ValueFactory.Create(DateTime.UtcNow);
        }

        [ContextMethod("ТекущаяУниверсальнаяДатаВМиллисекундах", "CurrentUniversalDateInMilliseconds")]
        public long CurrentUniversalDateInMilliseconds()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Проверяет заполненность значения по принципу, заложенному в 1С:Предприятии
        /// </summary>
        /// <param name="inValue"></param>
        /// <returns></returns>
        [ContextMethod("ЗначениеЗаполнено","ValueIsFilled")]
        public bool ValueIsFilled(IValue inValue)
        {
            var value = inValue?.GetRawValue();
            if (value == null)
            {
                return false;
            }
            if (value.DataType == DataType.Undefined)
                return false;
            else if (value.DataType == DataType.Boolean)
                return true;
            else if (value.DataType == DataType.String)
                return !String.IsNullOrWhiteSpace(value.AsString());
            else if (value.DataType == DataType.Number)
                return value.AsNumber() != 0;
            else if (value.DataType == DataType.Date)
            {
                var emptyDate = new DateTime(1, 1, 1, 0, 0, 0);
                return value.AsDate() != emptyDate;
            }
            else if (value is COMWrapperContext)
            {
                return true;
            }
            else if (value is ICollectionContext)
            {
                var col = value as ICollectionContext;
                return col.Count() != 0;
            }
            else if (ValueFactory.CreateNullValue().Equals(value))
            {
                return false;
            }
            else
                return true;
            
        }

        /// <summary>
        /// Заполняет одноименные значения свойств одного объекта из другого
        /// </summary>
        /// <param name="acceptor">Объект-приемник</param>
        /// <param name="source">Объект-источник</param>
        /// <param name="filledProperties">Заполняемые свойства (строка, через запятую)</param>
        /// <param name="ignoredProperties">Игнорируемые свойства (строка, через запятую)</param>
        [ContextMethod("ЗаполнитьЗначенияСвойств","FillPropertyValues")]
        public void FillPropertyValues(IRuntimeContextInstance acceptor, IRuntimeContextInstance source, IValue filledProperties = null, IValue ignoredProperties = null)
        {
            string strFilled;
            string strIgnored;

            if (filledProperties == null || filledProperties.DataType == DataType.Undefined)
            {
                strFilled = null;
            }
            else if (filledProperties.DataType == DataType.String)
            {
                strFilled = filledProperties.AsString();
            }
            else
            {
                throw RuntimeException.InvalidArgumentType(3, nameof(filledProperties));
            }

            if (ignoredProperties == null || ignoredProperties.DataType == DataType.Undefined)
            {
                strIgnored = null;
            }
            else if (ignoredProperties.DataType == DataType.String)
            {
                strIgnored = ignoredProperties.AsString();
            }
            else
            {
                throw RuntimeException.InvalidArgumentType(4, nameof(ignoredProperties));
            }

            FillPropertyValuesStr(acceptor, source, strFilled, strIgnored);
        }

        public void FillPropertyValuesStr(IRuntimeContextInstance acceptor, IRuntimeContextInstance source, string filledProperties = null, string ignoredProperties = null)
        {
            IEnumerable<string> sourceProperties;

            if (filledProperties == null)
            {
                string[] names = new string[source.GetPropCount()];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = source.GetPropName(i);
                }

                if (ignoredProperties == null)
                {
                    sourceProperties = names;
                }
                else
                {
                    IEnumerable<string> ignoredPropCollection = ignoredProperties.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0);

                    sourceProperties = names.Where(x => !ignoredPropCollection.Contains(x));
                }
            }
            else
            {
                sourceProperties = filledProperties.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);

                // Проверка существования заявленных свойств
                foreach (var item in sourceProperties)
                {
                    acceptor.FindProperty(item); // бросает PropertyAccessException если свойства нет
                }
            }


            foreach (var srcProperty in sourceProperties)
            {
                try
                {
                    var srcPropIdx = source.FindProperty(srcProperty);
                    var accPropIdx = acceptor.FindProperty(srcProperty); // бросает PropertyAccessException если свойства нет

                    if (source.IsPropReadable(srcPropIdx) && acceptor.IsPropWritable(accPropIdx))
                        acceptor.SetPropValue(accPropIdx, source.GetPropValue(srcPropIdx));

                }
                catch (PropertyAccessException)
                {
                    // игнорировать свойства Источника, которых нет в Приемнике
                }
            }
        }


        /// <summary>
        /// Получает объект класса COM по его имени или пути. Подробнее см. синтакс-помощник от 1С.
        /// </summary>
        /// <param name="pathName">Путь к библиотеке</param>
        /// <param name="className">Имя класса</param>
        /// <returns>COMОбъект</returns>
        [ContextMethod("ПолучитьCOMОбъект", "GetCOMObject")]
        public IValue GetCOMObject(string pathName = null, string className = null)
        {
            var comObject = GetCOMObjectInternal(pathName, className);

            return COMWrapperContext.Create(comObject);
        }

        /// <summary>
        /// Ported from Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
        /// By JetBrains dotPeek decompiler
        /// </summary>
        private object GetCOMObjectInternal(string pathName = null, string className = null)
        {
            if (String.IsNullOrEmpty(className))
            {
                return Marshal.BindToMoniker(pathName);
            }
            else if (pathName == null)
            {
#if (NETSTANDARD2_0 || NETSTANDARD2_1) 
                throw new NotSupportedException("Getting object by classname not supported on netstandard2");
#else
                return Marshal.GetActiveObject(className);
#endif
            }
            else if (pathName.Length == 0)
            {
                return Activator.CreateInstance(System.Type.GetTypeFromProgID(className));
            }
            else
            {
#if (NETSTANDARD2_0 || NETSTANDARD2_1) 
                throw new NotSupportedException("Getting object by classname not supported on netstandard2");
#else
                var persistFile = (IPersistFile)Marshal.GetActiveObject(className);
                persistFile.Load(pathName, 0);
                
                return (object)persistFile;
#endif
            }
        }
        
#region IAttachableContext Members

        public void OnAttach(MachineInstance machine, 
            out IVariable[] variables, 
            out MethodInfo[] methods)
        {
            if (_state == null)
                InitContextVariables();

            variables = _state;
            methods = new MethodInfo[_methods.Count];
            for (int i = 0; i < _methods.Count; i++)
            {
                methods[i] = _methods.GetMethodInfo(i);
            }
        }

#endregion

#region IRuntimeContextInstance Members

        public bool IsIndexed
        {
            get 
            { 
                return false; 
            }
        }

        public bool DynamicMethodSignatures
        {
            get
            {
                return false;
            }
        }

        public IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public int FindProperty(string name)
        {
            return _propHolder.GetPropertyNumber(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return true;
        }

        public bool IsPropWritable(int propNum)
        {
            return false;
        }

        public IValue GetPropValue(int propNum)
        {
            return _properties[propNum]();
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new InvalidOperationException("global props are not writable");
        }

        public int GetPropCount()
        {
            return _properties.Count;
        }

        public string GetPropName(int index)
        {
            return _propHolder.GetProperties().First(x => x.Value == index).Key;
        }

        public int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public int GetMethodsCount()
        {
            return _methods.Count;
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _methods.GetMethod(methodNumber)(this, arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = _methods.GetMethod(methodNumber)(this, arguments);
        }

#endregion

        private static readonly ContextMethodsMapper<SystemGlobalContext> _methods;

        static SystemGlobalContext()
        {
            _methods = new ContextMethodsMapper<SystemGlobalContext>();
        }
        
    }
}

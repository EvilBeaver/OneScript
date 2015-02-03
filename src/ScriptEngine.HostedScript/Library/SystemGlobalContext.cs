using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Глобальный контекст. Представляет глобально доступные свойства и методы.
    /// </summary>
    [GlobalContext(Category="Процедуры и функции взаимодействия с системой", ManualRegistration=true)]
    class SystemGlobalContext : IRuntimeContextInstance, IAttachableContext
    {
        private IVariable[] _state;
        private CommandLineArguments _args;
        private DynamicPropertiesHolder _propHolder = new DynamicPropertiesHolder();
        private List<Func<IValue>> _properties = new List<Func<IValue>>();

        public SystemGlobalContext()
        {
            RegisterProperty("АргументыКоманднойСтроки", new Func<IValue>(()=>(IValue)CommandLineArguments));
        }

        public void RegisterProperty(string name, IValue value)
        {
            RegisterProperty(name, () => value);
        }

        private void RegisterProperty(string name, Func<IValue> getter)
        {
            _propHolder.RegisterProperty(name);
            _properties.Add(getter);
        }

        internal ScriptingEngine EngineInstance{ get; set; }

        public void InitInstance()
        {
            InitContextVariables();
        }

        private void InitContextVariables()
        {
            _state = new IVariable[_properties.Count];

            for (int i = 0; i < _properties.Count; i++)
            {
                _state[i] = Variable.CreateContextPropertyReference(this, i);
            }
        }

        public IHostApplication ApplicationHost { get; set; }
        public ICodeSource CodeSource { get; set; }

        /// <summary>
        /// Выдает сообщение в консоль.
        /// </summary>
        /// <param name="message">Выдаваемое сообщение.</param>
        [ContextMethod("Сообщить", "Message")]
        public void Echo(string message)
        {
            ApplicationHost.Echo(message);
        }

        /// <summary>
        /// Подключает сторонний файл сценария к текущей системе типов.
        /// Подключенный сценарий выступает, как самостоятельный класс, создаваемый оператором Новый
        /// </summary>
        /// <param name="path">Путь к подключаемому сценарию</param>
        /// <param name="typeName">Имя типа, которое будет иметь новый класс. Экземпляры класса создаются оператором Новый.
        /// <example>ПодключитьСценарий("C:\file.os", "МойОбъект");
        /// А = Новый МойОбъект();</example>
        /// </param>
        [ContextMethod("ПодключитьСценарий", "LoadScript")]
        public void LoadScript(string path, string typeName)
        {
            var compiler = EngineInstance.GetCompilerService();
            EngineInstance.AttachedScriptsFactory.AttachByPath(compiler, path, typeName);
        }

        /// <summary>
        /// Возвращает информацию о сценарии, который был точкой входа в программу.
        /// Можно выделить два вида сценариев: те, которые были подключены, как классы и те, которые запущены непосредственно. Метод СтартовыйСценарий возвращает информацию о сценарии, запущенном непосредственно.
        /// Для получения информации о текущем выполняемом сценарии см. метод ТекущийСценарий()
        /// </summary>
        /// <returns>Объект ИнформацияОСценарии</returns>
        [ContextMethod("СтартовыйСценарий", "EntryScript")]
        public IRuntimeContextInstance CurrentScript()
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
                    if (ApplicationHost == null)
                    {
                        _args = Library.CommandLineArguments.Empty;
                    }
                    else
                    {
                        _args = new CommandLineArguments(ApplicationHost.GetCommandLineArguments());
                    }
                }

                return _args;
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
            var sInfo = PrepareProcessStartupInfo(cmdLine, currentDir);

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
        [ContextMethod("СоздатьПроцесс", "CreateProcess")]
        public ProcessContext CreateProcess(string cmdLine, string currentDir = null, bool redirectOutput = false, bool redirectInput = false)
        {
            var sInfo = PrepareProcessStartupInfo(cmdLine, currentDir);
            sInfo.UseShellExecute = false;
            if (redirectInput)
                sInfo.RedirectStandardInput = true;

            if(redirectOutput)
            {
                sInfo.RedirectStandardOutput = true;
                sInfo.RedirectStandardError = true;
            }

            var p = new System.Diagnostics.Process();
            p.StartInfo = sInfo;

            return new ProcessContext(p);

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

        [ContextMethod("КаталогПрограммы","ProgramDirectory")]
        public string ProgramDirectory()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var filename = asm.Location;

            return System.IO.Path.GetDirectoryName(filename);
        }

        private static System.Diagnostics.ProcessStartInfo PrepareProcessStartupInfo(string cmdLine, string currentDir)
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo();

            var enumArgs = Utils.SplitCommandLine(cmdLine);

            bool fNameRead = false;
            StringBuilder argsBuilder = new StringBuilder();

            foreach (var item in enumArgs)
            {
                if (!fNameRead)
                {
                    sInfo.FileName = item;
                    fNameRead = true;
                }
                else
                {
                    argsBuilder.Append(' ');
                    argsBuilder.Append(item);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(0, 1);
            }

            sInfo.Arguments = argsBuilder.ToString();
            if (currentDir != null)
                sInfo.WorkingDirectory = currentDir;
            return sInfo;
        }

        /// <summary>
        /// Текущая дата машины
        /// </summary>
        /// <returns>Дата</returns>
        [ContextMethod("ТекущаяДата", "CurrentDate")]
        public DateTime CurrentDate()
        {
            return DateTime.Now;
        }

        [ContextMethod("ЗначениеЗаполнено","IsValueFilled")]
        public bool IsValueFilled(IValue value)
        {
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
            else if (value.GetRawValue() is ICollectionContext)
            {
                var col = value.GetRawValue() as ICollectionContext;
                return col.Count() != 0;
            }
            else
                return true;
            
        }

        [ContextMethod("ЗаполнитьЗначенияСвойств","FillPropertyValues")]
        public void FillPropertyValues(IRuntimeContextInstance acceptor, IRuntimeContextInstance source, string filledProperties = null, string ignoredProperties = null)
        {
            var accReflector = acceptor as IReflectableContext;
            if (accReflector == null)
                throw RuntimeException.InvalidArgumentValue();
            
            var srcReflector = source as IReflectableContext;
            if (srcReflector == null)
                throw RuntimeException.InvalidArgumentValue();

            IEnumerable<string> sourceProperties;
            IEnumerable<string> ignoredPropCollection;
            if(filledProperties == null)
            {
                sourceProperties = srcReflector.GetProperties().Select(x => x.Identifier);
            }
            else
            {
                sourceProperties = filledProperties.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

                // Проверка существования заявленных свойств
                foreach (var item in sourceProperties)
                {
                    acceptor.FindProperty(item);
                }
            }

            if(ignoredProperties != null)
            {
                ignoredPropCollection = ignoredProperties.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);
            }
            else
            {
                ignoredPropCollection = new string[0];
            }

            foreach (var srcProperty in sourceProperties.Where(x=>!ignoredPropCollection.Contains(x)))
            {
                try
                {
                    var propIdx = acceptor.FindProperty(srcProperty);
                    var srcPropIdx = source.FindProperty(srcProperty);

                    acceptor.SetPropValue(propIdx, source.GetPropValue(srcPropIdx));

                }
                catch(PropertyAccessException)
                {
                }

            }

        }

        #region IAttachableContext Members

        public void OnAttach(MachineInstance machine, 
            out IVariable[] variables, 
            out MethodInfo[] methods, 
            out IRuntimeContextInstance instance)
        {
            variables = _state;
            methods = GetMethods().ToArray();
            instance = this;
        }

        public IEnumerable<VariableInfo> GetProperties()
        {
            VariableInfo[] array = new VariableInfo[_properties.Count];
            foreach (var propKeyValue in _propHolder.GetProperties())
            {
                var descr = new VariableInfo();
                descr.Identifier = propKeyValue.Key;
                descr.Type = SymbolType.ContextProperty;
                array[propKeyValue.Value] = descr;
            }
            
            return array;
        }

        public IEnumerable<MethodInfo> GetMethods()
        {
            var array = new MethodInfo[_methods.Count];
            for (int i = 0; i < _methods.Count; i++)
            {
                array[i] = _methods.GetMethodInfo(i);
            }

            return array;
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

        public int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
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

        private static ContextMethodsMapper<SystemGlobalContext> _methods;

        static SystemGlobalContext()
        {
            _methods = new ContextMethodsMapper<SystemGlobalContext>();
        }


    }
}

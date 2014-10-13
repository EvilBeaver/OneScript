using ScriptEngine.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.HostedScript;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;

namespace ScriptEngine.Machine.Library
{
    /// <summary>
    /// Глобальный контекст. Представляет глобально доступные свойства и методы.
    /// </summary>
    class GlobalContext : IRuntimeContextInstance, IAttachableContext
    {
        private IVariable[] _state;
        private CommandLineArguments _args;
        private DynamicPropertiesHolder _propHolder = new DynamicPropertiesHolder();
        private List<Func<IValue>> _properties = new List<Func<IValue>>();

        public GlobalContext()
        {
            RegisterProperty("АргументыКоманднойСтроки", new Func<IValue>(()=>(IValue)CommandLineArguments));
            RegisterProperty("Символы", new Func<IValue>(() => (IValue)SymbolsEnum));
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
        /// Возвращает информацию о текущем сценарии.
        /// </summary>
        /// <returns>Объект ИнформацияОСценарии</returns>
        [ContextMethod("ТекущийСценарий", "CurrentScript")]
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
        /// Копирует файл из одного расположения в другое. Перезаписывает приемник, если он существует.
        /// </summary>
        /// <param name="source">Имя файла-источника</param>
        /// <param name="destination">Имя файла приемника</param>
        [ContextMethod("КопироватьФайл", "CopyFile")]
        public void CopyFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination, true);
        }

        /// <summary>
        /// Перемещает файл из одного расположения в другое.
        /// </summary>
        /// <param name="source">Имя файла-источника</param>
        /// <param name="destination">Имя файла приемника</param>
        [ContextMethod("ПереместитьФайл", "MoveFile")]
        public void MoveFile(string source, string destination)
        {
            System.IO.File.Move(source, destination);
        }

        /// <summary>
        /// Возвращает каталог временных файлов ОС
        /// </summary>
        /// <returns>Путь к каталогу временных файлов</returns>
        [ContextMethod("КаталогВременныхФайлов", "TempFilesDir")]
        public string TempFilesDir()
        {
            return System.IO.Path.GetTempPath();
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
        /// Если объект поддерживает этот интерфейс, то данный метод вызовет Dispose у данного объекта.
        /// Как правило, интерфейс IDisposable реализуется различными ресурсами (файлами, соединениями с ИБ и т.п.)
        /// </summary>
        /// <param name="obj">Объект, ресурсы которого требуется освободить.</param>
        /// <remarks>OneScript не выполняет подсчет ссылок на объекты, а полагается на сборщик мусора CLR.
        /// Это значит, что объекты автоматически не освобождаются при выходе из области видимости. Метод ОсвободитьОбъект можно использовать для детерминированного освобождения ресурсов.</remarks>
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
        [ContextProperty("АргументыКоманднойСтроки", CanWrite = false)]
        public IRuntimeContextInstance CommandLineArguments
        {
            get
            {
                if (_args == null)
                {
                    if (ApplicationHost == null)
                    {
                        _args = ScriptEngine.Machine.Library.CommandLineArguments.Empty;
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
        /// Специальные символы
        /// </summary>
        [ContextProperty("Символы")]
        public IRuntimeContextInstance SymbolsEnum
        {
            get
            {
                return Library.SymbolsEnum.GetInstance();
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
            var sInfo = new System.Diagnostics.ProcessStartInfo();

            var enumArgs = Utils.SplitCommandLine(cmdLine);

            bool fNameRead = false;
            StringBuilder argsBuilder = new StringBuilder();

            foreach (var item in enumArgs)
            {
                if(!fNameRead)
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

            if(argsBuilder.Length > 0)
            {
                argsBuilder.Remove(0, 1);
            }

            sInfo.Arguments = argsBuilder.ToString();
            if(currentDir != null)
                sInfo.WorkingDirectory = currentDir;

            var p = new System.Diagnostics.Process();
            p.StartInfo = sInfo;
            p.Start();

            if(wait)
            {
                p.WaitForExit();
                retCode.Value = ValueFactory.Create(p.ExitCode);
            }

        }

        /// <summary>
        /// Выполняет поиск файлов по маске
        /// </summary>
        /// <param name="dir">Каталог, в котором выполняется поиск</param>
        /// <param name="mask">Маска имени файла (включая символы * и ?)</param>
        /// <param name="recursive">Флаг рекурсивного поиска в поддиректориях</param>
        /// <returns>Массив объектов Файл, которые были найдены.</returns>
        [ContextMethod("НайтиФайлы","FindFiles")]
        public IRuntimeContextInstance FindFiles(string dir, string mask = null, bool recursive = false)
        {
            if(mask == null)
            {
                recursive = false;
                if(System.Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    mask = "*";
                }
                else
                {
                    mask = "*.*";
                }
            }

            System.IO.SearchOption mode = recursive? System.IO.SearchOption.AllDirectories: System.IO.SearchOption.TopDirectoryOnly;
            var entries = System.IO.Directory.EnumerateFileSystemEntries(dir, mask, mode)
                .Select<string, IValue>((x)=>new FileContext(x));

            return new ArrayImpl(entries);

        }

        /// <summary>
        /// Удаление файлов
        /// </summary>
        /// <param name="path">Каталог из которого удаляются файлы, или сам файл.</param>
        /// <param name="mask">Маска файлов. Необязательный параметр. Если указан, то первый параметр трактуется, как каталог.</param>
        [ContextMethod("УдалитьФайлы", "DeleteFiles")]
        public void DeleteFiles(string path, string mask = "")
        {
            if (mask == null)
            {
                var file = new FileContext(path);
                if (file.IsDirectory())
                {
                    System.IO.Directory.Delete(path, true);
                }
                else
                {
                    System.IO.File.Delete(path);
                }
            }
            else
            {
                var entries = System.IO.Directory.EnumerateFileSystemEntries(path, mask)
                    .AsParallel();
                foreach (var item in entries)
                {
                    System.IO.FileInfo finfo = new System.IO.FileInfo(item);
                    if (finfo.Attributes == System.IO.FileAttributes.Directory)
                    {
                        //recursively delete directory
                        System.IO.Directory.Delete(item, true);
                    }
                    else
                    {
                        System.IO.File.Delete(item);
                    }
                }
            }
        }

        /// <summary>
        /// Создать каталог
        /// </summary>
        /// <param name="path">Имя нового каталога</param>
        [ContextMethod("СоздатьКаталог", "CreateDirectory")]
        public void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
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

        #endregion

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

        private static ContextMethodsMapper<GlobalContext> _methods;

        static GlobalContext()
        {
            _methods = new ContextMethodsMapper<GlobalContext>();
        }


    }
}

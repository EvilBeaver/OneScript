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

        [ContextMethod("Сообщить", "Message")]
        public void Echo(string message)
        {
            ApplicationHost.Echo(message);
        }

        [ContextMethod("ПодключитьСценарий", "LoadScript")]
        public void LoadScript(string path, string typeName)
        {
            var compiler = EngineInstance.GetCompilerService();
            EngineInstance.AttachedScriptsFactory.AttachByPath(compiler, path, typeName);
        }

        [ContextMethod("ТекущийСценарий", "CurrentScript")]
        public IRuntimeContextInstance CurrentScript()
        {
            return new ScriptInformationContext(CodeSource);
        }

        [ContextMethod("Приостановить", "Sleep")]
        public void Sleep(int delay)
        {
            System.Threading.Thread.Sleep(delay);
        }

        [ContextMethod("ЗавершитьРаботу", "Exit")]
        public void Quit(int exitCode)
        {
            throw new ScriptInterruptionException(exitCode);
        }

        [ContextMethod("КопироватьФайл", "CopyFile")]
        public void CopyFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination, true);
        }

        [ContextMethod("ПереместитьФайл", "MoveFile")]
        public void MoveFile(string source, string destination)
        {
            System.IO.File.Move(source, destination);
        }

        [ContextMethod("КаталогВременныхФайлов", "TempFilesDir")]
        public string TempFilesDir()
        {
            return System.IO.Path.GetTempPath();
        }

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

        [ContextMethod("ОсвободитьОбъект", "FreeObject")]
        public void DisposeObject(IRuntimeContextInstance obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

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

        [ContextProperty("Символы")]
        public IRuntimeContextInstance SymbolsEnum
        {
            get
            {
                return Library.SymbolsEnum.GetInstance();
            }
        }

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

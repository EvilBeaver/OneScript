using ScriptEngine.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.HostedScript;
using ScriptEngine.Environment;
using ScriptEngine.Machine.Contexts;

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

        [ContextMethod("ПрочитатьСтандартныйВвод", "ReadStdIn")]
        public string ReadStdIn(int length)
        {
            using (var reader = new System.IO.StreamReader(Console.OpenStandardInput()))
            {
                char[] buffer = new char[length];
                reader.Read(buffer, 0, length);

                return new string(buffer);
            }
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

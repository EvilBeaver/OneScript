using ScriptEngine.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Environment;

namespace ScriptEngine.Machine.Library
{
    class GlobalContext : IRuntimeContextInstance, IAttachableContext, ICompilerSymbolsProvider
    {
        private Process _currentProcess;
        private IVariable[] _state;
        private DynamicPropertiesHolder _propHolder = new DynamicPropertiesHolder();
        private List<Func<IValue>> _properties = new List<Func<IValue>>();

        public GlobalContext()
        {
            InitLibrary();
        }

        public void SetProcess(Process process)
        {
            _currentProcess = process;
            InitInstance();
        }

        private void InitLibrary()
        {
            var discoverer = new ContextDiscoverer();
            discoverer.Discover(System.Reflection.Assembly.GetExecutingAssembly());
            //StdLib.Initialize();
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

        private void InitInstance()
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

        //[ContextMethod("Сообщить")]
        //public void Echo(string message)
        //{
        //    _currentProcess.ApplicationHost.Echo(message);
        //}

        //[ContextMethod("ПодключитьСценарий")]
        //public void LoadScript(string path, string typeName)
        //{
        //    AttachedScriptsFactory.Attach(path, typeName);
        //}

        //[ContextMethod("ТекущийСценарий")]
        //public IRuntimeContextInstance CurrentScript()
        //{
        //    return new ScriptInformationContext(_currentProcess.CodeSource);
        //}

        [ContextMethod("Приостановить")]
        public void Sleep(int delay)
        {
            System.Threading.Thread.Sleep(delay);
        }

        [ContextMethod("ЗавершитьРаботу")]
        public void Quit(int exitCode)
        {
            throw new ScriptInterruptionException(exitCode);
        }

        //[ContextMethod("ВвестиСтроку")]
        //public bool InputString([ByRef] IVariable resut, int len = 0)
        //{
        //    string input;
        //    bool inputIsDone;

        //    inputIsDone = _currentProcess.ApplicationHost.InputString(out input, len);

        //    if (inputIsDone)
        //    {
        //        resut.Value = ValueFactory.Create(input);
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        [ContextMethod("ОсвободитьОбъект")]
        public void DisposeObject(IRuntimeContextInstance obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
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

        #region ICompilerSymbolsProvider Members

        public IEnumerable<Compiler.VariableDescriptor> GetSymbols()
        {
            VariableDescriptor[] array = new VariableDescriptor[_properties.Count];
            foreach (var propKeyValue in _propHolder.GetProperties())
            {
                var descr = new VariableDescriptor();
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
            get { return false; }
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

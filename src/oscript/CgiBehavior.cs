/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class CgiBehavior : AppBehavior,  IHostApplication, IRuntimeContextInstance, IAttachableContext
    {
        private bool _isContentEchoed;
        private readonly HashSet<string> _headersWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CgiBehavior()
        {
            Encoding = new UTF8Encoding();
        }

        public override int Execute()
        {
            string scriptFile;
            scriptFile = Environment.GetEnvironmentVariable("PATH_TRANSLATED");
            if (scriptFile == null)
            {
                scriptFile = Environment.GetEnvironmentVariable("SCRIPT_FILENAME");
            }

            if (scriptFile == null)
            {
                Header("Content-type", "text/plain");
                Echo("No CGI Variables found");
                return 1;
            }

            if (!System.IO.File.Exists(scriptFile))
            {
                Header("Content-type", "text/plain");
                Echo(String.Format("Script file not found: {0}", scriptFile));
                return 1;
            }

            return RunCGIMode(scriptFile);

        }

        private int RunCGIMode(string scriptFile)
        {
            var engine = new HostedScriptEngine();
            engine.CustomConfig = ScriptFileHelper.CustomConfigPath(scriptFile);
            engine.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            var request = new Web.WebRequestContext();
            engine.InjectGlobalProperty("ВебЗапрос", request, true);
            engine.InjectGlobalProperty("WebRequest", request, true);
            engine.InjectObject(this, false);

            ScriptFileHelper.OnBeforeScriptRead(engine);
            var source = engine.Loader.FromFile(scriptFile);
            
            Process process;

            try
            {
                process = engine.CreateProcess(this, source);
            }
            catch (Exception e)
            {
                ShowExceptionInfo(e);
                return 1;
            }

            int exitCode = process.Start();
            
            if (!_isContentEchoed)
                Echo("");

            return exitCode;
        }

        #region CGIHost

        [ContextMethod("ВывестиЗаголовок", "Header")]
        public void Header(string header, string value)
        {
            if (_isContentEchoed)
                throw new InvalidOperationException("Headers can not be written after the main content");

            Output(header + ": " + value + "\r\n");
            _headersWritten.Add(header);
        }

        public Encoding Encoding { get; set; }

        private bool IsHeaderWritten(string header)
        {
            return _headersWritten.Contains(header);
        }

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            if(!_isContentEchoed)
            {
                if(!IsHeaderWritten("Content-type"))
                    Header("Content-type", "text/html");
                if (!IsHeaderWritten("Content-encoding"))
                    Header("Content-encoding", Encoding.BodyName);
                oscript.Output.WriteLine();

                _isContentEchoed = true;
            }

            if (str != "")
            {
                Output (str);
                oscript.Output.WriteLine();
            }
        }

        private void Output(string str)
        {
            using (var stream = Console.OpenStandardOutput())
            {
                var bytes = Encoding.GetBytes(str);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            Echo(exc.ToString());
        }

        public bool InputString(out string result, int maxLen)
        {
            result = null;
            return false;
        }

        public string[] GetCommandLineArguments()
        {
            return new string[0];
        }

        #endregion

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods, out IRuntimeContextInstance instance)
        {
            variables = new IVariable[0];
            methods = (MethodInfo[])GetMethods();
            instance = this;
        }

        public IEnumerable<VariableInfo> GetProperties()
        {
            return new VariableInfo[0];
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
            throw RuntimeException.PropNotFoundException(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return false;
        }

        public bool IsPropWritable(int propNum)
        {
            return false;
        }

        public IValue GetPropValue(int propNum)
        {
            throw new ArgumentException();
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new InvalidOperationException("global props are not writable");
        }

        public int GetPropCount()
        {
            return 0;
        }

        public string GetPropName(int index)
        {
            throw new ArgumentOutOfRangeException();
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

        private static ContextMethodsMapper<CgiBehavior> _methods = new ContextMethodsMapper<CgiBehavior>();
    }
}

using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    /// <summary>
    /// Менеджер потоков, выполняющих скрипты. Предназначен для того, чтобы внутри реализации
    /// класса контекста получать runtime информацию из какого скрипта вызван тот или иной C# код.
    /// </summary>
    public class ScriptThread
    {
        private static object _lockHolder = new object();
        private static Dictionary<int, ScriptThread> _threads = new Dictionary<int, ScriptThread>();

        private IScriptEngine _engine;

        internal ScriptThread()
        {
        }

        static ScriptThread()
        {
        }

        public IValue Run(Func<IValue> runner)
        {
            IValue result;
            try
            {
                if (ScriptThread.Current != null)
                    throw new InvalidOperationException("This thread already running a script");

                ScriptThread.Current = this;
                result = runner();
            }
            finally
            {
                ScriptThread.Current = null;
            }

            return result;
        }

        public ILoadedModule Module { get { throw new NotImplementedException(); } }
        
        //public ICallStack CallStack { get { throw new NotImplementedException(); } }

        public IScriptEngine CurrentEngineInstance { get { return _engine; } }

        public static ScriptThread Create(IScriptEngine engine)
        {
            var thread = new ScriptThread();
            thread._engine = engine;
            return thread;
        }

        public static ScriptThread Current 
        { 
            get
            {
                int currentCLRThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                lock(_lockHolder)
                {
                    ScriptThread outVal;
                    _threads.TryGetValue(currentCLRThreadID, out outVal);
                    return outVal;
                }
            }
            internal set
            {
                int currentCLRThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                lock (_lockHolder)
                {
                    if(value == null)
                    {
                        _threads.Remove(currentCLRThreadID);
                    }
                    else
                    {
                        _threads.Add(currentCLRThreadID, value);
                    }
                }
            }
        }

    }
}

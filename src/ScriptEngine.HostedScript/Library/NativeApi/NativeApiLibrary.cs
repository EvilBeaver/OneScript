using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    class NativeApiLibrary: NativeApiKernel
    {
        private delegate IntPtr GetClassNames();

        public static bool Register(String filepath, String identifier)
        {
            if (_libraries.ContainsKey(identifier)) return false;
            NativeApiLibrary instance = new NativeApiLibrary(filepath, identifier);
            if (instance.Loaded) _libraries.Add(identifier, instance);
            return instance.Loaded;
        }

        private static Dictionary<string, NativeApiLibrary> _libraries = new Dictionary<string, NativeApiLibrary>();
        private readonly String _filepath = String.Empty;
        private readonly IntPtr _module = IntPtr.Zero;

        internal static void Initialize()
        {
            _libraries = new Dictionary<string, NativeApiLibrary>();
        }

        public NativeApiLibrary(String filepath, String identifier) {
            _filepath = filepath;
            _module = LoadLibrary(filepath);
            if (Loaded) RegisterComponents(identifier);
        }

        ~NativeApiLibrary()
        {
            if (Loaded) FreeLibrary(_module);
        }

        public IntPtr Module
        {
            get => _module;
        }

        private Boolean Loaded {
            get => _module != IntPtr.Zero;
        }

        private void RegisterComponents(String identifier) {
            IntPtr funcPtr = GetProcAddress(_module, "GetClassNames");
            if (funcPtr == IntPtr.Zero) return;
            GetClassNames func = (GetClassNames)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(GetClassNames));
            IntPtr namesPtr = func();
            if (namesPtr == IntPtr.Zero) return;
            String names = NativeApiProxy.Str(namesPtr);
            if (String.IsNullOrWhiteSpace(names)) return;
            char[] separator = new char[] { '|' };
            String[] nameArray = names.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (String name in nameArray)
            {
                TypeManager.RegisterType($"AddIn.{identifier}.{name}", typeof(NativeApiLibrary));
            }
        }

        [Machine.Contexts.ScriptConstructor(ParametrizeWithClassName = true)]
        public static IValue Constructor(String typeName)
        {
            char[] separator = new char[] { '.' };
            String[] names = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 3 && _libraries.TryGetValue(names[1], out NativeApiLibrary library))
            {
                NativeApiComponent comp = new NativeApiComponent(library, names[2]);
                return ValueFactory.Create(comp);
            }
            throw new NotImplementedException();
        }
    }
}

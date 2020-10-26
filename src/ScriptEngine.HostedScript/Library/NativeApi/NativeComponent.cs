using ScriptEngine.Machine;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    class NativeApiComponent: IRuntimeContextInstance
    {
        private readonly IntPtr ptr;

        public static String[] ClassNames(String library)
        {
            IntPtr ptr = NativeApiProxy.GetClassNames(library);
            String names = NativeApiProxy.Str(ptr);
            char[] separator = new char[] { '|' };
            return names.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        public NativeApiComponent(String library, String component)
        {
            ptr = NativeApiProxy.GetClassObject(library, component);
        }

        ~NativeApiComponent()
        {
            NativeApiProxy.DestroyObject(ptr);
        }

        public bool IsIndexed => false;
        public bool DynamicMethodSignatures => false;

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
            return (int)NativeApiProxy.FindProp(ptr, name);
        }

        public bool IsPropReadable(int propNum)
        {
            return NativeApiProxy.IsPropReadable(ptr, propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return NativeApiProxy.IsPropWritable(ptr, propNum);
        }

        public int GetPropCount()
        {
            return (int)NativeApiProxy.GetNProps(ptr);
        }

        public string GetPropName(int propNum)
        {
            string name = String.Empty;
            NativeApiProxy.GetPropName(ptr, propNum, 0, n => name = NativeApiProxy.Str(n));
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            throw new ArgumentException();
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new NotImplementedException();
        }

        public int GetMethodsCount()
        {
            return (int)NativeApiProxy.GetNMethods(ptr);
        }

        public int FindMethod(string name)
        {
            return (int)NativeApiProxy.FindMethod(ptr, name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            String name = String.Empty;
            string alias = String.Empty;
            NativeApiProxy.GetMethodName(ptr, methodNumber, 0, s => name = NativeApiProxy.Str(s));
            NativeApiProxy.GetMethodName(ptr, methodNumber, 1, s => alias = NativeApiProxy.Str(s));
            return new MethodInfo
            {
                Name = name,
                Alias = alias,
                IsFunction = NativeApiProxy.HasRetVal(ptr, methodNumber),
                IsDeprecated = false,
                IsExport = false,
                ThrowOnUseDeprecated = false,
            };
          
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            throw new NotImplementedException();
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            throw new NotImplementedException();
        }
    }

}
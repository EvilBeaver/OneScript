using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    class NativeApiComponent : NativeApiValue, IRuntimeContextInstance, IValue
    {
        private readonly IntPtr _object;

        public override IRuntimeContextInstance AsObject()
        {
            return this;
        }

        public NativeApiComponent(NativeApiLibrary library, String typeName, String component)
        {
            _object = NativeApiProxy.GetClassObject(library.Module, component);
            DefineType(TypeManager.GetTypeByName(typeName));
        }

        ~NativeApiComponent()
        {
            NativeApiProxy.DestroyObject(_object);
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
            return (int)NativeApiProxy.FindProp(_object, name);
        }

        public bool IsPropReadable(int propNum)
        {
            return NativeApiProxy.IsPropReadable(_object, propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return NativeApiProxy.IsPropWritable(_object, propNum);
        }

        public int GetPropCount()
        {
            return (int)NativeApiProxy.GetNProps(_object);
        }

        public string GetPropName(int propNum)
        {
            string name = String.Empty;
            NativeApiProxy.GetPropName(_object, propNum, 0, n => name = NativeApiProxy.Str(n));
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            IValue result = ValueFactory.Create();
            NativeApiProxy.GetPropVal(_object, propNum, var => result = NativeApiVariant.GetValue(var));
            return result;
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            NativeApiVariant variant = new NativeApiVariant();
            variant.SetValue(newVal);
            NativeApiProxy.SetPropVal(_object, propNum, var => NativeApiVariant.SetValue(var, newVal));
        }

        public int GetMethodsCount()
        {
            return (int)NativeApiProxy.GetNMethods(_object);
        }

        public int FindMethod(string name)
        {
            return (int)NativeApiProxy.FindMethod(_object, name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            String name = String.Empty;
            string alias = String.Empty;
            NativeApiProxy.GetMethodName(_object, methodNumber, 0, s => name = NativeApiProxy.Str(s));
            NativeApiProxy.GetMethodName(_object, methodNumber, 1, s => alias = NativeApiProxy.Str(s));
            return new MethodInfo
            {
                Name = name,
                Alias = alias,
                IsFunction = NativeApiProxy.HasRetVal(_object, methodNumber),
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
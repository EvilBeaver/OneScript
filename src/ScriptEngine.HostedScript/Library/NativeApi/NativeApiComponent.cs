/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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

        public NativeApiComponent(NativeApiLibrary library, String typeName, String componentName)
        {
            _object = NativeApiProxy.GetClassObject(library.Module, componentName);
            DefineType(TypeManager.GetTypeByName(typeName));
        }

        public void Dispose()
        {
            try { NativeApiProxy.DestroyObject(_object); } catch (Exception) { }
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
            var name = String.Empty;
            NativeApiProxy.GetPropName(_object, propNum, 0,
                n => name = NativeApiProxy.Str(n)
            );
            return name;
        }

        public IValue GetPropValue(int propNum)
        {
            var result = ValueFactory.Create();
            NativeApiProxy.GetPropVal(_object, propNum,
                variant => result = NativeApiVariant.GetValue(variant)
            );
            return result;
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            var variant = new NativeApiVariant();
            variant.SetValue(newVal);
            NativeApiProxy.SetPropVal(_object, propNum, ref variant);
            variant.Clear();
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
            var name = String.Empty;
            var alias = String.Empty;
            NativeApiProxy.GetMethodName(_object, methodNumber, 0,
                str => name = NativeApiProxy.Str(str)
            );
            NativeApiProxy.GetMethodName(_object, methodNumber, 1,
                str => alias = NativeApiProxy.Str(str)
            );
            var paramCount = NativeApiProxy.GetNParams(_object, methodNumber);
            var paramArray = new ParameterDefinition[paramCount];
            for (int i = 0; i < paramCount; i++)
                NativeApiProxy.GetParamDefValue(_object, methodNumber, i, variant =>
                {
                    if (NativeApiVariant.NotEmpty(variant))
                    {
                        paramArray[i].HasDefaultValue = true;
                        paramArray[i].DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                    }
                });

            return new MethodInfo
            {
                Name = name,
                Alias = alias,
                IsFunction = NativeApiProxy.HasRetVal(_object, methodNumber),
                IsDeprecated = false,
                IsExport = false,
                ThrowOnUseDeprecated = false,
                Params = paramArray,
            };
        }

        private void SetDefValues(int methodNumber, int paramCount, IValue[] arguments)
        {
            for (int i = 0; i < paramCount; i++)
                if (arguments[i] == null)
                    NativeApiProxy.GetParamDefValue(_object, methodNumber, i,
                        variant => arguments[i] = NativeApiVariant.GetValue(variant)
                    );
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var paramArray = IntPtr.Zero;
            int paramCount = (int)NativeApiProxy.GetNParams(_object, methodNumber);
            if (paramCount > 0)
                paramArray = Marshal.AllocHGlobal(NativeApiVariant.Size * paramCount);
            SetDefValues(methodNumber, paramCount, arguments);
            NativeApiVariant.SetValue(paramArray, arguments, paramCount);
            NativeApiProxy.CallAsProc(_object, methodNumber, paramArray);
            NativeApiVariant.Clear(paramArray, paramCount);
            Marshal.FreeHGlobal(paramArray);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var paramArray = IntPtr.Zero;
            int paramCount = (int)NativeApiProxy.GetNParams(_object, methodNumber);
            if (paramCount > 0)
                paramArray = Marshal.AllocHGlobal(NativeApiVariant.Size * paramCount);
            SetDefValues(methodNumber, paramCount, arguments);
            NativeApiVariant.SetValue(paramArray, arguments, paramCount);
            IValue result = retValue = ValueFactory.Create();
            bool ok = NativeApiProxy.CallAsFunc(_object, methodNumber, paramArray,
                variant => result = NativeApiVariant.GetValue(variant)
            );
            NativeApiVariant.Clear(paramArray, paramCount);
            Marshal.FreeHGlobal(paramArray);
            if (ok)
                retValue = result;
        }
    }
}

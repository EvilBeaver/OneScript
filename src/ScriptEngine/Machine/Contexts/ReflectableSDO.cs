/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Environment;
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class ReflectableSDO : IValue, IRuntimeContextInstance, IReflect
    {
        readonly ScriptDrivenObject _instance;

        readonly LoadedModule _module;
        Dictionary<string, int> _reflectedMethods;
        Dictionary<string, int> _reflectedProperties;
        ReflectedMethodInfo[] _methodsCache;
        ReflectedPropertyInfo[] _propsCache;

        public ReflectableSDO(ScriptDrivenObject instance, LoadedModuleHandle module)
            : this(instance, module.Module)
        {
        }

        internal ReflectableSDO(ScriptDrivenObject instance, LoadedModule module)
        {
            _module = module;
            _instance = instance;

            GatherProperties();
            GatherMethods();

        }

        protected virtual object InvokeInternal(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, System.Globalization.CultureInfo culture)
        {
            if (invokeAttr.HasFlag(BindingFlags.GetProperty))
            {
                var prop = GetProperty(name, invokeAttr);
                if (prop == null)
                {
                    throw new MissingFieldException("ScriptDrivenObject", name);
                }

                return prop.GetValue(target, null);

            }
            else if (invokeAttr.HasFlag(BindingFlags.SetProperty))
            {
                var prop = GetProperty(name, invokeAttr);
                if (prop == null)
                {
                    throw new MissingFieldException("ScriptDrivenObject", name);
                }

                prop.SetValue(target, args[0], invokeAttr, binder, null, culture);

                return null;
            }
            else if (invokeAttr.HasFlag(BindingFlags.InvokeMethod))
            {
                var method = GetMethod(name, invokeAttr);
                if (method == null)
                {
                    throw new MissingMethodException("ScriptDrivenObject", name);
                }

                return method.Invoke(target, invokeAttr, binder, args, culture);
            }
            else
            {
                throw new ArgumentException("No flags for property access or method call");
            }
        }

        private void GatherProperties()
        {
            var props = _module.ExportedProperies;
            
            _reflectedProperties = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            _propsCache = new ReflectedPropertyInfo[props.Length];
            
            for (int i = 0; i < props.Length; i++)
            {
                var reflected = CreatePropInfo(props[i]);
                _propsCache[i] = (ReflectedPropertyInfo)reflected;
                _reflectedProperties.Add(props[i].SymbolicName, i);
                _reflectedProperties.Add("[DispId=" + props[i].Index + "]", i);
            }
            
        }

        private void GatherMethods()
        {
            _methodsCache = new ReflectedMethodInfo[_module.ExportedMethods.Length];
            _reflectedMethods = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < _module.ExportedMethods.Length; i++)
            {
                var item = _module.ExportedMethods[i];
                var reflected = CreateMethodInfo(item);
                _methodsCache[i] = reflected;
                _reflectedMethods.Add(item.SymbolicName, i);
                _reflectedMethods.Add("[DispId=" + item.Index + "]", i);
            }
        }

        #region IReflect Members

        public FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return null; //throw new NotImplementedException();
        }

        public FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return null; //throw new NotImplementedException();
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public System.Reflection.MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            int index;
            if (!_reflectedMethods.TryGetValue(name, out index))
            {
                return null;
            }

            return _methodsCache[index];
        }

        public System.Reflection.MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return GetMethod(name, bindingAttr);
        }

        public System.Reflection.MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return _methodsCache.ToArray();
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return _propsCache.ToArray();
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return GetProperty(name, bindingAttr);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            int index;
            if (!_reflectedProperties.TryGetValue(name, out index))
            {
                return null;
            }

            return _propsCache[index];
        }

        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            return InvokeInternal(name, invokeAttr, binder, target, args, culture);
        }

        public Type UnderlyingSystemType
        {
            get { return this.GetType(); }
        }

        // helpers
        private PropertyInfo CreatePropInfo(ExportedSymbol prop)
        {
            var pi = new ReflectedPropertyInfo(prop.SymbolicName);
            pi.SetDispId(prop.Index);
            return pi;
        }

        private ReflectedMethodInfo CreateMethodInfo(ExportedSymbol engineMethod)
        {
            var methInfo = _module.Methods[engineMethod.Index].Signature;
            var reflectedMethod = new ReflectedMethodInfo(methInfo.Name);
            reflectedMethod.SetDispId(engineMethod.Index);
            reflectedMethod.IsFunction = methInfo.IsFunction;
            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var currentParam = methInfo.Params[i];
                var reflectedParam = new ReflectedParamInfo("param" + i.ToString(), currentParam.IsByValue);
                reflectedParam.SetOwner(reflectedMethod);
                reflectedParam.SetPosition(i);
                reflectedMethod.Parameters.Add(reflectedParam);
            }

            return reflectedMethod;

        }

        #endregion

        #region IValue Members

        public DataType DataType
        {
            get { return _instance.DataType; }
        }

        public TypeDescriptor SystemType
        {
            get { return _instance.SystemType; }
        }

        public decimal AsNumber()
        {
            return _instance.AsNumber();
        }

        public DateTime AsDate()
        {
            return _instance.AsDate();
        }

        public bool AsBoolean()
        {
            return _instance.AsBoolean();
        }

        public string AsString()
        {
            return _instance.AsString();
        }

        public IRuntimeContextInstance AsObject()
        {
            return this;
        }

        public IValue GetRawValue()
        {
            return this;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            return _instance.CompareTo(other);
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            return _instance.Equals(other);
        }

        #endregion

        #region IRuntimeContextInstance Members

        public bool IsIndexed
        {
            get { return _instance.IsIndexed; }
        }

        public bool DynamicMethodSignatures
        {
            get { return _instance.DynamicMethodSignatures; }
        }

        public IValue GetIndexedValue(IValue index)
        {
            return _instance.GetIndexedValue(index);
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            _instance.SetIndexedValue(index, val);
        }

        public int FindProperty(string name)
        {
            return _instance.FindProperty(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return _instance.IsPropReadable(propNum);
        }

        public bool IsPropWritable(int propNum)
        {
            return _instance.IsPropWritable(propNum);
        }

        public IValue GetPropValue(int propNum)
        {
            return _instance.GetPropValue(propNum);
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            _instance.SetPropValue(propNum, newVal);
        }

        public int GetPropCount()
        {
            return _instance.GetPropCount();
        }

        public string GetPropName(int propNum)
        {
            return _instance.GetPropName(propNum);
        }

        public IEnumerable<MethodInfo> GetMethods()
        {
            return _instance.GetMethods();
        }

        public int FindMethod(string name)
        {
            return _instance.FindMethod(name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _instance.GetMethodInfo(methodNumber);
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _instance.CallAsProcedure(methodNumber, arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            _instance.CallAsFunction(methodNumber, arguments, out retValue);
        }

        #endregion
    }

}
//#endif
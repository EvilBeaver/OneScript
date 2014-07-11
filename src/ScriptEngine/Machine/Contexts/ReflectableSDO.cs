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
        ScriptDrivenObject _instance;
        LoadedModule _module;

        public ReflectableSDO(ScriptDrivenObject instance, LoadedModuleHandle module)
            : this(instance, module.Module)
        {
        }

        internal ReflectableSDO(ScriptDrivenObject instance, LoadedModule module)
        {
            _module = module;
            _instance = instance;
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
            var allMethods = GetMethods(bindingAttr);
            return allMethods.FirstOrDefault(x => String.Compare(x.Name, name, true) == 0);
        }

        public System.Reflection.MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return GetMethod(name, bindingAttr);
        }

        public System.Reflection.MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            var result = new System.Reflection.MethodInfo[_module.ExportedMethods.Length];
            int i = 0;
            foreach (var item in _module.ExportedMethods)
            {
                var signature = _module.Methods[item.Index].Signature;
                var reflected = CreateMethodInfo(signature, false);
                reflected.SetDispId(item.Index);
                result[i++] = reflected;
            }

            return result;
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            var props = _module.ExportedProperies;
            var resultPropInfo = new PropertyInfo[props.Length];
            for (int i = 0; i < resultPropInfo.Length; i++)
            {
                resultPropInfo[i] = CreateExportedPropInfo(props[i]);
            }

            return resultPropInfo;
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return GetProperty(name, bindingAttr);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            var collection = _module.ExportedProperies.Where(x => String.Compare(x.SymbolicName, name, true) == 0);
            PropertyInfo result = null;
            foreach (var item in collection)
            {
                result = CreateExportedPropInfo(item);
                break;
            }

            return result;
        }

        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
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

        public Type UnderlyingSystemType
        {
            get { return this.GetType(); }
        }

        // helpers
        private static PropertyInfo CreateExportedPropInfo(ExportedSymbol prop)
        {
            var pi = new ReflectedPropertyInfo(prop.SymbolicName);
            pi.SetDispId(prop.Index);
            return pi;
        }

        private static ReflectedMethodInfo CreateMethodInfo(ScriptEngine.Machine.MethodInfo engineMethod, bool asPrivate)
        {
            var reflectedMethod = new ReflectedMethodInfo(engineMethod.Name, asPrivate);
            reflectedMethod.IsFunction = engineMethod.IsFunction;
            for (int i = 0; i < engineMethod.Params.Length; i++)
            {
                var currentParam = engineMethod.Params[i];
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

        public double AsNumber()
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

        public TypeDescriptor AsType()
        {
            return _instance.AsType();
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

    class ReflectedPropertyInfo : PropertyInfo
    {
        bool _canRead;
        bool _canWrite;
        string _name;
        int _dispId;

        public ReflectedPropertyInfo(string name) : base()
        {
            _name = name;
            SetReadable(true);
            SetWritable(true);
        }

        internal void SetReadable(bool val)
        {
            _canRead = val;
        }
        internal void SetWritable(bool val)
        {
            _canWrite = val;
        }

        public override PropertyAttributes Attributes
        {
            get { return PropertyAttributes.None; }
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            IRuntimeContextInstance inst = obj as IRuntimeContextInstance;
            if (inst == null)
                throw new ArgumentException("Wrong argument type");

            IValue retVal = inst.GetPropValue(_dispId);

            return COMWrapperContext.MarshalIValue(retVal);
        }

        public override Type PropertyType
        {
            get { return typeof(IValue); }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            IRuntimeContextInstance inst = obj as IRuntimeContextInstance;
            if (inst == null)
                throw new ArgumentException("Wrong argument type");

            inst.SetPropValue(_dispId, COMWrapperContext.CreateIValue(value));
        }

        public override Type DeclaringType
        {
            get { return typeof(ReflectableSDO); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(DispIdAttribute))
            {
                return this.GetCustomAttributes(inherit);
            }
            else
            {
                return new object[0];
            }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            DispIdAttribute[] attribs = new DispIdAttribute[1];
            attribs[0] = new DispIdAttribute(_dispId);
            return attribs;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(DispIdAttribute);
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type ReflectedType
        {
            get { return typeof(ReflectableSDO); }
        }

        internal void SetDispId(int p)
        {
            _dispId = p;
        }
    }

    class ReflectedMethodInfo : System.Reflection.MethodInfo
    {
        string _name;
        int _dispId;
        bool _isPrivate;

        List<ParameterInfo> _parameters;

        public ReflectedMethodInfo(string name, bool isPrivate)
        {
            _name = name;
            _isPrivate = isPrivate;
            _parameters = new List<ParameterInfo>();
        }

        internal void SetDispId(int p)
        {
            _dispId = p;
        }

        public List<ParameterInfo> Parameters 
        {
            get
            {
                return _parameters;
            }
        }

        public bool IsFunction { get; set; }

        public override System.Reflection.MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public override MethodAttributes Attributes
        {
            get 
            {
                if (_isPrivate)
                {
                    return MethodAttributes.Private;
                }
                else
                {
                    return MethodAttributes.Public;
                }
            }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            return _parameters.ToArray();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            IRuntimeContextInstance inst = obj as IRuntimeContextInstance;
            if (inst == null)
                throw new ArgumentException("Wrong argument type");

            IValue[] engineParameters = parameters.Select(x => COMWrapperContext.CreateIValue(x)).ToArray();
            IValue retVal = null;

            inst.CallAsFunction(_dispId, engineParameters, out retVal);

            return COMWrapperContext.MarshalIValue(retVal);
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override Type DeclaringType
        {
            get { return typeof(ReflectableSDO); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(DispIdAttribute))
            {
                return this.GetCustomAttributes(inherit);
            }
            else
            {
                return new object[0];
            }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            DispIdAttribute[] attribs = new DispIdAttribute[1];
            attribs[0] = new DispIdAttribute(_dispId);
            return attribs;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(DispIdAttribute);
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type ReflectedType
        {
            get { return typeof(ReflectableSDO); }
        }
    }

    class ReflectedParamInfo : ParameterInfo
    {
        public ReflectedParamInfo(string name, bool isByVal)
        {
            NameImpl = name;
            AttrsImpl = ParameterAttributes.In;
            if (!isByVal)
            {
                AttrsImpl |= ParameterAttributes.Out;
            }

            ClassImpl = typeof(IValue);

        }

        public void SetOwner(MemberInfo owner)
        {
            MemberImpl = owner;
        }

        public void SetPosition(int index)
        {
            PositionImpl = index;
        }

    }

}

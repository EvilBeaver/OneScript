#if !__MonoCS__ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ScriptEngine.Machine.Contexts
{
   
	class ReflectedMethodInfo : System.Reflection.MethodInfo
    {
        string _name;
        int _dispId;
        bool _isPrivate;

        List<ParameterInfo> _parameters;

        public ReflectedMethodInfo(string name)
        {
            _name = name;
            _parameters = new List<ParameterInfo>();
        }

        internal void SetDispId(int p)
        {
            _dispId = p;
        }

        internal void SetPrivate(bool makePrivate)
        {
            _isPrivate = makePrivate;
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
}
#endif
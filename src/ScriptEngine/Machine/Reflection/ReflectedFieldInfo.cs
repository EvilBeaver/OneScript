using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public class ReflectedFieldInfo : FieldInfo
    {
        private Type _declaringType;
        private bool _isPublic;
        private readonly VariableInfo _info;

        public ReflectedFieldInfo(VariableInfo info, bool isPublic)
        {
            _info = info;
            _isPublic = isPublic;
        }
        
        public void SetDeclaringType(Type declType)
        {
            _declaringType = declType;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override object GetValue(object obj)
        {
            var irc = obj as IRuntimeContextInstance;
            if(irc == null)
                throw new ArgumentException();

            return ContextValuesMarshaller.ConvertReturnValue(irc.GetPropValue(_info.Index));
        }
        
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            var irc = obj as IRuntimeContextInstance;
            if (irc == null)
                throw new ArgumentException();

            irc.SetPropValue(_info.Index, (IValue) value);
        }

        public override string Name
        {
            get { return _info.Identifier; }
        }

        public override Type DeclaringType
        {
            get { return _declaringType; }
        }

        public override Type ReflectedType
        {
            get { return _declaringType; }
        }

        public override Type FieldType
        {
            get { return typeof(IValue); }
        }

        public override FieldAttributes Attributes
        {
            get { return _isPublic ? FieldAttributes.Public : FieldAttributes.Private; }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit);
        }
    }
}

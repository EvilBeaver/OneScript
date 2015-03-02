using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    class ManagedCOMWrapperContext : COMWrapperContext
    {
        private Type _instanceType;
        private object _instance;
        private bool? _isIndexed;

        private ComReflectionNameToIdMapper _nameMapper;

        public ManagedCOMWrapperContext(object instance)
        {
            _instanceType = instance.GetType();
            if (!_instanceType.IsCOMObject)
                throw new ArgumentException("Instance must be a COM-object");
            
            _instance = instance;
            _nameMapper = new ComReflectionNameToIdMapper(_instanceType);
        }

        public override bool IsIndexed
        {
            get
            {
                if (_isIndexed == null)
                {
                    _isIndexed = _instanceType.GetProperties().Any(x => x.GetIndexParameters().Length > 0);
                }     
                
                return (bool)_isIndexed;
            }
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            System.Collections.IEnumerator comEnumerator;

            try
            {

                comEnumerator = (System.Collections.IEnumerator)_instanceType.InvokeMember("GetEnumerator",
                                        BindingFlags.InvokeMethod,
                                        null,
                                        _instance,
                                        new object[0]);
            }
            catch (MissingMethodException)
            {
                throw RuntimeException.IteratorIsNotDefined();
            }

            while (comEnumerator.MoveNext())
            {
                yield return CreateIValue(comEnumerator.Current);
            }
        }

        public override object UnderlyingObject
        {
            get { return _instance; }
        }

        public override int FindProperty(string name)
        {
            return _nameMapper.FindProperty(name);
        }

        public override bool IsPropReadable(int propNum)
        {
            PropertyInfo pi = _nameMapper.GetProperty(propNum);
            return pi.CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            PropertyInfo pi = _nameMapper.GetProperty(propNum);
            return pi.CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            var pi = _nameMapper.GetProperty(propNum);
            var result = pi.GetGetMethod().Invoke(_instance, null);
            return CreateIValue(result);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var pi = _nameMapper.GetProperty(propNum);
            pi.GetSetMethod().Invoke(_instance, new[] { MarshalIValue(newVal) });
        }
    }
}

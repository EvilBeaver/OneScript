using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OneScript.Core;

namespace OneScript.ComponentModel
{
    public class PropertyTarget<TInstance>
    {
        private Func<TInstance, IValue> _getter;
        private Action<TInstance, IValue> _setter;
        private string _name;
        private string _alias;

        public PropertyTarget(PropertyInfo propInfo)
        {
            var attrib = (ContextPropertyAttribute)propInfo.GetCustomAttributes(typeof(ContextPropertyAttribute), false)[0];
            _name = attrib.Name == null ? propInfo.Name : attrib.Name;
            _alias = attrib.Alias == null ? "" : attrib.Alias;

            Func<TInstance, IValue> cantReadAction = (inst) => { throw ContextAccessException.PropIsNotReadable(_name); };
            Action<TInstance, IValue> cantWriteAction = (inst, val) => { throw ContextAccessException.PropIsNotWritable(_name); };

            this.CanRead = attrib.CanRead;
            this.CanWrite = attrib.CanWrite;

            if (attrib.CanRead)
            {
                var getMethodInfo = propInfo.GetGetMethod();
                if (getMethodInfo == null)
                {
                    _getter = cantReadAction;
                }
                else
                {
                    var genericGetter = typeof(PropertyTarget<TInstance>).GetMembers(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .Where(x => x.MemberType == System.Reflection.MemberTypes.Method && x.Name == "CreateGetter")
                        .Select(x => (System.Reflection.MethodInfo)x)
                        .First();

                    var resolvedGetter = genericGetter.MakeGenericMethod(new Type[] { propInfo.PropertyType });

                    _getter = (Func<TInstance, IValue>)resolvedGetter.Invoke(this, new object[] { getMethodInfo });
                }
            }
            else
            {
                _getter = cantReadAction;
            }

            if (attrib.CanWrite)
            {
                var setMethodInfo = propInfo.GetSetMethod();
                if (setMethodInfo == null)
                {
                    _setter = cantWriteAction;
                }
                else
                {
                    var genericSetter = typeof(PropertyTarget<TInstance>).GetMembers(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .Where(x => x.MemberType == System.Reflection.MemberTypes.Method && x.Name == "CreateSetter")
                        .Select(x => (System.Reflection.MethodInfo)x)
                        .First();

                    var resolvedSetter = genericSetter.MakeGenericMethod(new Type[] { propInfo.PropertyType });

                    _setter = (Action<TInstance, IValue>)resolvedSetter.Invoke(this, new object[] { setMethodInfo });
                }
            }
            else
            {
                _setter = cantWriteAction;
            }
        }

        public Func<TInstance, IValue> Getter
        {
            get { return _getter; }
        }

        public Action<TInstance, IValue> Setter
        {
            get { return _setter; }
        }
        
        public string Name
        {
            get { return _name; }
        }

        public string Alias
        {
            get { return _alias; }
        }

        public bool CanRead { get; private set; }
        public bool CanWrite { get; private set; }

        private Func<TInstance, IValue> CreateGetter<T>(System.Reflection.MethodInfo methInfo)
        {
            var method = (Func<TInstance, T>)Delegate.CreateDelegate(typeof(Func<TInstance, T>), methInfo);
            return (inst) => ConvertReturnValue(method(inst));
        }

        private Action<TInstance, IValue> CreateSetter<T>(System.Reflection.MethodInfo methInfo)
        {
            var method = (Action<TInstance, T>)Delegate.CreateDelegate(typeof(Action<TInstance, T>), methInfo);
            return (inst, val) => method(inst, ConvertParam<T>(val));
        }

        private T ConvertParam<T>(IValue value)
        {
            return ValueMarshaler.IValueToCLRType<T>(value);
        }

        private IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ValueMarshaler.CLRTypeToIValue(param);
        }

    }

    public class ContextPropertyMapper<TInstance>
    {
        private List<PropertyTarget<TInstance>> _properties;

        public ContextPropertyMapper()
        {
            FindProperties();
        }

        private void FindProperties()
        {
            _properties = typeof(TInstance).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Any())
                .Select(x => new PropertyTarget<TInstance>(x)).ToList();
        }

        public int FindProperty(string name)
        {
            var idx = _properties.FindIndex(x => x.Name.ToLower() == name.ToLower() || x.Alias.ToLower() == name.ToLower());
            if (idx < 0)
                throw ContextAccessException.PropNotFound(name);

            return idx;
        }

        public PropertyTarget<TInstance> GetProperty(int index)
        {
            return _properties[index];
        }

        public int Count
        {
            get
            {
                return _properties.Count;
            }
        }

    }
}

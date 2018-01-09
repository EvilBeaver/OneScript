/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ContextPropertyAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextPropertyAttribute(string name, string alias = "")
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = name;
            _alias = alias;
            CanRead = true;
            CanWrite = true;
        }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }
        
    }

    public class PropertyTarget<TInstance>
    {
        private readonly Func<TInstance, IValue> _getter;
        private readonly Action<TInstance, IValue> _setter;
        private readonly string _name;
        private readonly string _alias;

        public PropertyTarget(PropertyInfo propInfo)
        {
            var attrib = (ContextPropertyAttribute)propInfo.GetCustomAttributes(typeof(ContextPropertyAttribute), false)[0];
            _name = attrib.GetName();
            _alias = attrib.GetAlias();
            if (string.IsNullOrEmpty(_alias))
                _alias = _name;

            Func<TInstance, IValue> cantReadAction = (inst) => { throw RuntimeException.PropIsNotReadableException(_name); };
            Action<TInstance, IValue> cantWriteAction = (inst, val) => { throw RuntimeException.PropIsNotWritableException(_name); };

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
            return ContextValuesMarshaller.ConvertParam<T>(value);
        }

        private IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ContextValuesMarshaller.ConvertReturnValue<TRet>(param);
        }

    }

    public class ContextPropertyMapper<TInstance>
    {
        private List<PropertyTarget<TInstance>> _properties;

        public void Init()
        {
            if (_properties != null) return;

            lock (this)
            {
                if(_properties == null)
                    FindProperties();
            }
        }

        private void FindProperties()
        {
            _properties = typeof(TInstance).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Any())
                .Select(x => new PropertyTarget<TInstance>(x)).ToList();
        }

        public int FindProperty(string name)
        {
            Init();
            var idx = _properties.FindIndex(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) 
                || String.Equals(x.Alias, name, StringComparison.OrdinalIgnoreCase));
            if (idx < 0)
                throw RuntimeException.PropNotFoundException(name);

            return idx;
        }

        public PropertyTarget<TInstance> GetProperty(int index)
        {
            Init();
            return _properties[index];
        }

        public int Count
        {
            get
            {
                Init();
                return _properties.Count;
            }
        }

        public IEnumerable<VariableInfo> GetProperties()
        {
            Init();
            var infos = new VariableInfo[Count];
            for (int i = 0; i < infos.Length; i++)
            {
                var prop = _properties[i];
                infos[i] = new VariableInfo()
                {
                    Identifier = prop.Name,
                    Alias = prop.Alias,
                    Type = SymbolType.ContextProperty,
                    Index = i
                };
            }

            return infos;
        }
    }
}

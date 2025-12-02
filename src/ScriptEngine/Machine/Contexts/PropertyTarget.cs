/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Exceptions;

namespace ScriptEngine.Machine.Contexts
{
    public class PropertyTarget<TInstance>
    {
        private readonly ContextPropertyInfo _propertyInfo;
        private volatile bool _deprecationIsWarned = false;

        internal PropertyTarget(ContextPropertyInfo propInfo)
        {
            _propertyInfo = propInfo;
            Name = _propertyInfo.Name;
            Alias = _propertyInfo.Alias;
            
            if (string.IsNullOrEmpty(Alias))
                Alias = propInfo.Name;

            IValue CantReadAction(TInstance inst)
            {
                throw PropertyAccessException.PropIsNotReadableException(Name);
            }

            void CantWriteAction(TInstance inst, IValue val)
            {
                throw PropertyAccessException.PropIsNotWritableException(Name);
            }

            if (_propertyInfo.CanRead)
            {
                var getMethodInfo = propInfo.GetGetMethod();
                if (getMethodInfo == null)
                {
                    Getter = CantReadAction;
                }
                else
                {
                    var genericGetter = typeof(PropertyTarget<TInstance>).GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.MemberType == MemberTypes.Method && x.Name == nameof(CreateGetter))
                        .Select(x => (MethodInfo)x)
                        .First();

                    var resolvedGetter = genericGetter.MakeGenericMethod(propInfo.PropertyType);

                    Getter = (Func<TInstance, IValue>)resolvedGetter.Invoke(this, new object[] { getMethodInfo });
                }
            }
            else
            {
                Getter = CantReadAction;
            }

            if (_propertyInfo.CanWrite)
            {
                var setMethodInfo = propInfo.GetSetMethod();
                if (setMethodInfo == null)
                {
                    Setter = CantWriteAction;
                }
                else
                {
                    var genericSetter = typeof(PropertyTarget<TInstance>).GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.MemberType == MemberTypes.Method && x.Name == nameof(CreateSetter))
                        .Select(x => (MethodInfo)x)
                        .First();

                    var resolvedSetter = genericSetter.MakeGenericMethod(propInfo.PropertyType);

                    Setter = (Action<TInstance, IValue>)resolvedSetter.Invoke(this, new object[] { setMethodInfo });
                }
            }
            else
            {
                Setter = CantWriteAction;
            }
        }
        
        public Func<TInstance, IValue> Getter { get; }

        public Action<TInstance, IValue> Setter { get; }

        public string Name { get; }

        public string Alias { get; }

        public bool CanRead => _propertyInfo.CanRead;
        public bool CanWrite => _propertyInfo.CanWrite;

        public BslPropertyInfo PropertyInfo => _propertyInfo;

        private Func<TInstance, IValue> CreateGetter<T>(MethodInfo methInfo)
        {
            var method = (Func<TInstance, T>)Delegate.CreateDelegate(typeof(Func<TInstance, T>), methInfo);

            if (_propertyInfo.IsForbiddenToUse)
            {
                return inst => throw RuntimeException.DeprecatedPropertyAccess(Name); 
            }
            
            if (_propertyInfo.IsDeprecated)
            {
                return (inst) =>
                {
                    if (!_deprecationIsWarned)
                    {
                        SystemLogger.Write($"ВНИМАНИЕ! Обращение к устаревшему свойству {Name}");
                        _deprecationIsWarned = true;
                    }
                    
                    return ConvertReturnValue(method(inst));
                };
            }
            else
            {
                return inst => ConvertReturnValue(method(inst));
            }
        }

        private Action<TInstance, IValue> CreateSetter<T>(MethodInfo methInfo)
        {
            var method = (Action<TInstance, T>)Delegate.CreateDelegate(typeof(Action<TInstance, T>), methInfo);

            if (_propertyInfo.IsForbiddenToUse)
            {
                return (inst, val) => throw RuntimeException.DeprecatedPropertyAccess(Name);
            }
            
            if (_propertyInfo.IsDeprecated)
            {
                return (inst, val) =>
                {
                    if (!_deprecationIsWarned)
                    {
                        SystemLogger.Write($"ВНИМАНИЕ! Обращение к устаревшему свойству {Name}");
                        _deprecationIsWarned = true;
                    }
                    
                    method(inst, ConvertParam<T>(val));
                };
            }
            else
            {
                return (inst, val) => method(inst, ConvertParam<T>(val));
            }
        }

        private static T ConvertParam<T>(IValue value)
        {
            return ContextValuesMarshaller.ConvertValueStrict<T>(value);
        }

        private static IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ContextValuesMarshaller.ConvertReturnValue(param);
        }

    }
}
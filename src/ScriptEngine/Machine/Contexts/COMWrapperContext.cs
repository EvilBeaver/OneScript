/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("COMОбъект", "COMObject", TypeUUID = "5E4FA60E-9724-494A-A5C8-5BB0A4F914E0")]
    public abstract class COMWrapperContext : PropertyNameIndexAccessor, 
        ICollectionContext,
        IEmptyValueCheck,
        IDisposable,
        IObjectWrapper,
        IEnumerable<IValue>
    {
        private static readonly DateTime MIN_OLE_DATE = new DateTime(100,1,1);
        protected static readonly TypeDescriptor ComObjectType = typeof(COMWrapperContext).GetTypeFromClassMarkup();
            
        protected object Instance;

        protected COMWrapperContext(object instance)
            : base(ComObjectType)
        {
            Instance = instance;
        }

        private static Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
            {
                var tt = assembly.GetType(typeName, throwOnError:false, ignoreCase:true);
                if (tt != null)
                {
                    return tt;
                }
            }
            return Type.GetType(typeName, throwOnError:false, ignoreCase:true);
        }

        private static COMWrapperContext Create(string progId, IValue[] arguments)
        {
            Type type = null;
#if NETFRAMEWORK
            if (!Utils.IsMonoRuntime)
            {
                type = Type.GetTypeFromProgID(progId, throwOnError: false);
            }
            if (type == null)
            {
                type = FindTypeByName(progId);
            }
#else
            type = FindTypeByName(progId);
            if (type == null)
            {
                type = Type.GetTypeFromProgID(progId, false);
            }
#endif
            if (type == null)
            {
                throw new TypeLoadException(String.Format("Тип {0} не найден!", progId));
            }

            if (type.IsGenericType)
            {
                // В первом приближении мы заполняем параметры шаблона классом Object
                // TODO: Продумать параметры шаблонного класса
                var genericTypes = new List<Type>();
                foreach (var ga in type.GetGenericArguments())
                {
                    genericTypes.Add(typeof(object));
                }
                type = type.MakeGenericType(genericTypes.ToArray());
            }

            object instance = Activator.CreateInstance(type, MarshalArguments(arguments));

            return InitByInstance(type, instance);
        }

        public static COMWrapperContext Create(object instance)
        {
            return InitByInstance(instance.GetType(), instance);
        }

        private static COMWrapperContext InitByInstance(Type type, object instance)
        {
            if (TypeIsRuntimeCallableWrapper(type))
            {               
                return new UnmanagedCOMWrapperContext(instance);
            }
            else if (IsObjectType(type) || IsAStruct(type))
            {
                return new ManagedCOMWrapperContext(instance);
            }
            else
                throw new ArgumentException(String.Format("Can't create COM wrapper for type {0}", type.ToString()));
        }

        private static bool IsObjectType(Type type)
        {
            return !type.IsPrimitive && !type.IsValueType;
        }

        private static bool IsAStruct(Type type)
        {
            return !type.IsPrimitive && type.IsValueType;
        }

        private static bool TypeIsRuntimeCallableWrapper(Type type)
        {
            return type.FullName == "System.__ComObject" || type.BaseType.FullName == "System.__ComObject"; // string, cause it's hidden type
        }

        public static object[] MarshalArguments(IValue[] arguments)
        {
            var args = arguments.Select(x => MarshalIValue(x)).ToArray();
            return args;
        }

        public static object MarshalIValue(IValue val)
        {
            object retValue;
            if (val != null && val is BslDateValue dateVal)
            {
                var date = (DateTime)dateVal;
                if (date <= MIN_OLE_DATE)
                {
                    retValue = MIN_OLE_DATE;
                }
                else
                {
                    retValue = date;
                }
            }
            else
            {
                retValue = ContextValuesMarshaller.ConvertToCLRObject(val);
            }

            return retValue;
        }

        protected static object[] MarshalArgumentsStrict(IValue[] arguments, Type[] argumentsTypes)
        {
            if (argumentsTypes.Length < arguments.Length)
                throw RuntimeException.TooManyArgumentsPassed();

            object[] marshalledArgs = new object[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                marshalledArgs[i] = ContextValuesMarshaller.ConvertParam(arguments[i], argumentsTypes[i]);
            }

            return marshalledArgs;
        }

        public static object[] MarshalArgumentsStrict(System.Reflection.MethodInfo method, IValue[] arguments)
        {
            var parameters = method.GetParameters();

            object[] marshalledArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if(i < arguments.Length)
                {
                    if (IsMissedArg(arguments[i]) && parameters[i].IsOptional)
                        marshalledArgs[i] = Type.Missing;
                    else
                        marshalledArgs[i] = ContextValuesMarshaller.ConvertParam(arguments[i], parameters[i].ParameterType);
                }
                else
                {
                    marshalledArgs[i] = Type.Missing;
                }
            }

            return marshalledArgs;
        }

        private static bool IsMissedArg(IValue arg)
        {
            return arg == null || arg.IsSkippedArgument();
        }

        public static IValue CreateIValue(object objParam)
        {
            if (objParam == null)
                return ValueFactory.Create();

            var type = objParam.GetType();
            if (typeof(IValue).IsAssignableFrom(type))
            {
                return (IValue)objParam;
            }
            else if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
            {
                return ValueFactory.Create(System.Convert.ToInt32(objParam));
            }
            else if(type == typeof(long) || type == typeof(ulong))
            {
                return ValueFactory.Create(System.Convert.ToInt64(objParam));
            }
            else if (type == typeof(double))
            {
                return ValueFactory.Create((decimal)(double)objParam);
            }
            else if (type == typeof(Single))
            {
                return ValueFactory.Create((decimal)System.Convert.ToDouble(objParam));
            }
            else if (type == typeof(decimal))
            {
                return ValueFactory.Create((decimal)objParam);
            }
            else if (type == typeof(DateTime))
            {
                var unboxed = (DateTime)objParam;
                if (unboxed == MIN_OLE_DATE)
                    unboxed = DateTime.MinValue;

                return ValueFactory.Create(unboxed);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if (type.IsArray)
            {
                return new SafeArrayWrapper(objParam);
            }
            else if (IsObjectType(type) || IsAStruct(type))
            {
                COMWrapperContext ctx;
                try
                {
                    ctx = COMWrapperContext.Create(objParam);
                }
                catch (ArgumentException e)
                {
                    throw new RuntimeException("Тип " + type + " невозможно преобразовать в один из поддерживаемых типов", e);
                }
                return ctx;
            }
            
            else
            {
                throw new RuntimeException("Тип " + type + " невозможно преобразовать в один из поддерживаемых типов");
            }
        }

        #region ICollectionContext Members

        public virtual int Count() => 0;

        bool IEmptyValueCheck.IsEmpty => false;

        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public abstract IEnumerator<IValue> GetEnumerator();

        public object UnderlyingObject => Instance;

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion        

        #region IDisposable Members

        protected virtual void Dispose(bool manualDispose)
        {
            if (manualDispose)
            {
                GC.SuppressFinalize(this);
            }

        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~COMWrapperContext()
        {
            Dispose(false);
        }

        #endregion

        public override bool DynamicMethodSignatures
        {
            get
            {
                return true;
            }
        }

        [ScriptConstructor]
        public static COMWrapperContext Constructor(IValue[] args)
        {
            return COMWrapperContext.Create(args[0].AsString(), args.Skip(1).ToArray());
        }

    }
}
//#endif

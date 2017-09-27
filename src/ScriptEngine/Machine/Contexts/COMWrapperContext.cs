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
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("COMОбъект", "COMObject")]
    public abstract class COMWrapperContext : PropertyNameIndexAccessor, ICollectionContext, IDisposable, IObjectWrapper, IEnumerable<IValue>
    {
        protected static readonly DateTime MIN_OLE_DATE = new DateTime(100,1,1);

        public COMWrapperContext()
            : base(TypeManager.GetTypeByFrameworkType(typeof(COMWrapperContext)))
        {

        }

        public static COMWrapperContext Create(string progId, IValue[] arguments)
        {
            var type = Type.GetType(progId, throwOnError: false, ignoreCase: true);
            if (type == null)
            {
                type = Type.GetTypeFromProgID(progId, throwOnError: true);
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
                return new UnmanagedRCWComContext(instance);
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
            return type.FullName == "System.__ComObject"; // string, cause it's hidden type
        }

        public static object[] MarshalArguments(IValue[] arguments)
        {
            var args = arguments.Select(x => MarshalIValue(x)).ToArray();
            return args;
        }

        public static object MarshalIValue(IValue val)
        {
            object retValue;
            if (val != null && val.DataType == Machine.DataType.Date)
            {
                var date = val.AsDate();
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
            return arg == null || arg.DataType == DataType.NotAValidValue;
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
                return ValueFactory.Create(ctx);
            }
            
            else
            {
                throw new RuntimeException("Тип " + type + " невозможно преобразовать в один из поддерживаемых типов");
            }
        }

        #region ICollectionContext Members

        public int Count()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public abstract IEnumerator<IValue> GetEnumerator();
        public abstract object UnderlyingObject { get; }

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
        public static IRuntimeContextInstance Constructor(IValue[] args)
        {
            return COMWrapperContext.Create(args[0].AsString(), args.Skip(1).ToArray());
        }

    }
}
//#endif
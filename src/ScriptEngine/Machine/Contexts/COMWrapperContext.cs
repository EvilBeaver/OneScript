#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("COMОбъект", "COMObject")]
    abstract public class COMWrapperContext : PropertyNameIndexAccessor, ICollectionContext, IDisposable, IObjectWrapper
    {
        public COMWrapperContext()
            : base(TypeManager.GetTypeByFrameworkType(typeof(COMWrapperContext)))
        {

        }
        
        public static COMWrapperContext Create(string progId, IValue[] arguments)
        {
            var type = Type.GetTypeFromProgID(progId, true);

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
            else if (IsObjectType(type))
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

        private static bool TypeIsRuntimeCallableWrapper(Type type)
        {
            return type.FullName == "System.__ComObject"; // string, cause it's hidden type
        }

        protected static object[] MarshalArguments(IValue[] arguments)
        {
            var args = arguments.Select(x => MarshalIValue(x)).ToArray();
            return args;
        }

        public static object MarshalIValue(IValue val)
        {
            return ContextValuesMarshaller.ConvertToCLRObject(val);
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
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
            }
            else if (type == typeof(double))
            {
                return ValueFactory.Create((decimal)objParam);
            }
            else if (type == typeof(DateTime))
            {
                return ValueFactory.Create((DateTime)objParam);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if (type.IsArray)
            {
                return new SafeArrayWrapper(objParam);
            }
            else if (IsObjectType(type))
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
#endif
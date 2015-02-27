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
        public static COMWrapperContext Create(string progId, IValue[] arguments)
        {
            var type = Type.GetTypeFromProgID(progId, true);
            if(TypeIsRuntimeCallableWrapper(type))
            {
                return new UnmanagedRCWComContext(type, arguments);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static COMWrapperContext Create(object instance)
        {
            if(TypeIsRuntimeCallableWrapper(instance.GetType()))
            {
                return new UnmanagedRCWComContext(instance);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static bool TypeIsRuntimeCallableWrapper(Type type)
        {
            return type.FullName == "System.__ComObject"; // string, cause it's hidden type
        }

        public static object MarshalIValue(IValue val)
        {
            return ContextValuesMarshaller.ConvertToCLRObject(val);
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
            else if (DispatchUtility.ImplementsIDispatch(objParam))
            {
                var ctx = COMWrapperContext.Create(objParam);
                return ValueFactory.Create(ctx);
            }
            else if (type.IsArray)
            {
                return new SafeArrayWrapper(objParam);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("Структура")]
    class StructureImpl : DynamicPropertiesCollectionHolder, ICollectionContext
    {
        private List<IValue> _values = new List<IValue>();
        
        public StructureImpl()
        {
            
        }

        public StructureImpl(string strProperties, params IValue[] values)
        {
            var props = strProperties.Split(',');
            if (props.Length < values.Length)
                throw new RuntimeException("Wrong argument value");

            for (int i = 0; i < props.Length; i++)
            {
                if (i < values.Length)
                {
                    Insert(props[i], values[i]);
                }
                else
                {
                    Insert(props[i], null);
                }
            }
        }

        [ContextMethod("Вставить")]
        public void Insert(string name, IValue val)
        {
            var num = RegisterProperty(name);
            if (num == _values.Count)
            {
                _values.Add(null);
            }

            if (val == null)
            {
                val = ValueFactory.Create();
            }

            SetPropValue(num, val);
        }

        [ContextMethod("Удалить")]
        public void Remove(string name)
        {
            var id = FindProperty(name);
            _values.RemoveAt(id);
            RemoveProperty(name);
            ReorderPropertyNumbers();
        }

        [ContextMethod("Свойство")]
        public bool HasProperty(string name, [ByRef] IVariable value = null)
        {
            int propIndex;
            try
            {
                propIndex = FindProperty(name);
            }
            catch (PropertyAccessException)
            {
                if(value != null)
                    value.Value = ValueFactory.Create();
                return false;
            }

            if(value != null)
                value.Value = GetPropValue(propIndex);

            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            return _values[propNum];
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            _values[propNum] = newVal;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                retValue = binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        #region ICollectionContext Members

        [ContextMethod("Количество")]
        public int Count()
        {
            return _values.Count;
        }

        [ContextMethod("Очистить")]
        public void Clear()
        {
            ClearProperties();
            _values.Clear();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IValue> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in GetProperties())
            {
                yield return new KeyAndValueImpl(
                    ValueFactory.Create(item.Key),
                    GetPropValue(item.Value));
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion 

        private static ContextMethodsMapper<StructureImpl> _methods = new ContextMethodsMapper<StructureImpl>();

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new StructureImpl();
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue strProperties, IValue[] args)
        {
            return new StructureImpl(strProperties.AsString(), args);
        }

    }
}

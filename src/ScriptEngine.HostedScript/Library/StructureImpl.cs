/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("Структура", "Structure")]
    public class StructureImpl : DynamicPropertiesAccessor, ICollectionContext, IEnumerable<KeyAndValueImpl>
    {
        private readonly List<IValue> _values = new List<IValue>();
        
        public StructureImpl()
        {
            
        }

        public StructureImpl(string strProperties, params IValue[] values)
        {
            var props = strProperties.Split(',');
            if (props.Length < values.Length)
                throw RuntimeException.InvalidArgumentValue();

            for (int i = 0; i < props.Length; i++)
            {
                props[i] = props[i].Trim();
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
        public void Insert(string name, IValue val = null)
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

        [ContextMethod("Удалить", "Delete")]
        public void Remove(string name)
        {
            var id = FindProperty(name);
            _values.RemoveAt(id);
            RemoveProperty(name);
            ReorderPropertyNumbers();
        }

        [ContextMethod("Свойство", "Property")]
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

        public override int GetPropCount()
        {
            return _values.Count;
        }

        public override string GetPropName(int propNum)
        {
            return GetPropertyName(propNum);
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

        #region IReflectableContext Members

        public override int GetMethodsCount()
        {
            return _methods.Count;
        }

        #endregion

        #region ICollectionContext Members

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _values.Count;
        }

        [ContextMethod("Очистить", "Clear")]
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

        public IEnumerator<KeyAndValueImpl> GetEnumerator()
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

        private static readonly ContextMethodsMapper<StructureImpl> _methods = new ContextMethodsMapper<StructureImpl>();

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new StructureImpl();
        }

        /// <summary>
        /// Создает структуру по заданному перечню свойств и значений
        /// </summary>
        /// <param name="strProperties">Строка с именами свойств, указанными через запятую.</param>
        /// <param name="args">Значения свойств. Каждое значение передается, как отдельный параметр.</param>
        [ScriptConstructor(Name="На основании свойств и значений")]
        public static IRuntimeContextInstance Constructor(IValue strProperties, IValue[] args)
        {
            return new StructureImpl(strProperties.AsString(), args);
        }

    }
}

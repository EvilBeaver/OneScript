/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("ФиксированнаяСтруктура", "FixedStructure")]
    public class FixedStructureImpl : DynamicPropertiesAccessor, ICollectionContext, IEnumerable<KeyAndValueImpl>
    {
        private readonly StructureImpl _structure = new StructureImpl();


        public FixedStructureImpl(StructureImpl structure)
        {
        	foreach (KeyAndValueImpl keyValue in structure)
        		_structure.Insert(keyValue.Key.AsString(), keyValue.Value);
        }

        public FixedStructureImpl(string strProperties, params IValue[] values)
        {
        	_structure = new StructureImpl(strProperties, values);
        }

        [ContextMethod("Свойство", "Property")]
        public bool HasProperty(string name, [ByRef] IVariable value = null)
        {
        	return _structure.HasProperty(name, value);
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public override IValue GetPropValue(int propNum)
        {
        	return _structure.GetPropValue(propNum);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
           throw new RuntimeException("Свойство только для чтения");
        }

        public override int FindProperty(string name)
        {
        	return _structure.FindProperty(name);
        }


        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public override VariableInfo GetPropertyInfo(int propertyNumber)
        {
            var realProp = _structure.GetPropertyInfo(propertyNumber);
            realProp.CanSet = false;
            return realProp;
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
        	return _structure.Count();
        }

        public CollectionEnumerator GetManagedIterator()
        {
        	return new CollectionEnumerator(_structure.GetManagedIterator());
        }
        
        #endregion

        #region IEnumerable<IValue> Members

        public IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
        	return _structure.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
        	return _structure.GetEnumerator();
        }

        #endregion

        private static readonly ContextMethodsMapper<FixedStructureImpl> _methods = new ContextMethodsMapper<FixedStructureImpl>();

        /// <summary>
        /// Создает фиксированную структуру по исходной структуре
        /// </summary>
        /// <param name="structObject">Исходная структура</param>
        //[ScriptConstructor(Name = "Из структуры")]
        private static FixedStructureImpl Constructor(StructureImpl structObject)
        {
            return new FixedStructureImpl(structObject);
        }

        /// <summary>
        /// Создает фиксированную структуру по структуре либо заданному перечню свойств и значений
        /// </summary>
        /// <param name="param1">Структура либо строка с именами свойств, указанными через запятую.</param>
        /// <param name="args">Только для перечня свойств:
        /// Значения свойств. Каждое значение передается, как отдельный параметр.</param>
        [ScriptConstructor(Name = "По ключам и значениям")]
        public static FixedStructureImpl Constructor(IValue param1, IValue[] args)
        {
            var rawArgument = param1.GetRawValue();
            if (rawArgument is BslStringValue s)
            {
                return new FixedStructureImpl((string)s, args);
            }
            else if (rawArgument is StructureImpl)
            {
                return new FixedStructureImpl(rawArgument as StructureImpl);
            }

            throw new RuntimeException("В качестве параметра для конструктора можно передавать только Структура или Ключи и Значения");
    }

}
}

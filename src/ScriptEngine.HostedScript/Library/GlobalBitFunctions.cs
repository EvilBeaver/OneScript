/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
	/// <summary>
	/// Глобальный контекст. Побитовые операции с целыми числами.
	/// </summary>
	[GlobalContext(Category="Побитовые операции с целыми числами", ManualRegistration=true)]
	public class GlobalBitFunctions : IAttachableContext
	{

		/// <summary>
		/// Выполняет побитовое И для заданных чисел.
		/// </summary>
		/// <param name="number1">Число 1</param>
		/// <param name="number2">Число 2</param>
		/// <returns>Число. Результат побитового И</returns>
		[ContextMethod("ПобитовоеИ")]
		public uint BitwiseAnd(uint number1, uint number2)
		{
			return number1 & number2;
		}

		/// <summary>
		/// Выполняет побитовое Или для заданных чисел.
		/// </summary>
		/// <param name="number1">Число 1</param>
		/// <param name="number2">Число 2</param>
		/// <returns>Число. Результат побитового Или</returns>
		[ContextMethod("ПобитовоеИли")]
		public uint BitwiseOr(uint number1, uint number2)
		{
			return number1 | number2;
		}

		/// <summary>
		/// Инвертирует биты числе.
		/// </summary>
		/// <param name="number1"></param>
		/// <returns>Число</returns>
		[ContextMethod("ПобитовоеНе")]
		public uint BitwiseNot(uint number1)
		{
			return ~number1;
		}

		/// <summary>
		/// Выполняет преобразование, эквивалентное <code>ПобитовоеИ(Число1, ПобитовоеНе(Число2))</code>
		/// </summary>
		/// <param name="number1">Число 1</param>
		/// <param name="number2">Число 2</param>
		/// <returns>Число. Результат преобразования</returns>
		[ContextMethod("ПобитовоеИНе")]
		public uint BitwiseAndNot(uint number1, uint number2)
		{
			return number1 & ~number2;
		}

		/// <summary>
		/// Выполняет побитовое Исключительное Или для заданных чисел.
		/// </summary>
		/// <param name="number1">Число 1</param>
		/// <param name="number2">Число 2</param>
		/// <returns>Число. Результат побитового Исключительного Или</returns>
		[ContextMethod("ПобитовоеИсключительноеИли")]
		public uint BitwiseXor(uint number1, uint number2)
		{
			return number1 ^ number2;
		}

		/// <summary>
		/// Получает значение заданного бита.
		/// </summary>
		/// <param name="value">Число</param>
		/// <param name="bitNumber">Номер бита</param>
		/// <returns>Булево. Истина - бит установлен в 1, Ложь - бит установлен в 0</returns>
		[ContextMethod("ПроверитьБит")]
		public bool CheckBit(uint value, int bitNumber)
		{
			return (value & (1 << bitNumber)) != 0;
		}

		/// <summary>
		/// Устанавливает нужный бит числа в указанное значение 
		/// </summary>
		/// <param name="value">Число</param>
		/// <param name="bitNumber">Номер бита</param>
		/// <param name="bitValue">Значение бита</param>
		/// <returns>Число. Число с установленным в нужное значение битом</returns>
		[ContextMethod("УстановитьБит")]
		public uint SetBit(uint value, int bitNumber, int bitValue)
		{
			return bitValue == 0
				? (value & ~(uint)(1 << bitNumber))
				: (value |  (uint)(1 << bitNumber));
		}

		/// <summary>
		/// Проверяет соответствие числа битовой маске.
		/// </summary>
		/// <param name="value">Число</param>
		/// <param name="mask">Маска</param>
		/// <returns>Булево. Истина, когда в числе установлены в 1 все биты маски.
		///  Ложь - в остальных случаях</returns>
		[ContextMethod("ПроверитьПоБитовойМаске")]
		public bool CheckByBitMask(uint value, uint mask)
		{
			return (value & mask) == mask;
		}

		/// <summary>
		/// Выполняет побитовый сдвиг числа влево на заданное смещение
		/// </summary>
		/// <param name="value">Число</param>
		/// <param name="offset">Смещение</param>
		/// <returns></returns>
		[ContextMethod("ПобитовыйСдвигВлево")]
		public uint BitwiseShiftLeft(uint value, int offset)
		{
			if (offset < 0 || offset > 31)
			{
				throw RuntimeException.InvalidArgumentValue(offset);
			}
			return value << offset;
		}

		/// <summary>
		/// Выполняет побитовый сдвиг числа вправо на заданное смещение
		/// </summary>
		/// <param name="value">Число</param>
		/// <param name="offset">Смещение</param>
		/// <returns></returns>
		[ContextMethod("ПобитовыйСдвигВправо")]
		public uint BitwiseShiftRight(uint value, int offset)
		{
			if (offset < 0 || offset > 31)
			{
				throw RuntimeException.InvalidArgumentValue(offset);
			}
			return value >> offset;
		}

		#region IAttachableContext Members

		public void OnAttach(MachineInstance machine, 
			out IVariable[] variables, 
			out MethodInfo[] methods)
		{
			variables = new IVariable[0];
			methods = GetMethods().ToArray();
		}

		public IEnumerable<MethodInfo> GetMethods()
		{
			var array = new MethodInfo[_methods.Count];
			for (int i = 0; i < _methods.Count; i++)
			{
				array[i] = _methods.GetMethodInfo(i);
			}
			return array;
		}

		#endregion

		#region IRuntimeContextInstance Members

		public bool IsIndexed
		{
			get 
			{ 
				return false; 
			}
		}

		public bool DynamicMethodSignatures
		{
			get
			{
				return false;
			}
		}

		public IValue GetIndexedValue(IValue index)
		{
			throw new NotSupportedException();
		}

		public void SetIndexedValue(IValue index, IValue val)
		{
			throw new NotSupportedException();
		}

		public int FindProperty(string name)
		{
			return -1;
		}

		public bool IsPropReadable(int propNum)
		{
			throw new NotSupportedException();
		}

		public bool IsPropWritable(int propNum)
		{
			throw new NotSupportedException();
		}

		public IValue GetPropValue(int propNum)
		{
			throw new NotSupportedException();
		}

		public void SetPropValue(int propNum, IValue newVal)
		{
			throw new NotSupportedException();
		}

		public int GetPropCount()
		{
			return 0;
		}

		public string GetPropName(int index)
		{
			throw new NotSupportedException();
		}

		public int FindMethod(string name)
		{
			return _methods.FindMethod(name);
		}

		public MethodInfo GetMethodInfo(int methodNumber)
		{
			return _methods.GetMethodInfo(methodNumber);
		}

		public int GetMethodsCount()
		{
			return _methods.Count;
		}

		public void CallAsProcedure(int methodNumber, IValue[] arguments)
		{
			_methods.GetMethod(methodNumber)(this, arguments);
		}

		public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
		{
			retValue = _methods.GetMethod(methodNumber)(this, arguments);
		}

		#endregion

		private static readonly ContextMethodsMapper<GlobalBitFunctions> _methods;

		static GlobalBitFunctions()
		{
			_methods = new ContextMethodsMapper<GlobalBitFunctions>();
		}
	}
}

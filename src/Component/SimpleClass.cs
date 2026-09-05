/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace Component
{
	[ContextClass("ПростоКласс")]
	public sealed class SimpleClass : AutoContext<SimpleClass>, ISimple
	{

        // Для вызова некоторых операций значений IValue, таких как AsString(), Count(), требуется указание активного процесса.
		// Процесс может быть передан как параметр в конструкторе или как первый параметр методов класса
        private readonly IBslProcess _bslProcess;

		private SimpleClass(IBslProcess bslProcess)
		{
			_bslProcess = bslProcess;
		}

		[ContextProperty("СвойствоПеречисление")]
		public SimpleEnum EnumProperty { get; set; }

		[ContextProperty("ЦелочисленноеСвойство")]
		public int IntProperty { get; set; }

		[ContextProperty("СвойствоСПроизвольнымЗначением")]
		public IValue AnyValueProperty { get; set; } = ValueFactory.Create();

		[ContextMethod("МетодСПроцессом")]
		public string MethodWithProcess(IBslProcess bslProcess)
		{
            // Параметр IBslProcess bslProcess не будет виден из скрипта.
			// Скрипт будет считать такой метод методом без параметров.
            return AnyValueProperty.AsString(bslProcess);
        }

        [ScriptConstructor]
		public static SimpleClass Constructor(TypeActivationContext ctx)
		{

            // В отличие от методов в конструктор передается TypeActivationContext.

            // Параметр TypeActivationContext не будет виден из скрипта.
            // Скрипт будет считать такой конструктор конструктором без параметров.
            return new SimpleClass(ctx.CurrentProcess);
		}

		[ScriptConstructor]
		public static SimpleClass Constructor(TypeActivationContext ctx, int initialProperty)
		{
            // Параметр TypeActivationContext не будет виден из скрипта.
            // Скрипт будет считать такой конструктор конструктором с ОДНИМ параметром.
            var result = new SimpleClass(ctx.CurrentProcess);
			result.IntProperty = initialProperty;
			return result;
		}
	}
}
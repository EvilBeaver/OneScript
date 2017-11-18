using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;

namespace TestRunScript
{
    class Program
    {
        static void Main(string[] args)
        {
            // Процедура или функция должны быть помечена как экспортная
            string sourceCode =
                "Функция МояФункция (МойПараметр) Экспорт" + 
                "    Возврат МойПараметр;" +
                "КонецФункции";

            HostedScriptEngine hostedScript = new HostedScriptEngine();
            // Обязательно инициализируем объект
            hostedScript.Initialize();

            var scriptEngine = hostedScript.EngineInstance;
            var compiler = scriptEngine.GetCompilerService();

            var runner = hostedScript.EngineInstance.AttachedScriptsFactory.LoadFromString(compiler, sourceCode);

            int methodIndex = runner.FindMethod("МояФункция");
            IValue result;
            IValue[] arguments = new IValue[1];

            arguments[0] = ValueFactory.Create(10);

            runner.CallAsFunction(methodIndex, arguments, out result);

            Console.WriteLine("The value is: {0}", result.AsNumber());

        }
    }
}

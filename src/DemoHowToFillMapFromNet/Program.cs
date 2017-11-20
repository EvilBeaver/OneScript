using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Ингода необходимо заполнить стандартные объекты OneScript, такие как Соответствие из .NET
// К примеру, мне надобыло передать заголовки в HTTPСервисЗапрос, которые имеют тип ФиксированноеСоответствие
// Ниже пример заполнения объекта ФиксированноеСоответствие с использованием класса ValueFactory,
// который преобразует типы .NET в IValue

using ScriptEngine.HostedScript;
// Пространство имен для стандартных коллекций
using ScriptEngine.HostedScript.Library;
// Пространство имен для ValueFactory
using ScriptEngine.Machine;

namespace DemoHowToFillMapFromNet
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создаем экземпляр только для того, чтобы можно было использовать типы MapImpl etc. из внешнего приложения
            HostedScriptEngine hostedScript = new HostedScriptEngine();

            MapImpl map = new MapImpl();
            
            map.Insert(ValueFactory.Create("Ключ1"), ValueFactory.Create(1));
            map.Insert(ValueFactory.Create("Ключ2"), ValueFactory.Create("Строка"));
            // Добавляем Неопределено
            map.Insert(ValueFactory.Create("Ключ3"), ValueFactory.Create());

            // ПараметрыURL будут пустыми
            FixedMapImpl fixedMap = new FixedMapImpl(new MapImpl());
        }
    }
}

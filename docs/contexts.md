# BSL-контексты и глобальные методы: руководство разработчика

Этот документ — практическая инструкция по добавлению в OneScript новых BSL‑контекстов (классов), методов и свойств, а также глобальных методов. Здесь собраны готовые сниппеты, чек‑лист и ссылки на ключевые места в исходниках.

См. также «Архитектурный обзор»: docs/arhitecture_overview.md (карта компонентов и «куда лезть»).


Содержание

- Что такое BSL‑контекст
- Добавление нового BSL‑класса (контекста)
- Добавление свойства
- Добавление метода
- Создание глобального контекста и глобальных методов
- Регистрация библиотек и package‑loader.os
- i18n для API (двуязычные имена)
- Депрекейшен и предупреждения
- Тестирование (C# и BSL)
- Документация (OneScriptDocumenter)
- Безопасность
- Чек‑лист готовности


1. Что такое BSL‑контекст

- Контекст — это .NET‑класс, методы/свойства которого доступны из BSL. Экземпляр контекста может создаваться оператором Новый (класс‑контекст) или предоставляться глобально (глобальный контекст).
- Отражение и метаданные описываются атрибутами:
  - [ContextClass("РусИмя", "EngName")]
  - [ContextMethod("РусИмя", "EngName")]
  - [ContextProperty("РусИмя", "EngName", CanRead = true, CanWrite = false, ...)]
  - [GlobalContext(...)] для глобального контекста
  - [ScriptConstructor] для создания объектов через Новый
- Двуязычные имена обязательны: все элементы публичного API должны иметь пару имен Рус/Eng.

Где в коде смотреть

- Атрибуты и метаданные: src/OneScript.Core/Contexts/*
- Базовые помощники контекстов: src/ScriptEngine/Machine/Contexts/*
- Глобальные контексты (база): GlobalContextBase — src/ScriptEngine/Machine/Contexts/GlobalContextBase.cs


2. Добавление нового BSL‑класса (контекста)

Минимальный шаблон

```csharp
using OneScript.Core.Contexts;
using OneScript.Core.Types;
using OneScript.Core.Values;
using ScriptEngine;
using ScriptEngine.Machine.Contexts; // AutoContext<T>
using OneScript.Core.Execution;      // IBslProcess

[ContextClass("ПримерКласс", "SampleClass")]
public class SampleClass : AutoContext<SampleClass>
{
    // Конструктор для BSL: Новый ПримерКласс()
    [ScriptConstructor(Name = "Без параметров")]
    public static SampleClass Ctor(TypeActivationContext ctx)
        => new SampleClass();

    // Свойство только для чтения
    [ContextProperty("Версия", "Version", CanWrite = false)]
    public IValue Version => ValueFactory.Create("1.0");

    // Процедура с доступом к процессу (лог, сервисы и т.п.)
    [ContextMethod("Сообщить", "Message")]
    public void Message(IBslProcess process, IValue text)
    {
        // Пример: вывести в лог хоста или SystemLogger (зависит от окружения)
        // process.Services.Resolve<IHostApplication>() ... 
    }

    // Функция с возвратом значения
    [ContextMethod("Сложить", "Add")]
    public IValue Add(IValue a, IValue b)
    {
        var sum = a.AsNumber() + b.AsNumber();
        return ValueFactory.Create(sum);
    }
}
```

Комментарии к шаблону

- Наследуемся от AutoContext<T> — это стандартная база для классов‑контекстов.
- [ScriptConstructor] — статический фабричный метод, принимающий TypeActivationContext. Можно объявить несколько перегрузок.
- IBslProcess можно внедрять первым параметром метода, чтобы получить доступ к сервисам/окружению выполнения.
- Возвраты:
  - Процедура — метод без возвращаемого значения (void).
  - Функция — возвращает IValue (используйте ValueFactory.Create(...) для упаковки).

Регистрация в движке

- В хостинге (HostedScript) добавьте сборку в окружение: env.AddAssembly(typeof(SampleClass).Assembly)
  - Обычно через расширения билдера: src/ScriptEngine.HostedScript/Extensions/EngineBuilderExtensions.cs
- ContextDiscoverer просканирует сборку и зарегистрирует класс автоматически.

Типичные ошибки

- Неуказанные двуязычные имена: всегда задавайте оба (РусИмя, EngName).
- Неверный тип параметров по ссылке: для by‑ref используйте IVariable (см. ниже).
- Отсутствие регистрации сборки в окружении: класс «не найден» в BSL.


3. Добавление свойства

Шаблон

```csharp
[ContextProperty("Порог", "Threshold", CanRead = true, CanWrite = true)]
public IValue Threshold
{
    get => ValueFactory.Create(_threshold);
    set => _threshold = value.AsNumber();
}
private decimal _threshold = 0m;
```

Заметки

- CanRead/CanWrite управляют доступностью геттера/сеттера из BSL.
- Для упаковки/распаковки используйте ValueFactory и методы As*() у IValue.
- Устаревание (deprecated) свойства — см. раздел «Депрекейшен»: свойство выдаст compile‑time предупреждение при обращении.


4. Добавление метода

Шаблон процедуры и функции

```csharp
// Процедура, изменяющая параметр по ссылке
[ContextMethod("УдвоитьЧисло", "DoubleNumber")]
public void DoubleNumber(IVariable number)
{
    var doubled = number.Value.AsNumber() * 2;
    number.Value = ValueFactory.Create(doubled);
}

// Функция с необязательным аргументом
[ContextMethod("СтрокаСВерсией", "StringWithVersion")]
public IValue StringWithVersion(IValue prefix /* может быть пропущен */)
{
    var hasPrefix = !(prefix is OneScript.Core.Values.BslSkippedParameterValue);
    var p = hasPrefix ? prefix.AsString() : "";
    return ValueFactory.Create($"{p}{Version.AsString()}");
}
```

Заметки

- По ссылке: используйте тип IVariable — в него можно присвоить новое значение через .Value.
- По значению: используйте IValue. Для чисел/строк/дат есть AsNumber/AsString/AsDate.
- Пропуск аргументов: при вызове из BSL необязательные параметры могут быть пропущены; движок передаёт специальное значение «пропущено» (BslSkippedParameterValue). Проверяйте и применяйте значения по умолчанию самостоятельно.


5. Создание глобального контекста и глобальных методов

Глобальный контекст

```csharp
using OneScript.Core.Contexts;
using OneScript.Core.Values;
using ScriptEngine.Machine.Contexts;

[GlobalContext(Category = "Мои функции")]
public class MyGlobals : GlobalContextBase<MyGlobals>
{
    // Фабрика экземпляра для внедрения в глобальную область
    public static IAttachableContext CreateInstance() => new MyGlobals();

    [ContextMethod("МояФункция", "MyFunc")]
    public IValue MyFunc(IValue x)
    {
        return ValueFactory.Create(x.ToString().Length);
    }
}
```

Заметки

- По умолчанию глобальные контексты регистрируются автоматически (ManualRegistration = false). Достаточно, чтобы сборка была добавлена в окружение.
- Вручную можно внедрить через HostedScriptEngine.InjectObject или IRuntimeEnvironment.InjectObject.

Добавление метода в существующий глобальный контекст

- Например, StandardGlobalContext: добавьте [ContextMethod] в соответствующий класс и реализуйте логику.
- Внимание: изменение публичного API стандартной библиотеки требует обсуждения с мэйнтейнерами.


6. Регистрация библиотек и package‑loader.os

- HostedScript ищет библиотеку и обрабатывает package‑loader.os (дефолтный или кастомный).
- Основные операции загрузчика (см. src/ScriptEngine.HostedScript/LibraryLoader.cs):
  - ДобавитьКласс/AddClass("path", "ИмяКласса") — регистрирует новый BSL‑тип;
  - ДобавитьМодуль/AddModule("path", "ИмяМодуля") — подключает модуль как глобальный;
  - ДобавитьМакет/AddTemplate — регистрирует шаблон.

Пример package‑loader.os

```bsl
// package-loader.os
Загрузчик = Новый LibraryLoader();
Загрузчик.ДобавитьКласс("./src/MyType.os", "МойТип");
Загрузчик.ДобавитьМодуль("./src/Utils.os", "МойМодуль");
```

Где смотреть

- Лоадер/поиск: src/ScriptEngine.HostedScript/LibraryLoader.cs, FileSystemDependencyResolver.cs


7. i18n для API (двуязычные имена)

- Всегда задавайте пары имён (РусИмя, EngName) в атрибутах классов/методов/свойств.
- Пользовательские строки делайте через НСтр/Locale/BilingualString (см. src/OneScript.Language/Localization/BilingualString.cs и docs/arhitecture_overview.md).


8. Депрекейшен и предупреждения

- Методы:
  - [ContextMethod(IsDeprecated = true, ThrowOnUse = false)] — при вызове формируется предупреждение (через SystemLogger).
  - ThrowOnUse = true — при обращении будет исключение времени выполнения.
- Свойства:
  - [ContextProperty(IsDeprecated = true, ...)] — предупреждение на этапе генерации кода стек‑машины (compile‑time). ThrowOnUse для свойств обычно не применяется.
- Старые имена:
  - [DeprecatedName] — для сохранения совместимости со старым названием члена.

Пример

```csharp
[ContextMethod("СтарыйМетод", "OldMethod", IsDeprecated = true, ThrowOnUse = false)]
public void OldMethod() { /* ... */ }

[ContextProperty("СтароеСвойство", "OldProperty", IsDeprecated = true, CanRead = true)]
public IValue OldProperty => ValueFactory.Create(true);
```


9. Тестирование

- C#‑тесты:
  - Проверяйте отражение и вызовы: наличие членов, сигнатуры, поведение (src/Tests/OneScript.Core.Tests/*).
  - Для нативного режима (#native) — см. src/Tests/OneScript.Dynamic.Tests/* (по необходимости).
- BSL‑тесты:
  - Добавьте скрипты в tests/*.os, которые создают объект, дергают методы/свойства и проверяют результаты.

Мини‑пример BSL‑теста

```bsl
// tests/my-context-test.os
Объект = Новый ПримерКласс();
Сообщить(Объект.Версия);
Значение = 10;
Объект.УдвоитьЧисло(Значение);
Если Значение <> 20 Тогда
    ВызватьИсключение "Тест не пройден";
КонецЕсли;
```


10. Документация (OneScriptDocumenter)

- XML‑док‑комментарии классов/методов попадут в автогенерат.
- Исключить элемент из автодока можно атрибутом (например, DocumenterHint/SkipForDocumenter, если применяется в проекте).
- Генерация: msbuild Build.csproj /t:BuildDocumentation (см. README.md).


11. Безопасность

- Подключение внешних .NET/Native компонентов не изолировано песочницей.
- Не загружайте непроверенные библиотеки в production. Выводите предупреждения через SystemLogger при потенциально небезопасных операциях.


12. Чек‑лист готовности (BSL‑контекст/глобальный метод)

- [ ] Указаны двуязычные имена во всех атрибутах.
- [ ] Для классов реализованы [ScriptConstructor] при необходимости «Новый ...».
- [ ] Сборка регистрируется в окружении (env.AddAssembly(...)) или через пакетный загрузчик.
- [ ] Написаны C#‑тесты (отражение/вызовы) и BSL‑скрипт‑тест(ы).
- [ ] При необходимости добавлены XML‑док‑комментарии для автодокументации.
- [ ] Локализуемые строки — через НСтр/BilingualString.
- [ ] Для устаревших членов выставлены IsDeprecated/ThrowOnUse и/или DeprecatedName.


Ссылки в исходниках

- Атрибуты и метаданные: src/OneScript.Core/Contexts/*
- Базовые классы и инъекции: src/ScriptEngine/Machine/Contexts/*
- HostedScript и загрузка библиотек: src/ScriptEngine.HostedScript/*
- Стандартная библиотека (примеры контекстов): src/OneScript.StandardLibrary/*

Если вы впервые добавляете контекст — начните с раздела «Архитектурный обзор»: docs/arhitecture_overview.md, затем следуйте шагам из этого How‑To.
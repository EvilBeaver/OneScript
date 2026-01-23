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

    // Процедура с доступом к bsl-процессу (возможность запускать свой код bsl из кода c#)
    [ContextMethod("Сообщить", "Message")]
    public void Message(IBslProcess process, IValue text)
    {
        // вызов bsl-метода в том же стеке вызовов, что и у переданного процесса
        process.Run(/*...*/);
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
- [ScriptConstructor] — статический фабричный метод, возможно, принимающий TypeActivationContext. Можно объявить несколько перегрузок.
- IBslProcess можно внедрять первым параметром метода, чтобы получить доступ к сервисам/окружению выполнения.
- Возвраты:
  - Процедура — метод без возвращаемого значения (void).
  - Функция — возвращает IValue или конвертируемый тип C# (см. ContextValuesMarshaller).

Регистрация в движке

- При старте ContextDiscoverer просканирует сборку и автоматически зарегистрирует в движке все классы, помеченные атрибутами ContextClass, GlobalContext, EnumerationType

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

- CanRead/CanWrite управляют доступностью геттера/сеттера из BSL, если не указаны, берутся наличия стандартных get/set у свойства.
- Маршаллинг значений свойства автоматический.


4. Добавление метода

Шаблон процедуры и функции

```csharp
// Процедура, изменяющая параметр по ссылке
[ContextMethod("УдвоитьЧисло", "DoubleNumber")]
public int DoubleNumber(int number)
{
    var doubled = number * 2;
    return doubled;
}
```

Значения параметров и результат метода будут автоматически сконвертированы из типов C# в тиаы bsl.

Заметки

- Для передачи аргумента по ссылке: используйте тип IVariable — в него можно присвоить новое значение через .Value.
- По значению: используйте типы C# напрямую, если они поддерживаются маршаллером, или IValue.

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

- HostedScript ищет библиотеку и вызывает package‑loader.os (дефолтный или кастомный).
- Основные операции загрузчика (см. src/ScriptEngine.HostedScript/LibraryLoader.cs):
  - ДобавитьКласс/AddClass("path", "ИмяКласса") — регистрирует новый BSL‑тип;
  - ДобавитьМодуль/AddModule("path", "ИмяМодуля") — подключает модуль как глобальный;
  - ДобавитьМакет/AddTemplate — регистрирует шаблон.
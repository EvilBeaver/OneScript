# BSL-контексты и глобальные методы: руководство разработчика

Этот документ — практическая инструкция по добавлению в OneScript новых BSL‑контекстов (классов), методов и свойств, а также глобальных методов. Здесь собраны готовые сниппеты, чек‑лист и ссылки на ключевые места в исходниках.

См. также «Архитектурный обзор»: docs/arhitecture_overview.md (карта компонентов и «куда лезть»).

Содержание

- Что такое BSL‑контекст
- Добавление нового BSL‑класса (контекста)
- Добавление свойства
- Добавление метода
- Конвертеры значений при маршаллинге
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

5. Конвертеры значений при маршаллинге

По умолчанию маршаллер `ContextValuesMarshaller` самостоятельно преобразует примитивные типы C# (`int`, `string`, `decimal`, `bool`, `DateTime`) и типы, реализующие `IValue`/`BslValue`, в значения BSL и обратно. Если нужно перехватить это преобразование для произвольного CLR-типа — используется механизм конвертеров.

5.1. Интерфейс `IBslValueConverter`

Конвертеры реализуют статические методы (C# 12 / `static abstract`):

```csharp
public interface IBslValueConverter
{
    // CLR-объект → BSL-значение (при возврате из метода или чтении свойства)
    static abstract BslValue ToBslValue(object value);

    // BSL-значение → CLR-объект (при передаче параметра или записи свойства)
    static abstract object ToClrValue(BslValue value);
}
```

Реализующий тип — обычный `sealed class` (не `static`), методы объявлены как `static`. Экземпляр конвертера никогда не создаётся. Если нужна стандартная конвертация вложенных значений — вызывайте `ContextValuesMarshaller` напрямую (он публичный и статический).

5.2. Атрибут `BslValueConverterAttribute`

Атрибут задаёт конвертер для конкретного параметра метода, возвращаемого значения или свойства. Рекомендуется использовать generic-форму — она обеспечивает компайл-тайм валидацию:

```csharp
// Generic-форма (рекомендуется): ошибка компиляции, если T не реализует IBslValueConverter
[BslValueConverter<MyDtoConverter>]

// Базовый абстрактный класс используется только для рефлексии внутри движка
```

Правила применения:
- На **параметре** — конвертер вызывается при передаче аргумента из BSL (BSL → CLR, метод `ToClrValue`).
- На **методе** — конвертер вызывается для возвращаемого значения (CLR → BSL, метод `ToBslValue`).
- На **свойстве** — конвертер вызывается в обоих направлениях: `ToClrValue` при записи из BSL, `ToBslValue` при чтении из BSL.

5.3. Конвертер на параметре метода

```csharp
[ContextMethod("ОбработатьДанные", "ProcessData")]
public void ProcessData([BslValueConverter<MyDtoConverter>] MyDto dto)
{
    // dto уже преобразован из BSL-значения конвертером MyDtoConverter.ToClrValue
}
```

5.4. Конвертер на возвращаемом значении

Атрибут размещается на самом методе — это означает конвертацию возвращаемого значения:

```csharp
[ContextMethod("ПолучитьДанные", "GetData")]
[BslValueConverter<MyDtoConverter>]
public MyDto GetData()
{
    // возвращаемый объект будет преобразован в BSL-значение конвертером MyDtoConverter.ToBslValue
    return new MyDto { Value = 42 };
}
```

5.5. Конвертер на свойстве

```csharp
[ContextProperty("Данные", "Data")]
[BslValueConverter<MyDtoConverter>]
public MyDto Data
{
    get => _data;              // ToBslValue вызывается при чтении из BSL
    set => _data = value;      // ToClrValue вызывается при записи из BSL
}
```

5.6. Пример полного конвертера

```csharp
public sealed class MyDtoConverter : IBslValueConverter
{
    public static BslValue ToBslValue(object value)
    {
        var dto = (MyDto)value;
        var structure = new StructureImpl();
        structure.Insert("Value", BslNumericValue.Create(dto.Value));
        return structure;
    }

    public static object ToClrValue(BslValue value)
    {
        var structure = (StructureImpl)value;
        var num = structure.GetIndexedValue(ValueFactory.Create("Value")).AsNumber();
        return new MyDto { Value = (int)num };
    }
}
```

6. Создание глобального контекста и глобальных методов

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

7. Регистрация библиотек и package‑loader.os

- HostedScript ищет библиотеку и вызывает package‑loader.os (дефолтный или кастомный).
- Основные операции загрузчика (см. src/ScriptEngine.HostedScript/LibraryLoader.cs):
  - ДобавитьКласс/AddClass("path", "ИмяКласса") — регистрирует новый BSL‑тип;
  - ДобавитьМодуль/AddModule("path", "ИмяМодуля") — подключает модуль как глобальный;
  - ДобавитьМакет/AddTemplate — регистрирует шаблон.
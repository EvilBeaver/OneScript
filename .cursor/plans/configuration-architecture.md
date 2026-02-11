# Архитектура системы конфигурации OneScript

## Оглавление
1. [Обзор](#обзор)
2. [Основные компоненты](#основные-компоненты)
3. [Архитектура классов](#архитектура-классов)
4. [Поток данных](#поток-данных)
5. [Жизненный цикл конфигурации](#жизненный-цикл-конфигурации)
6. [Источники конфигурации](#источники-конфигурации)
7. [Порядок приоритетов](#порядок-приоритетов)
8. [Использование конфигурации](#использование-конфигурации)
9. [Диаграммы и схемы](#диаграммы-и-схемы)
10. [Точки расширения](#точки-расширения)
11. [Примеры использования](#примеры-использования)

---

## Обзор

Система конфигурации OneScript предоставляет гибкий механизм настройки параметров движка через множественные источники с определенным порядком приоритетов. Конфигурация основана на принципе "слияния" (merge) значений из разных провайдеров, где последующие провайдеры перезаписывают значения предыдущих.

### Ключевые принципы
- **Множественные источники**: системные файлы, локальные файлы, переменные окружения
- **Приоритеты**: каждый источник имеет свой приоритет
- **Слияние**: значения объединяются с перезаписью по ключу
- **Ленивая загрузка**: провайдеры возвращают функции-фабрики, вычисление происходит при необходимости
- **Dependency Injection**: конфигурация интегрирована в DI-контейнер

---

## Основные компоненты

### 1. Ядро конфигурации (ScriptEngine.Hosting)

#### `KeyValueConfig`
**Расположение**: `src/ScriptEngine/Hosting/KeyValueConfig.cs`

Центральный класс для хранения конфигурационных параметров в формате ключ-значение.

```csharp
public class KeyValueConfig
{
    private readonly Dictionary<string, string> _values;
    
    public KeyValueConfig()
    public KeyValueConfig(IDictionary<string, string> source)
    
    public void Merge(IDictionary<string, string> source)  // Объединяет значения
    public string this[string key] { get; private set; }   // Доступ по ключу
}
```

**Особенности**:
- Использует `StringComparer.InvariantCultureIgnoreCase` для регистронезависимого поиска
- Метод `Merge` перезаписывает существующие значения новыми
- Валидация ключей: не допускаются пустые или null ключи

#### `ConfigurationProviders`
**Расположение**: `src/ScriptEngine/Hosting/ConfigurationProviders.cs`

Коллекция провайдеров конфигурации, которая управляет порядком их применения.

```csharp
public class ConfigurationProviders
{
    private List<Func<IDictionary<string, string>>> _providers;
    
    public void Add(Func<IDictionary<string, string>> configGetter)
    public KeyValueConfig CreateConfig()  // Создает объединенную конфигурацию
}
```

**Алгоритм работы `CreateConfig()`**:
1. Создает новый пустой `KeyValueConfig`
2. Итерируется по всем зарегистрированным провайдерам в порядке добавления
3. Для каждого провайдера:
   - Вызывает функцию-фабрику `provider()`
   - Получает словарь значений
   - Вызывает `config.Merge(values)` для объединения
4. Возвращает итоговую конфигурацию

**Важно**: Порядок добавления провайдеров определяет приоритет (последний побеждает).

#### `IEngineBuilder`
**Расположение**: `src/ScriptEngine/Hosting/IEngineBuilder.cs`

Интерфейс построителя движка, предоставляющий доступ к провайдерам конфигурации.

```csharp
public interface IEngineBuilder
{
    ConfigurationProviders ConfigurationProviders { get; }
    EnvironmentProviders EnvironmentProviders { get; }
    IServiceDefinitions Services { get; set; }
    ScriptingEngine Build();
}
```

#### `DefaultEngineBuilder`
**Расположение**: `src/ScriptEngine/Hosting/DefaultEngineBuilder.cs`

Реализация построителя по умолчанию.

```csharp
public class DefaultEngineBuilder : IEngineBuilder
{
    public ConfigurationProviders ConfigurationProviders { get; } = new ConfigurationProviders();
    public EnvironmentProviders EnvironmentProviders { get; } = new EnvironmentProviders();
    public IServiceDefinitions Services { get; set; } = new TinyIocImplementation();
    
    public virtual ScriptingEngine Build()
}
```

**Метод `Build()`**:
1. Создает DI-контейнер из `Services`
2. Разрешает зависимости (`ScriptingEngine`, `ExecutionContext`)
3. Применяет провайдеры окружения через `EnvironmentProviders.Invoke(env)`
4. Инициализирует `IDependencyResolver` если зарегистрирован
5. Возвращает готовый движок

### 2. Провайдеры конфигурации (ScriptEngine.HostedScript)

#### `IConfigProvider`
**Расположение**: `src/ScriptEngine.HostedScript/IConfigProvider.cs`

Базовый интерфейс для всех провайдеров конфигурации.

```csharp
public interface IConfigProvider
{
    Func<IDictionary<string, string>> GetProvider();
}
```

**Паттерн**: Фабрика функций — возвращает не сами данные, а функцию для их получения. Это обеспечивает:
- Ленивое вычисление
- Возможность повторного чтения конфигурации
- Изоляцию состояния

#### `CfgFileConfigProvider`
**Расположение**: `src/ScriptEngine.HostedScript/CfgFileConfigProvider.cs`

Провайдер для чтения конфигурации из файлов `oscript.cfg`.

```csharp
public class CfgFileConfigProvider : IConfigProvider
{
    public const string CONFIG_FILE_NAME = "oscript.cfg";
    
    public string FilePath { get; set; }
    public bool Required { get; set; }
    
    public Func<IDictionary<string, string>> GetProvider()
    private IDictionary<string, string> ReadConfigFile(string configPath)
    private static void ExpandRelativePaths(IDictionary<string, string> conf, string configFile)
}
```

**Формат файла `oscript.cfg`**:
```
# Комментарий
ключ=значение
lib.system=./oscript_modules
lib.additional=/path1;/path2
encoding.script=utf-8
```

**Алгоритм чтения**:
1. Открывает файл с автоопределением кодировки
2. Читает построчно
3. Пропускает пустые строки и комментарии (начинаются с `#`)
4. Разбивает строки по первому символу `=` на ключ и значение
5. Обрезает пробелы у ключа и значения
6. Вызывает `ExpandRelativePaths()` для преобразования относительных путей

**Обработка путей `ExpandRelativePaths()`**:
- Для ключа `lib.system`: если путь относительный, преобразует в абсолютный относительно директории конфиг-файла
- Для ключа `lib.additional`: разбивает по `;`, каждый путь преобразует в абсолютный, объединяет обратно через `;`

**Обработка ошибок**:
- Если файл не найден и `Required=false`: возвращает пустой словарь
- Если файл не найден и `Required=true`: пробрасывает `IOException`

#### `FormatStringConfigProvider`
**Расположение**: `src/ScriptEngine.HostedScript/FormatStringConfigProvider.cs`

Провайдер для чтения конфигурации из форматированной строки (используется для переменных окружения).

```csharp
public class FormatStringConfigProvider : IConfigProvider
{
    public string ValuesString { get; set; }
    
    public Func<IDictionary<string, string>> GetProvider()
}
```

**Формат строки**: `key1=value1;key2=value2;key3=value3`

**Реализация**: Использует `FormatParametersList` из `OneScript.Core.Commons` для парсинга строки.

### 3. Методы расширения (ScriptEngine.HostedScript.Extensions)

#### `EngineBuilderExtensions`
**Расположение**: `src/ScriptEngine.HostedScript/Extensions/EngineBuilderExtensions.cs`

Набор методов расширения для удобной настройки провайдеров конфигурации.

```csharp
public static class EngineBuilderExtensions
{
    // Добавляет произвольный файл конфигурации
    public static ConfigurationProviders UseConfigFile(
        this ConfigurationProviders providers, 
        string configFile, 
        bool required = true)
    
    // Добавляет системный файл конфигурации (рядом с движком)
    public static ConfigurationProviders UseSystemConfigFile(
        this ConfigurationProviders providers)
    
    // Добавляет файл конфигурации рядом с точкой входа (скриптом)
    public static ConfigurationProviders UseEntrypointConfigFile(
        this ConfigurationProviders providers, 
        string entryPoint)
    
    // Добавляет конфигурацию из переменной окружения
    public static ConfigurationProviders UseEnvironmentVariableConfig(
        this ConfigurationProviders providers, 
        string varName)
    
    // Настраивает файловую систему библиотек
    public static IEngineBuilder UseFileSystemLibraries(
        this IEngineBuilder b)
    
    // Другие методы расширения...
}
```

#### Детали реализации методов:

**`UseSystemConfigFile()`**:
1. Получает путь к сборке `IValue` (ядро движка)
2. Если путь пустой, использует `GetEntryAssembly().Location`
3. Формирует путь: `{путь_к_сборке}/oscript.cfg`
4. Вызывает `UseConfigFile()` с этим путем и `required=true`

**`UseEntrypointConfigFile()`**:
1. Извлекает директорию из пути к точке входа
2. Формирует путь: `{директория}/oscript.cfg`
3. Проверяет существование файла
4. Если существует, вызывает `UseConfigFile()` с `required=false`
5. Возвращает `providers` без изменений если файл не существует

**`UseEnvironmentVariableConfig()`**:
1. Читает значение переменной окружения через `Environment.GetEnvironmentVariable(varName)`
2. Если переменная не установлена (`null`), возвращает `providers` без изменений
3. Создает `FormatStringConfigProvider` со значением переменной
4. Добавляет провайдер через `providers.Add(reader.GetProvider())`

**`UseFileSystemLibraries()`**:
1. Регистрирует в DI фабрику для `IDependencyResolver`
2. Фабрика:
   - Разрешает `OneScriptLibraryOptions` из контейнера
   - Создает список директорий поиска
   - Определяет `SystemLibraryDir`: из опций или директория сборки
   - Добавляет `AdditionalLibraries` если есть
   - Создает и настраивает `FileSystemDependencyResolver`

### 4. Регистрация в DI (ScriptEngine.Hosting)

#### `ServiceRegistrationExtensions`
**Расположение**: `src/ScriptEngine/Hosting/ServiceRegistrationExtensions.cs`

```csharp
public static class ServiceRegistrationExtensions
{
    public static IEngineBuilder SetupConfiguration(
        this IEngineBuilder b, 
        Action<ConfigurationProviders> setup)
    {
        setup(b.ConfigurationProviders);                      // 1. Настройка провайдеров
        b.Services.RegisterSingleton(b.ConfigurationProviders); // 2. Регистрация в DI
        return b;
    }
}
```

**Назначение**: Связывает настройку провайдеров с их регистрацией в DI-контейнере.

#### `EngineBuilderExtensions.SetDefaultOptions()`
**Расположение**: `src/ScriptEngine/Hosting/EngineBuilderExtensions.cs`

```csharp
public static IEngineBuilder SetDefaultOptions(this IEngineBuilder builder)
{
    builder.Services.Register<OneScriptCoreOptions>(sp =>
    {
        var cfg = sp.Resolve<KeyValueConfig>();
        return new OneScriptCoreOptions(cfg);
    });
    
    builder.Services.Register<KeyValueConfig>(sp =>
    {
        var providers = sp.Resolve<ConfigurationProviders>();
        return providers.CreateConfig();  // Создание конфигурации из провайдеров
    });
    
    builder.Services.Register<ScriptingEngine>();
    
    return builder;
}
```

**Важно**: 
- `KeyValueConfig` создается лениво при разрешении из DI
- `ConfigurationProviders` должен быть зарегистрирован как singleton ранее
- `OneScriptCoreOptions` зависит от `KeyValueConfig`

### 5. Классы опций

#### `OneScriptCoreOptions`
**Расположение**: `src/ScriptEngine/OneScriptCoreOptions.cs`

Базовый класс опций для ядра движка.

```csharp
public class OneScriptCoreOptions
{
    private const string FILE_READER_ENCODING = "encoding.script";
    private const string SYSTEM_LANGUAGE_KEY = "SystemLanguage";
    private const string PREPROCESSOR_DEFINITIONS_KEY = "preprocessor.define";
    private const string DEFAULT_RUNTIME_KEY = "runtime.default";
    private const string EXPLICIT_IMPORT = "lang.explicitImports";
    
    public OneScriptCoreOptions(KeyValueConfig config)
    
    public string SystemLanguage { get; }
    public Encoding FileReaderEncoding { get; }
    public bool UseNativeAsDefaultRuntime { get; }
    public IEnumerable<string> PreprocessorDefinitions { get; }
    public ExplicitImportsBehavior ExplicitImports { get; }
}
```

**Конфигурационные ключи**:
- `encoding.script`: кодировка файлов скриптов (по умолчанию UTF-8)
- `SystemLanguage`: язык системных сообщений
- `preprocessor.define`: список определений препроцессора (через запятую)
- `runtime.default`: выбор рантайма по умолчанию (`native` или `stack`)
- `lang.explicitImports`: режим явных импортов (`on`, `off`, `warn`, `dev`)

#### `OneScriptLibraryOptions`
**Расположение**: `src/ScriptEngine.HostedScript/OneScriptLibraryOptions.cs`

Расширенный класс опций для библиотек и модулей.

```csharp
public class OneScriptLibraryOptions : OneScriptCoreOptions
{
    public const string SYSTEM_LIBRARY_DIR = "lib.system";
    public const string ADDITIONAL_LIBRARIES = "lib.additional";
    
    public OneScriptLibraryOptions(KeyValueConfig config) : base(config)
    
    public string SystemLibraryDir { get; set; }
    public IEnumerable<string> AdditionalLibraries { get; set; }
}
```

**Конфигурационные ключи**:
- `lib.system`: путь к системной библиотеке модулей
- `lib.additional`: дополнительные пути поиска библиотек (разделитель `;`)

**Обработка**:
- `lib.additional` разбивается по символу `;`
- Создается список строк `List<string>`

### 6. Доступ к конфигурации из скриптов

#### `SystemConfigAccessor`
**Расположение**: `src/ScriptEngine.HostedScript/SystemConfigAccessor.cs`

Глобальный контекст для доступа к конфигурации из BSL-скриптов.

```csharp
[GlobalContext(Category = "Работа с настройками системы")]
public class SystemConfigAccessor : GlobalContextBase<SystemConfigAccessor>
{
    private KeyValueConfig _config;
    private readonly ConfigurationProviders _providers;
    
    public SystemConfigAccessor(ConfigurationProviders providers)
    
    [ContextMethod("ОбновитьНастройкиСистемы", "RefreshSystemConfig")]
    public void Refresh()
    
    [ContextMethod("ПолучитьЗначениеСистемнойНастройки", "GetSystemOptionValue")]
    public IValue GetSystemOptionValue(string optionKey)
}
```

**Методы**:
- `RefreshSystemConfig()`: пересоздает конфигурацию из провайдеров (актуализация)
- `GetSystemOptionValue(key)`: возвращает значение по ключу или `Undefined`

**Использование в BSL**:
```bsl
Значение = ПолучитьЗначениеСистемнойНастройки("lib.system");
ОбновитьНастройкиСистемы();
```

#### `EngineConfigProvider`
**Расположение**: `src/ScriptEngine.HostedScript/EngineConfigProvider.cs`

Вспомогательный класс для получения конфигурации в C# коде.

```csharp
public class EngineConfigProvider
{
    private readonly ConfigurationProviders _providers;
    private KeyValueConfig _currentConfig;
    
    public EngineConfigProvider(ConfigurationProviders providers)
    
    public KeyValueConfig ReadConfig()  // Всегда возвращает актуальную конфигурацию
}
```

---

## Архитектура классов

### Диаграмма классов

```
┌────────────────────────────────────────────────────────────────────┐
│                        IEngineBuilder                              │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ + ConfigurationProviders ConfigurationProviders              │  │
│  │ + EnvironmentProviders EnvironmentProviders                  │  │
│  │ + IServiceDefinitions Services                               │  │
│  │ + ScriptingEngine Build()                                    │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
                                ▲
                                │ implements
                                │
        ┌───────────────────────────────────────────────┐
        │      DefaultEngineBuilder                     │
        │  ┌─────────────────────────────────────────┐  │
        │  │ - ConfigurationProviders _configProv    │  │
        │  │ - EnvironmentProviders _envProv         │  │
        │  │ - IServiceDefinitions _services         │  │
        │  │ + static Create() : IEngineBuilder      │  │
        │  │ + Build() : ScriptingEngine             │  │
        │  └─────────────────────────────────────────┘  │
        └───────────────────────────────────────────────┘
                                │
                                │ has-a
                                ▼
        ┌────────────────────────────────────────────────────┐
        │         ConfigurationProviders                     │
        │  ┌──────────────────────────────────────────────┐  │
        │  │ - List<Func<IDictionary>> _providers         │  │
        │  │ + Add(Func<IDictionary> provider)            │  │
        │  │ + CreateConfig() : KeyValueConfig            │  │
        │  └──────────────────────────────────────────────┘  │
        └────────────────────────────────────────────────────┘
                                │
                                │ uses
                                ▼
        ┌────────────────────────────────────────────────────┐
        │              KeyValueConfig                        │
        │  ┌──────────────────────────────────────────────┐  │
        │  │ - Dictionary<string, string> _values         │  │
        │  │ + KeyValueConfig()                           │  │
        │  │ + KeyValueConfig(IDictionary source)         │  │
        │  │ + Merge(IDictionary source)                  │  │
        │  │ + string this[string key] { get; }           │  │
        │  └──────────────────────────────────────────────┘  │
        └────────────────────────────────────────────────────┘
                                ▲
                                │ creates
                                │
        ┌────────────────────────────────────────────────────┐
        │              IConfigProvider                       │
        │  ┌──────────────────────────────────────────────┐  │
        │  │ + GetProvider() : Func<IDictionary>          │  │
        │  └──────────────────────────────────────────────┘  │
        └────────────────────────────────────────────────────┘
                        ▲               ▲
                        │               │
          ┌─────────────┴─────┬─────────┴──────────┐
          │                   │                    │
┌─────────────────────┐ ┌────────────────────┐ ┌──────────────────────┐
│ CfgFileConfig-      │ │ FormatString-      │ │ (Custom providers)   │
│ Provider            │ │ ConfigProvider     │ │                      │
│ ┌─────────────────┐ │ │ ┌────────────────┐ │ │                      │
│ │+ FilePath       │ │ │ │+ ValuesString  │ │ │                      │
│ │+ Required       │ │ │ │                │ │ │                      │
│ └─────────────────┘ │ │ └────────────────┘ │ │                      │
└─────────────────────┘ └────────────────────┘ └──────────────────────┘

┌────────────────────────────────────────────────────────────────────┐
│                   OneScriptCoreOptions                             │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ + SystemLanguage : string                                    │  │
│  │ + FileReaderEncoding : Encoding                              │  │
│  │ + UseNativeAsDefaultRuntime : bool                           │  │
│  │ + PreprocessorDefinitions : IEnumerable<string>              │  │
│  │ + ExplicitImports : ExplicitImportsBehavior                  │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
                                ▲
                                │ extends
                                │
        ┌───────────────────────────────────────────────┐
        │      OneScriptLibraryOptions                  │
        │  ┌─────────────────────────────────────────┐  │
        │  │ + SystemLibraryDir : string             │  │
        │  │ + AdditionalLibraries : IEnumerable<>   │  │
        │  └─────────────────────────────────────────┘  │
        └───────────────────────────────────────────────┘
```

### Зависимости между компонентами

```
┌──────────────────┐
│   oscript.exe    │ (CLI приложение)
└────────┬─────────┘
         │ uses
         ▼
┌────────────────────────────┐
│  ConsoleHostBuilder        │
│  - Create(codePath)        │
│  - Build(builder)          │
└────────┬───────────────────┘
         │ creates & configures
         ▼
┌────────────────────────────┐
│  DefaultEngineBuilder      │
└────────┬───────────────────┘
         │ contains
         ▼
┌────────────────────────────┐       ┌──────────────────────────┐
│  ConfigurationProviders    │◄──────│ IConfigProvider          │
│  - Add(provider)           │       │  implementations         │
│  - CreateConfig()          │       └──────────────────────────┘
└────────┬───────────────────┘
         │ creates
         ▼
┌────────────────────────────┐
│      KeyValueConfig        │
└────────┬───────────────────┘
         │ passed to
         ▼
┌────────────────────────────┐
│  OneScriptCoreOptions      │
│  OneScriptLibraryOptions   │
└────────────────────────────┘
```

---

## Поток данных

### Общая схема потока данных

```
1. Источники конфигурации
   ├── oscript.cfg (system)        → CfgFileConfigProvider
   ├── oscript.cfg (entrypoint)    → CfgFileConfigProvider
   └── OSCRIPT_CONFIG (env var)    → FormatStringConfigProvider
                                             │
                                             ▼
2. Регистрация провайдеров              ConfigurationProviders
   (порядок определяет приоритет)       │
                                        │ .Add(provider1.GetProvider())
                                        │ .Add(provider2.GetProvider())
                                        │ .Add(provider3.GetProvider())
                                        │
                                        ▼
3. Создание конфигурации            .CreateConfig()
   ┌────────────────────────────────────────────────┐
   │ foreach provider in _providers:                │
   │   values = provider()        // вызов фабрики  │
   │   config.Merge(values)       // слияние        │
   └────────────────────────────────────────────────┘
                                        │
                                        ▼
4. Хранение                         KeyValueConfig
   Dictionary<string, string>       (case-insensitive)
                                        │
                                        ▼
5. Использование
   ├── OneScriptCoreOptions(config)      → парсинг базовых опций
   ├── OneScriptLibraryOptions(config)   → парсинг опций библиотек
   ├── SystemConfigAccessor(providers)   → доступ из BSL
   └── Прямой доступ: config[key]        → получение значения
```

### Детальный поток при запуске oscript

```
Program.Main()
    │
    ├── 1. Парсинг аргументов командной строки
    │
    ├── 2. Определение пути к скрипту (codePath)
    │
    ├── 3. ConsoleHostBuilder.Create(codePath)
    │       │
    │       ├── DefaultEngineBuilder.Create()
    │       │   └── new DefaultEngineBuilder()
    │       │       ├── ConfigurationProviders = new()
    │       │       ├── EnvironmentProviders = new()
    │       │       └── Services = new TinyIocImplementation()
    │       │
    │       ├── .SetupConfiguration(p => {
    │       │       │
    │       │       ├── p.UseSystemConfigFile()
    │       │       │   └── providers.Add(CfgFileConfigProvider("D:/oscript/bin/oscript.cfg"))
    │       │       │
    │       │       ├── p.UseEntrypointConfigFile(codePath)
    │       │       │   └── providers.Add(CfgFileConfigProvider("D:/scripts/oscript.cfg"))
    │       │       │
    │       │       └── p.UseEnvironmentVariableConfig("OSCRIPT_CONFIG")
    │       │           └── providers.Add(FormatStringConfigProvider("lib.system=..."))
    │       │   })
    │       │   └── Services.RegisterSingleton(ConfigurationProviders)
    │       │
    │       ├── .SetDefaultOptions()
    │       │   ├── Services.Register<KeyValueConfig>(sp => {
    │       │   │       var providers = sp.Resolve<ConfigurationProviders>();
    │       │   │       return providers.CreateConfig();  // ◄── СОЗДАНИЕ КОНФИГУРАЦИИ
    │       │   │   })
    │       │   │
    │       │   └── Services.Register<OneScriptCoreOptions>(sp => {
    │       │           var cfg = sp.Resolve<KeyValueConfig>();
    │       │           return new OneScriptCoreOptions(cfg);
    │       │       })
    │       │
    │       ├── .UseImports()
    │       ├── .UseFileSystemLibraries()
    │       │   └── Services.RegisterSingleton<IDependencyResolver>(sp => {
    │       │           var libOptions = sp.Resolve<OneScriptLibraryOptions>();
    │       │           // использует libOptions.SystemLibraryDir
    │       │           // использует libOptions.AdditionalLibraries
    │       │       })
    │       │
    │       ├── .UseNativeRuntime()
    │       └── .UseEventHandlers()
    │
    ├── 4. ConsoleHostBuilder.Build(builder)
    │       │
    │       └── builder.Build()
    │           │
    │           ├── container = Services.CreateContainer()  // ◄── РАЗРЕШЕНИЕ ЗАВИСИМОСТЕЙ
    │           │   ├── Resolve<ConfigurationProviders>    (singleton)
    │           │   ├── Resolve<KeyValueConfig>()
    │           │   │   └── providers.CreateConfig()       // вызов провайдеров
    │           │   │       ├── provider1() → dict1
    │           │   │       ├── config.Merge(dict1)
    │           │   │       ├── provider2() → dict2
    │           │   │       ├── config.Merge(dict2)        // перезапись значений
    │           │   │       ├── provider3() → dict3
    │           │   │       └── config.Merge(dict3)        // финальная перезапись
    │           │   │
    │           │   ├── Resolve<OneScriptCoreOptions>()
    │           │   │   └── new OneScriptCoreOptions(KeyValueConfig)
    │           │   │       ├── SystemLanguage = config["SystemLanguage"]
    │           │   │       ├── FileReaderEncoding = parse(config["encoding.script"])
    │           │   │       └── ...
    │           │   │
    │           │   └── Resolve<ScriptingEngine>()
    │           │
    │           ├── engine = container.Resolve<ScriptingEngine>()
    │           ├── env = container.Resolve<ExecutionContext>()
    │           ├── EnvironmentProviders.Invoke(env)
    │           └── return engine
    │
    └── 5. Исполнение скрипта с использованием конфигурации
```

### Поток данных при чтении файла конфигурации

```
CfgFileConfigProvider.GetProvider()()
    │
    ├── 1. Открытие файла
    │   └── new StreamReader(FilePath, detectEncoding: true)
    │
    ├── 2. Построчное чтение
    │   │
    │   └── while (!reader.EndOfStream)
    │       │
    │       ├── line = reader.ReadLine()
    │       │
    │       ├── Пропуск пустых строк и комментариев
    │       │   if (IsNullOrWhiteSpace(line) || line[0] == '#')
    │       │       continue
    │       │
    │       ├── Разбор пары ключ-значение
    │       │   keyValue = line.Split('=', maxCount: 2)
    │       │   if (keyValue.Length != 2)
    │       │       continue  // пропуск некорректных строк
    │       │
    │       └── Сохранение в словарь
    │           conf[keyValue[0].Trim()] = keyValue[1].Trim()
    │
    ├── 3. Обработка относительных путей
    │   │
    │   └── ExpandRelativePaths(conf, FilePath)
    │       │
    │       ├── Обработка lib.system
    │       │   if (conf.ContainsKey("lib.system"))
    │       │       path = conf["lib.system"]
    │       │       if (!Path.IsPathRooted(path))
    │       │           configDir = Path.GetDirectoryName(FilePath)
    │       │           absolutePath = Path.GetFullPath(Path.Combine(configDir, path))
    │       │           conf["lib.system"] = absolutePath
    │       │
    │       └── Обработка lib.additional
    │           if (conf.ContainsKey("lib.additional"))
    │               paths = conf["lib.additional"].Split(';')
    │               absolutePaths = []
    │               foreach path in paths:
    │                   absolutePath = Path.GetFullPath(Path.Combine(configDir, path))
    │                   absolutePaths.Add(absolutePath)
    │               conf["lib.additional"] = string.Join(";", absolutePaths)
    │
    └── 4. Возврат словаря
        return conf : IDictionary<string, string>
```

### Поток данных при чтении переменной окружения

```
FormatStringConfigProvider.GetProvider()()
    │
    ├── 1. Получение строки (из переменной окружения)
    │   valuesString = Environment.GetEnvironmentVariable("OSCRIPT_CONFIG")
    │   // например: "lib.system=./modules;encoding.script=utf-8"
    │
    ├── 2. Парсинг через FormatParametersList
    │   │
    │   └── new FormatParametersList(valuesString)
    │       │
    │       ├── Разбор по точке с запятой ';'
    │       │   parameters = valuesString.Split(';')
    │       │
    │       └── Для каждого параметра
    │           ├── parts = param.Split('=', maxCount: 2)
    │           ├── name = parts[0].Trim()
    │           ├── value = parts[1].Trim()
    │           └── _paramList.Add(new FormatParameter(name, value))
    │
    ├── 3. Конвертация в словарь
    │   └── parametersList.ToDictionary()
    │       └── return Dictionary<string, string> { {name1, value1}, ... }
    │
    └── 4. Возврат словаря
        return dict : IDictionary<string, string>
```

---

## Жизненный цикл конфигурации

### Фазы жизненного цикла

```
┌──────────────────────────────────────────────────────────────────┐
│  ФАЗА 1: ИНИЦИАЛИЗАЦИЯ (Initialization)                          │
│  Время: При создании IEngineBuilder                              │
│  Место: ConsoleHostBuilder.Create() / Program.Main()             │
└──────────────────────────────────────────────────────────────────┘
    │
    ├── 1.1 Создание DefaultEngineBuilder
    │       └── new ConfigurationProviders()
    │
    ├── 1.2 Настройка провайдеров через SetupConfiguration()
    │       ├── UseSystemConfigFile()
    │       ├── UseEntrypointConfigFile()
    │       └── UseEnvironmentVariableConfig()
    │
    └── 1.3 Регистрация в DI
            └── Services.RegisterSingleton(ConfigurationProviders)

┌──────────────────────────────────────────────────────────────────┐
│  ФАЗА 2: РЕГИСТРАЦИЯ ЗАВИСИМОСТЕЙ (Dependency Registration)      │
│  Время: После настройки провайдеров, до Build()                  │
│  Место: BuildUpWithIoC() / SetDefaultOptions()                   │
└──────────────────────────────────────────────────────────────────┘
    │
    ├── 2.1 Регистрация KeyValueConfig
    │       └── Services.Register<KeyValueConfig>(factory)
    │           factory: sp => sp.Resolve<ConfigurationProviders>().CreateConfig()
    │
    ├── 2.2 Регистрация OneScriptCoreOptions
    │       └── Services.Register<OneScriptCoreOptions>(factory)
    │           factory: sp => new OneScriptCoreOptions(sp.Resolve<KeyValueConfig>())
    │
    └── 2.3 Регистрация OneScriptLibraryOptions
            └── Services.Register<OneScriptLibraryOptions>(factory)
                factory: sp => new OneScriptLibraryOptions(sp.Resolve<KeyValueConfig>())

┌──────────────────────────────────────────────────────────────────┐
│  ФАЗА 3: ПОСТРОЕНИЕ ДВИЖКА (Engine Building)                     │
│  Время: При вызове builder.Build()                               │
│  Место: DefaultEngineBuilder.Build()                             │
└──────────────────────────────────────────────────────────────────┘
    │
    ├── 3.1 Создание DI-контейнера
    │       └── container = Services.CreateContainer()
    │
    ├── 3.2 Разрешение зависимостей (ЗДЕСЬ ПРОИСХОДИТ ЗАГРУЗКА КОНФИГУРАЦИИ)
    │       │
    │       ├── Resolve<ConfigurationProviders>  (singleton, уже есть)
    │       │
    │       ├── Resolve<KeyValueConfig>
    │       │   └── ConfigurationProviders.CreateConfig()
    │       │       ├── Вызов provider1() → чтение oscript.cfg (system)
    │       │       ├── config.Merge(values1)
    │       │       ├── Вызов provider2() → чтение oscript.cfg (entrypoint)
    │       │       ├── config.Merge(values2)
    │       │       ├── Вызов provider3() → чтение OSCRIPT_CONFIG
    │       │       └── config.Merge(values3)
    │       │
    │       ├── Resolve<OneScriptCoreOptions>
    │       │   └── new OneScriptCoreOptions(KeyValueConfig)
    │       │       └── Парсинг значений из конфигурации
    │       │
    │       └── Resolve<ScriptingEngine>
    │           └── Использует OneScriptCoreOptions
    │
    └── 3.3 Инициализация окружения
            └── EnvironmentProviders.Invoke(env)

┌──────────────────────────────────────────────────────────────────┐
│  ФАЗА 4: ИСПОЛНЕНИЕ (Runtime)                                    │
│  Время: Во время работы движка                                   │
│  Место: Выполнение скриптов                                      │
└──────────────────────────────────────────────────────────────────┘
    │
    ├── 4.1 Использование опций
    │       ├── FileReaderEncoding → чтение файлов скриптов
    │       ├── SystemLibraryDir → поиск модулей
    │       └── PreprocessorDefinitions → препроцессинг
    │
    └── 4.2 Доступ из скриптов (опционально)
            └── SystemConfigAccessor
                ├── GetSystemOptionValue(key)
                └── RefreshSystemConfig()  // пересоздание конфигурации

┌──────────────────────────────────────────────────────────────────┐
│  ФАЗА 5: ОБНОВЛЕНИЕ (Optional Refresh)                           │
│  Время: По требованию                                            │
│  Место: SystemConfigAccessor.Refresh() / EngineConfigProvider    │
└──────────────────────────────────────────────────────────────────┘
    │
    └── 5.1 Пересоздание конфигурации
            └── ConfigurationProviders.CreateConfig()
                └── Повторное чтение всех источников
```

### Временная диаграмма

```
Time  │ Component                  │ Action
──────┼────────────────────────────┼──────────────────────────────────────
  0ms │ Program.Main()             │ Старт приложения
      │                            │
  1ms │ ConsoleHostBuilder         │ Create(codePath)
      │   └─ DefaultEngineBuilder  │   new ConfigurationProviders()
      │                            │
  2ms │ SetupConfiguration         │ Настройка провайдеров
      │   ├─ UseSystemConfigFile() │   Add(provider1)
      │   ├─ UseEntrypointConfig() │   Add(provider2)
      │   └─ UseEnvironmentVar()   │   Add(provider3)
      │                            │
  3ms │ SetDefaultOptions          │ Регистрация в DI
      │   └─ Register<KeyValueCfg> │   Регистрация фабрики
      │                            │
  5ms │ builder.Build()            │ Создание движка
      │   └─ CreateContainer()     │   Построение DI-контейнера
      │                            │
  6ms │ Resolve<KeyValueConfig>    │ ◄── ПЕРВОЕ СОЗДАНИЕ КОНФИГУРАЦИИ
      │   └─ CreateConfig()        │
      │       ├─ provider1()       │   Чтение system oscript.cfg
  7ms │       │   └─ ReadFile()    │   I/O: 1-5ms
  8ms │       ├─ Merge(values1)    │   Запись в KeyValueConfig
      │       │                    │
  9ms │       ├─ provider2()       │   Чтение entrypoint oscript.cfg
 10ms │       │   └─ ReadFile()    │   I/O: 1-5ms (если файл существует)
 11ms │       ├─ Merge(values2)    │   Перезапись значений
      │       │                    │
 12ms │       ├─ provider3()       │   Парсинг OSCRIPT_CONFIG
 13ms │       │   └─ Parse()       │   CPU: <1ms
 14ms │       └─ Merge(values3)    │   Финальная перезапись (высший приоритет)
      │                            │
 15ms │ Resolve<CoreOptions>       │ Создание опций из конфигурации
      │   └─ new CoreOptions(cfg)  │   Парсинг значений, создание объектов
      │                            │
 20ms │ Resolve<ScriptingEngine>   │ Создание движка
      │                            │
100ms │ Runtime                    │ Исполнение скрипта
      │   └─ Use CoreOptions       │   Использование конфигурации
      │                            │
500ms │ (optional) Refresh()       │ ◄── ПОВТОРНОЕ СОЗДАНИЕ КОНФИГУРАЦИИ
      │   └─ CreateConfig()        │   Повторное чтение всех источников
```

---

## Источники конфигурации

### 1. Системный файл конфигурации

**Местоположение**: `{директория_движка}/oscript.cfg`

**Определение директории**:
1. Получает путь к сборке `IValue` (ScriptEngine.dll)
2. Если путь пустой, использует путь к исполняемому файлу (oscript.exe)
3. Извлекает директорию из этого пути

**Пример пути**:
- Windows: `C:\Program Files\OneScript\oscript.cfg`
- Linux: `/usr/lib/onescript/oscript.cfg`

**Назначение**: Глобальные настройки для всех скриптов, запускаемых данной установкой OneScript.

**Характеристики**:
- Требуется: `Required = true` (ошибка при отсутствии файла)
- Относительные пути разрешаются относительно директории движка
- Приоритет: **самый низкий**

### 2. Локальный файл конфигурации (Entrypoint)

**Местоположение**: `{директория_скрипта}/oscript.cfg`

**Определение директории**:
1. Извлекает директорию из пути к точке входа (скрипту)
2. Формирует полный путь к `oscript.cfg`
3. Проверяет существование файла

**Пример пути**:
- Скрипт: `D:\projects\myapp\main.os`
- Конфиг: `D:\projects\myapp\oscript.cfg`

**Назначение**: Настройки конкретного проекта/скрипта, перезаписывают системные настройки.

**Характеристики**:
- Требуется: `Required = false` (отсутствие файла не является ошибкой)
- Относительные пути разрешаются относительно директории скрипта
- Приоритет: **средний**

### 3. Переменная окружения OSCRIPT_CONFIG

**Имя переменной**: `OSCRIPT_CONFIG`

**Формат значения**: `key1=value1;key2=value2;key3=value3`

**Примеры**:
```bash
# Linux/macOS
export OSCRIPT_CONFIG="lib.system=/opt/oscript/lib;encoding.script=utf-8"

# Windows PowerShell
$env:OSCRIPT_CONFIG="lib.system=C:\oscript\lib;encoding.script=utf-8"

# Windows CMD
set OSCRIPT_CONFIG=lib.system=C:\oscript\lib;encoding.script=utf-8
```

**Назначение**: 
- Временное переопределение настроек для текущего сеанса
- CI/CD пайплайны
- Контейнеризация (Docker, Kubernetes)
- Тестирование с различными конфигурациями

**Характеристики**:
- Опциональная: если переменная не установлена, провайдер не добавляется
- Абсолютные пути рекомендуются
- Приоритет: **самый высокий** (перезаписывает все файлы конфигурации)

### Сравнительная таблица

| Характеристика        | Системный файл         | Локальный файл         | Переменная окружения  |
|-----------------------|------------------------|------------------------|-----------------------|
| **Путь**              | {движок}/oscript.cfg   | {скрипт}/oscript.cfg   | OSCRIPT_CONFIG        |
| **Приоритет**         | 1 (низкий)             | 2 (средний)            | 3 (высокий)           |
| **Required**          | true                   | false                  | false                 |
| **Формат**            | файл key=value         | файл key=value         | строка key=val;...    |
| **Область действия**  | глобальная             | проект/директория      | процесс/окружение     |
| **Пути**              | относительно движка    | относительно скрипта   | абсолютные            |
| **Когда использовать**| установка по умолчанию | настройки проекта      | переопределения       |

---

## Порядок приоритетов

### Механизм определения приоритета

Приоритет определяется **порядком добавления провайдеров** через метод `ConfigurationProviders.Add()`.

**Правило**: Последний добавленный провайдер имеет наивысший приоритет.

**Причина**: Метод `Merge()` перезаписывает существующие ключи новыми значениями:

```csharp
public void Merge(IDictionary<string, string> source)
{
    foreach (var keyValue in source)
    {
        this[keyValue.Key] = keyValue.Value;  // перезапись
    }
}
```

### Текущий порядок (после исправления bug #1657)

```csharp
// src/oscript/ConsoleHostBuilder.cs
.SetupConfiguration(p =>
{
    p.UseSystemConfigFile()           // 1-й: самый низкий приоритет
        .UseEntrypointConfigFile()    // 2-й: средний приоритет
        .UseEnvironmentVariableConfig(); // 3-й: самый высокий приоритет
})
```

### Визуализация приоритетов

```
                        ПРИОРИТЕТ
                    низкий → высокий
                    
┌──────────────────────────────────────────────────────────┐
│                                                          │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   │
│  │  Системный  │   │  Локальный  │   │  Переменная │   │
│  │    файл     │ → │    файл     │ → │  окружения  │   │
│  │ oscript.cfg │   │ oscript.cfg │   │OSCRIPT_CONFIG│   │
│  └─────────────┘   └─────────────┘   └─────────────┘   │
│        │                  │                  │           │
│        │                  │                  │           │
│        └──────────────────┴──────────────────┘           │
│                          │                               │
│                 ConfigurationProviders                   │
│                     .CreateConfig()                      │
│                          │                               │
│                          ▼                               │
│               ┌──────────────────────┐                   │
│               │   KeyValueConfig     │                   │
│               │  (итоговый словарь)  │                   │
│               └──────────────────────┘                   │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### Пример перезаписи значений

**Сценарий**: Настройка `lib.system` определена во всех трех источниках.

**Системный oscript.cfg** (C:\Program Files\OneScript\oscript.cfg):
```ini
lib.system=C:\Program Files\OneScript\lib
encoding.script=utf-8
```

**Локальный oscript.cfg** (D:\myproject\oscript.cfg):
```ini
lib.system=D:\myproject\modules
```

**Переменная окружения**:
```
OSCRIPT_CONFIG=lib.system=C:\temp\testmodules
```

**Процесс слияния**:

```
Шаг 1: Чтение системного файла
    config["lib.system"] = "C:\Program Files\OneScript\lib"
    config["encoding.script"] = "utf-8"

Шаг 2: Слияние с локальным файлом (ПЕРЕЗАПИСЬ lib.system)
    config.Merge({
        "lib.system": "D:\myproject\modules"
    })
    →
    config["lib.system"] = "D:\myproject\modules"  ◄ перезаписано
    config["encoding.script"] = "utf-8"            ◄ сохранено

Шаг 3: Слияние с переменной окружения (ФИНАЛЬНАЯ ПЕРЕЗАПИСЬ)
    config.Merge({
        "lib.system": "C:\temp\testmodules"
    })
    →
    config["lib.system"] = "C:\temp\testmodules"  ◄ финальное значение
    config["encoding.script"] = "utf-8"           ◄ сохранено
```

**Итоговая конфигурация**:
```ini
lib.system=C:\temp\testmodules        # из переменной окружения
encoding.script=utf-8                 # из системного файла
```

### Матрица приоритетов по ключам

| Ключ конфигурации   | Системный | Локальный | Переменная | Финальное значение |
|---------------------|-----------|-----------|------------|--------------------|
| lib.system          | ✓         | ✓         | ✓          | Переменная         |
| lib.additional      | ✓         | —         | —          | Системный          |
| encoding.script     | ✓         | —         | ✓          | Переменная         |
| SystemLanguage      | —         | ✓         | —          | Локальный          |
| custom.key          | —         | —         | ✓          | Переменная         |

Легенда: ✓ = определен, — = не определен

---

## Использование конфигурации

### 1. Через классы опций (основной способ)

#### Регистрация и разрешение

```csharp
// Регистрация в DI (SetDefaultOptions)
builder.Services.Register<OneScriptCoreOptions>(sp =>
{
    var cfg = sp.Resolve<KeyValueConfig>();
    return new OneScriptCoreOptions(cfg);
});

// Использование в коде
public class SomeComponent
{
    private readonly OneScriptCoreOptions _options;
    
    public SomeComponent(OneScriptCoreOptions options)
    {
        _options = options;
    }
    
    public void DoSomething()
    {
        var encoding = _options.FileReaderEncoding;
        var language = _options.SystemLanguage;
    }
}
```

#### Доступные опции

**OneScriptCoreOptions**:
- `SystemLanguage`: язык системных сообщений
- `FileReaderEncoding`: кодировка для чтения скриптов
- `UseNativeAsDefaultRuntime`: использовать ли native runtime по умолчанию
- `PreprocessorDefinitions`: список определений для препроцессора
- `ExplicitImports`: режим явных импортов

**OneScriptLibraryOptions** (наследует OneScriptCoreOptions):
- `SystemLibraryDir`: путь к системной библиотеке
- `AdditionalLibraries`: дополнительные пути библиотек

### 2. Прямой доступ к KeyValueConfig

```csharp
public class CustomComponent
{
    private readonly KeyValueConfig _config;
    
    public CustomComponent(KeyValueConfig config)
    {
        _config = config;
    }
    
    public string GetCustomOption()
    {
        var value = _config["my.custom.key"];
        return value ?? "default_value";
    }
}
```

**Особенности**:
- Доступ по индексу возвращает `string` или `null`
- Ключи регистронезависимые
- Нет валидации значений
- Подходит для кастомных/расширенных опций

### 3. Из BSL-скриптов через SystemConfigAccessor

```bsl
// Получение значения
Значение = ПолучитьЗначениеСистемнойНастройки("lib.system");

// Обновление конфигурации (перечитывание файлов)
ОбновитьНастройкиСистемы();

// Повторное получение
НовоеЗначение = ПолучитьЗначениеСистемнойНастройки("lib.system");
```

**Возможности**:
- Доступ к любым конфигурационным ключам из скрипта
- Динамическое обновление конфигурации
- Возвращает `Неопределено` если ключ не найден

### 4. Через EngineConfigProvider (редкий случай)

```csharp
public class ComponentWithRefreshableConfig
{
    private readonly EngineConfigProvider _configProvider;
    
    public ComponentWithRefreshableConfig(EngineConfigProvider provider)
    {
        _configProvider = provider;
    }
    
    public void UseLatestConfig()
    {
        var config = _configProvider.ReadConfig();  // всегда актуальная
        var value = config["some.key"];
    }
}
```

**Отличие от KeyValueConfig**:
- `KeyValueConfig` из DI — это snapshot (снимок) на момент создания
- `EngineConfigProvider.ReadConfig()` — всегда пересоздает конфигурацию

---

## Диаграммы и схемы

### Диаграмма последовательности (Sequence Diagram)

```
Program   ConsoleHost  DefaultEngine  ConfigProviders  KeyValueConfig  CfgFileProvider  FormatStringProvider
   │           │            Builder         │                │              │                    │
   │ Create()  │               │            │                │              │                    │
   │───────────>               │            │                │              │                    │
   │           │ Create()      │            │                │              │                    │
   │           │──────────────>│            │                │              │                    │
   │           │               │ new()      │                │              │                    │
   │           │               │───────────>│                │              │                    │
   │           │               │<───────────│                │              │                    │
   │           │               │            │                │              │                    │
   │           │ SetupConfiguration(...)    │                │              │                    │
   │           │──────────────────────────> │                │              │                    │
   │           │               │ Add(prov1) │                │              │                    │
   │           │               │───────────>│                │              │                    │
   │           │               │ Add(prov2) │                │              │                    │
   │           │               │───────────>│                │              │                    │
   │           │               │ Add(prov3) │                │              │                    │
   │           │               │───────────>│                │              │                    │
   │           │               │            │                │              │                    │
   │ Build()   │               │            │                │              │                    │
   │───────────────────────────>            │                │              │                    │
   │           │               │ CreateConfig()             │              │                    │
   │           │               │───────────────────────────>│              │                    │
   │           │               │            │ new()         │              │                    │
   │           │               │            │──────────────>│              │                    │
   │           │               │            │               │              │                    │
   │           │               │            │ provider1()   │ ReadFile()   │                    │
   │           │               │            │──────────────────────────────>                    │
   │           │               │            │               │ dict1        │                    │
   │           │               │            │<──────────────────────────────                    │
   │           │               │            │ Merge(dict1)  │              │                    │
   │           │               │            │──────────────>│              │                    │
   │           │               │            │               │              │                    │
   │           │               │            │ provider2()   │ ReadFile()   │                    │
   │           │               │            │──────────────────────────────>                    │
   │           │               │            │               │ dict2        │                    │
   │           │               │            │<──────────────────────────────                    │
   │           │               │            │ Merge(dict2)  │              │                    │
   │           │               │            │──────────────>│              │                    │
   │           │               │            │               │              │                    │
   │           │               │            │ provider3()   │              │ Parse()            │
   │           │               │            │───────────────────────────────────────────────────>
   │           │               │            │               │              │ dict3              │
   │           │               │            │<───────────────────────────────────────────────────
   │           │               │            │ Merge(dict3)  │              │                    │
   │           │               │            │──────────────>│              │                    │
   │           │               │            │               │              │                    │
   │           │               │            │ config        │              │                    │
   │           │               │<───────────────────────────│              │                    │
   │           │               │            │               │              │                    │
   │<───────────────────────────            │               │              │                    │
```

### Диаграмма компонентов

```
┌────────────────────────────────────────────────────────────────────────┐
│                        OneScript Application                           │
│                                                                        │
│  ┌──────────────────────────────────────────────────────────────────┐ │
│  │                      oscript.exe (CLI)                           │ │
│  │                    ┌────────────────────┐                         │ │
│  │                    │ ConsoleHostBuilder │                         │ │
│  │                    └─────────┬──────────┘                         │ │
│  └──────────────────────────────┼───────────────────────────────────┘ │
│                                 │                                     │
│  ┌──────────────────────────────┼───────────────────────────────────┐ │
│  │            ScriptEngine.Hosting (ядро конфигурации)              │ │
│  │                              │                                   │ │
│  │  ┌───────────────────────────▼────────────────────────┐          │ │
│  │  │           DefaultEngineBuilder                     │          │ │
│  │  │  ┌──────────────────────────────────────────────┐  │          │ │
│  │  │  │     ConfigurationProviders                   │  │          │ │
│  │  │  │  - List<Func<IDictionary>>                   │  │          │ │
│  │  │  │  + Add(provider)                             │  │          │ │
│  │  │  │  + CreateConfig() : KeyValueConfig           │  │          │ │
│  │  │  └──────────────────────┬───────────────────────┘  │          │ │
│  │  │                         │                           │          │ │
│  │  │                         │ creates                   │          │ │
│  │  │                         ▼                           │          │ │
│  │  │         ┌────────────────────────────┐              │          │ │
│  │  │         │     KeyValueConfig         │              │          │ │
│  │  │         │  Dictionary<string,string> │              │          │ │
│  │  │         └────────────────────────────┘              │          │ │
│  │  └─────────────────────────────────────────────────────┘          │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │      ScriptEngine.HostedScript (провайдеры конфигурации)        │ │
│  │                                                                 │ │
│  │  ┌──────────────────┐  ┌──────────────────┐                    │ │
│  │  │ IConfigProvider  │  │ IConfigProvider  │                    │ │
│  │  ├──────────────────┤  ├──────────────────┤                    │ │
│  │  │CfgFileConfig-    │  │FormatString-     │                    │ │
│  │  │Provider          │  │ConfigProvider    │                    │ │
│  │  │                  │  │                  │                    │ │
│  │  │+ FilePath        │  │+ ValuesString    │                    │ │
│  │  │+ Required        │  │                  │                    │ │
│  │  │+ GetProvider()   │  │+ GetProvider()   │                    │ │
│  │  │  ↓ returns       │  │  ↓ returns       │                    │ │
│  │  │ Func<IDict>      │  │ Func<IDict>      │                    │ │
│  │  └──────────────────┘  └──────────────────┘                    │ │
│  │           │                     │                               │ │
│  │           └─────────┬───────────┘                               │ │
│  │                     │                                           │ │
│  │                     │ registered in                             │ │
│  │                     ▼                                           │ │
│  │      ┌──────────────────────────────┐                          │ │
│  │      │  EngineBuilderExtensions     │                          │ │
│  │      │  + UseSystemConfigFile()     │                          │ │
│  │      │  + UseEntrypointConfigFile() │                          │ │
│  │      │  + UseEnvironmentVar...()    │                          │ │
│  │      └──────────────────────────────┘                          │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │              ScriptEngine (использование конфигурации)          │ │
│  │                                                                 │ │
│  │  ┌──────────────────────┐    ┌──────────────────────────┐      │ │
│  │  │OneScriptCoreOptions  │    │OneScriptLibraryOptions   │      │ │
│  │  │                      │    │                          │      │ │
│  │  │+ SystemLanguage      │    │+ SystemLibraryDir        │      │ │
│  │  │+ FileReaderEncoding  │    │+ AdditionalLibraries     │      │ │
│  │  │+ ...                 │    │                          │      │ │
│  │  └──────────────────────┘    └──────────────────────────┘      │ │
│  │                                                                 │ │
│  │  ┌──────────────────────────────────────────────┐              │ │
│  │  │      SystemConfigAccessor                    │              │ │
│  │  │      (глобальный контекст для BSL)           │              │ │
│  │  │  + GetSystemOptionValue(key) : IValue        │              │ │
│  │  │  + RefreshSystemConfig()                     │              │ │
│  │  └──────────────────────────────────────────────┘              │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘

   External Sources
┌──────────────────────┐
│ System oscript.cfg   │──────┐
└──────────────────────┘      │
                              ├───> CfgFileConfigProvider
┌──────────────────────┐      │
│ Local oscript.cfg    │──────┘
└──────────────────────┘

┌──────────────────────┐
│ OSCRIPT_CONFIG env   │─────────> FormatStringConfigProvider
└──────────────────────┘
```

---

## Точки расширения

### 1. Создание кастомного провайдера конфигурации

Для добавления нового источника конфигурации (например, база данных, удаленный сервер, Azure Key Vault):

```csharp
// 1. Реализовать IConfigProvider
public class DatabaseConfigProvider : IConfigProvider
{
    public string ConnectionString { get; set; }
    public string TableName { get; set; }
    
    public Func<IDictionary<string, string>> GetProvider()
    {
        var connString = ConnectionString;  // локальная копия для замыкания
        var table = TableName;
        
        return () => ReadFromDatabase(connString, table);
    }
    
    private IDictionary<string, string> ReadFromDatabase(string conn, string table)
    {
        var config = new Dictionary<string, string>();
        
        using (var connection = new SqlConnection(conn))
        {
            connection.Open();
            var command = new SqlCommand($"SELECT [Key], [Value] FROM {table}", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    config[reader.GetString(0)] = reader.GetString(1);
                }
            }
        }
        
        return config;
    }
}

// 2. Создать метод расширения для удобства
public static class CustomEngineBuilderExtensions
{
    public static ConfigurationProviders UseDatabaseConfig(
        this ConfigurationProviders providers,
        string connectionString,
        string tableName)
    {
        var provider = new DatabaseConfigProvider
        {
            ConnectionString = connectionString,
            TableName = tableName
        };
        
        providers.Add(provider.GetProvider());
        return providers;
    }
}

// 3. Использовать в коде настройки
builder.SetupConfiguration(p =>
{
    p.UseSystemConfigFile()
     .UseEntrypointConfigFile(codePath)
     .UseDatabaseConfig("Server=...;Database=...", "Config")  // кастомный провайдер
     .UseEnvironmentVariableConfig("OSCRIPT_CONFIG");
});
```

**Важно**: Порядок добавления определяет приоритет!

### 2. Расширение OneScriptCoreOptions

Для добавления новых конфигурационных опций:

```csharp
// 1. Создать класс-наследник
public class MyCustomOptions : OneScriptCoreOptions
{
    private const string MY_CUSTOM_KEY = "my.custom.option";
    private const string ANOTHER_KEY = "another.option";
    
    public MyCustomOptions(KeyValueConfig config) : base(config)
    {
        MyCustomOption = config[MY_CUSTOM_KEY] ?? "default_value";
        AnotherOption = ParseAnotherOption(config[ANOTHER_KEY]);
    }
    
    public string MyCustomOption { get; }
    public int AnotherOption { get; }
    
    private static int ParseAnotherOption(string value)
    {
        return int.TryParse(value, out var result) ? result : 42;
    }
}

// 2. Зарегистрировать в DI
builder.Services.Register<MyCustomOptions>(sp =>
{
    var cfg = sp.Resolve<KeyValueConfig>();
    return new MyCustomOptions(cfg);
});

// 3. Использовать через DI
public class MyComponent
{
    public MyComponent(MyCustomOptions options)
    {
        var customValue = options.MyCustomOption;
        var anotherValue = options.AnotherOption;
    }
}
```

### 3. Обработка конфигурации после загрузки

Для валидации или трансформации конфигурации:

```csharp
// 1. Создать обработчик
public class ConfigurationPostProcessor
{
    public KeyValueConfig Process(KeyValueConfig config)
    {
        // Валидация
        var libSystem = config["lib.system"];
        if (libSystem != null && !Directory.Exists(libSystem))
        {
            throw new ConfigurationException($"Library directory not found: {libSystem}");
        }
        
        // Трансформация
        var processedConfig = new KeyValueConfig(config);
        
        // Добавление вычисляемых значений
        var basePath = config["base.path"];
        if (basePath != null)
        {
            processedConfig["computed.full.path"] = 
                Path.Combine(basePath, "subdir", "file.txt");
        }
        
        return processedConfig;
    }
}

// 2. Зарегистрировать в DI с обработкой
builder.Services.Register<KeyValueConfig>(sp =>
{
    var providers = sp.Resolve<ConfigurationProviders>();
    var rawConfig = providers.CreateConfig();
    
    var processor = new ConfigurationPostProcessor();
    return processor.Process(rawConfig);
});
```

### 4. Мониторинг изменений конфигурации

Для реакции на изменения файлов конфигурации:

```csharp
public class ConfigurationWatcher : IDisposable
{
    private readonly ConfigurationProviders _providers;
    private readonly FileSystemWatcher _watcher;
    private KeyValueConfig _currentConfig;
    
    public event EventHandler<ConfigChangedEventArgs> ConfigChanged;
    
    public ConfigurationWatcher(ConfigurationProviders providers, string configDirectory)
    {
        _providers = providers;
        _currentConfig = providers.CreateConfig();
        
        _watcher = new FileSystemWatcher(configDirectory, "oscript.cfg");
        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
    }
    
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var newConfig = _providers.CreateConfig();
        var changes = DetectChanges(_currentConfig, newConfig);
        
        _currentConfig = newConfig;
        ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(changes));
    }
    
    private Dictionary<string, ConfigChange> DetectChanges(
        KeyValueConfig oldConfig, 
        KeyValueConfig newConfig)
    {
        // Реализация сравнения конфигураций
        // ...
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
```

---

## Примеры использования

### Пример 1: Настройка конфигурации для тестов

```csharp
[TestClass]
public class ScriptEngineTests
{
    [TestMethod]
    public void TestWithCustomConfig()
    {
        // Создаем тестовую конфигурацию
        var testConfig = new Dictionary<string, string>
        {
            ["lib.system"] = @"C:\test\modules",
            ["encoding.script"] = "utf-8",
            ["SystemLanguage"] = "en"
        };
        
        // Создаем кастомный провайдер
        var testProvider = new TestConfigProvider(testConfig);
        
        // Настраиваем движок
        var builder = DefaultEngineBuilder.Create()
            .SetupConfiguration(p =>
            {
                p.Add(testProvider.GetProvider());  // только тестовая конфигурация
            })
            .SetDefaultOptions();
        
        var engine = builder.Build();
        
        // Используем движок с тестовой конфигурацией
        // ...
    }
}

public class TestConfigProvider : IConfigProvider
{
    private readonly Dictionary<string, string> _config;
    
    public TestConfigProvider(Dictionary<string, string> config)
    {
        _config = config;
    }
    
    public Func<IDictionary<string, string>> GetProvider()
    {
        var localCopy = new Dictionary<string, string>(_config);
        return () => localCopy;
    }
}
```

### Пример 2: Многоуровневая конфигурация для микросервиса

```csharp
public class MicroserviceConfigBuilder
{
    public static IEngineBuilder CreateWithFullConfig(string serviceName, string scriptPath)
    {
        return DefaultEngineBuilder.Create()
            .SetupConfiguration(p =>
            {
                // 1. Глобальные настройки организации (lowest priority)
                p.UseConfigFile("/etc/oscript/global.cfg", required: false);
                
                // 2. Настройки кластера
                p.UseConfigFile($"/etc/oscript/clusters/{GetClusterName()}.cfg", required: false);
                
                // 3. Настройки сервиса
                p.UseConfigFile($"/etc/oscript/services/{serviceName}.cfg", required: false);
                
                // 4. Системные настройки движка
                p.UseSystemConfigFile();
                
                // 5. Локальные настройки скрипта
                p.UseEntrypointConfigFile(scriptPath);
                
                // 6. Secrets из HashiCorp Vault (если доступен)
                if (IsVaultAvailable())
                {
                    p.UseVaultSecrets($"secret/oscript/{serviceName}");
                }
                
                // 7. Переменные окружения (highest priority)
                p.UseEnvironmentVariableConfig("OSCRIPT_CONFIG");
            })
            .SetDefaultOptions()
            .UseImports()
            .UseFileSystemLibraries();
    }
    
    private static string GetClusterName()
    {
        return Environment.GetEnvironmentVariable("CLUSTER_NAME") ?? "default";
    }
    
    private static bool IsVaultAvailable()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAULT_ADDR"));
    }
}
```

### Пример 3: Динамическое переключение конфигурации

```csharp
public class ConfigurableScriptRunner
{
    private readonly ConfigurationProviders _providers;
    private readonly string _scriptPath;
    
    public ConfigurableScriptRunner(string scriptPath)
    {
        _scriptPath = scriptPath;
        _providers = new ConfigurationProviders();
        
        // Настройка базовых провайдеров
        _providers.UseSystemConfigFile()
                  .UseEntrypointConfigFile(scriptPath);
    }
    
    public void RunWithProfile(string profileName)
    {
        // Временно добавляем провайдер для профиля
        var profileConfig = LoadProfile(profileName);
        var tempProvider = new InMemoryConfigProvider(profileConfig);
        
        _providers.Add(tempProvider.GetProvider());
        
        try
        {
            var config = _providers.CreateConfig();
            var engine = BuildEngine(config);
            engine.Execute(_scriptPath);
        }
        finally
        {
            // После выполнения можно удалить временный провайдер
            // (в текущей реализации нет метода Remove, но можно расширить)
        }
    }
    
    private Dictionary<string, string> LoadProfile(string profileName)
    {
        // Загрузка профиля из базы данных, файла, etc.
        var profilePath = $"/profiles/{profileName}.cfg";
        // ...
        return new Dictionary<string, string>();
    }
    
    private ScriptingEngine BuildEngine(KeyValueConfig config)
    {
        var builder = DefaultEngineBuilder.Create();
        // Настройка движка с использованием config
        // ...
        return builder.Build();
    }
}
```

### Пример 4: Конфигурация с секретами (безопасность)

```csharp
public static class SecureConfigurationExtensions
{
    public static ConfigurationProviders UseEncryptedConfig(
        this ConfigurationProviders providers,
        string encryptedFilePath,
        byte[] key)
    {
        var provider = new EncryptedConfigProvider
        {
            FilePath = encryptedFilePath,
            DecryptionKey = key
        };
        
        providers.Add(provider.GetProvider());
        return providers;
    }
}

public class EncryptedConfigProvider : IConfigProvider
{
    public string FilePath { get; set; }
    public byte[] DecryptionKey { get; set; }
    
    public Func<IDictionary<string, string>> GetProvider()
    {
        var localPath = FilePath;
        var localKey = DecryptionKey;
        
        return () =>
        {
            var encryptedData = File.ReadAllBytes(localPath);
            var decryptedData = Decrypt(encryptedData, localKey);
            var configText = Encoding.UTF8.GetString(decryptedData);
            
            return ParseConfig(configText);
        };
    }
    
    private byte[] Decrypt(byte[] data, byte[] key)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            // ... реализация расшифровки
            return data;  // placeholder
        }
    }
    
    private IDictionary<string, string> ParseConfig(string text)
    {
        // Парсинг формата конфигурации
        return new Dictionary<string, string>();
    }
}

// Использование:
builder.SetupConfiguration(p =>
{
    var key = LoadEncryptionKey();  // из Azure Key Vault, AWS Secrets Manager, etc.
    
    p.UseSystemConfigFile()
     .UseEncryptedConfig("/secure/credentials.enc", key)
     .UseEnvironmentVariableConfig("OSCRIPT_CONFIG");
});
```

---

## Известные проблемы и ограничения

### Текущие ограничения

1. **Нет удаления провайдеров**: После добавления провайдера через `Add()` его нельзя удалить
2. **Нет приоритетов явно**: Приоритет определяется только порядком добавления
3. **Нет кеширования провайдеров**: Каждый вызов `CreateConfig()` заново вызывает все провайдеры
4. **Нет валидации**: Конфигурация не валидируется, некорректные значения приводят к ошибкам во время выполнения
5. **Нет типизации**: Все значения - строки, требуется ручной парсинг
6. **Нет событий**: Нет уведомлений об изменении конфигурации

### Решенные проблемы

- **Issue #1657**: Исправлен приоритет применения конфигурации (переменная окружения теперь имеет наивысший приоритет)

### Потенциальные улучшения

1. **Строготипизированная конфигурация**:
   ```csharp
   public interface ITypedConfig<T>
   {
       T Parse(KeyValueConfig config);
       void Validate(T config);
   }
   ```

2. **Поддержка вложенных секций**:
   ```ini
   [library]
   system=./modules
   additional=./lib
   
   [encoding]
   script=utf-8
   output=windows-1251
   ```

3. **Условная загрузка провайдеров**:
   ```csharp
   p.UseConfigFile("dev.cfg", condition: IsDevEnvironment);
   ```

4. **Профили конфигурации**:
   ```csharp
   p.UseProfile("development")  // автоматически загружает dev.cfg, dev.env, etc.
   ```

---

## Заключение

Система конфигурации OneScript предоставляет гибкий и расширяемый механизм настройки параметров движка через множественные источники. Ключевые преимущества:

- **Модульность**: Легко добавлять новые источники конфигурации
- **Приоритеты**: Четкий и предсказуемый порядок применения настроек
- **Интеграция с DI**: Бесшовная интеграция с dependency injection
- **Типобезопасность**: Классы опций обеспечивают типизированный доступ
- **Доступность из скриптов**: BSL-скрипты могут читать конфигурацию

Система готова к расширению для поддержки дополнительных источников конфигурации, таких как базы данных, удаленные сервисы, encrypted storage и другие корпоративные решения.

---

**Документ создан**: 10 февраля 2026  
**Версия OneScript**: 2.0.0+  
**Автор**: AI Assistant (Cursor)  
**Статус**: Актуально после исправления Issue #1657

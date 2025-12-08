# Developer Docs — архитектура и навигация по проекту OneScript (для новых контрибьюторов)

Этот документ помогает быстро разобраться, что лежит в репозитории, зачем это нужно, как устроено внутри и как использовать.

## 1 Картина целиком: из чего состоит OneScript

OneScript — реализация языка, совместимого с синтаксисом 1С/BSL. В основе - стековая виртуальная машина. Проект включает реализацию компилятора, исполняющей среды, системы типов и стандартной библиотеки BSL. Поверх этого — инструменты (CLI, раннер), отладка (DAP), веб-обёртки и API для нативных расширений.

OneScript — открытая реализация языка 1С/BSL поверх .NET, со стековой ВМ и стандартной библиотекой. Сценарии исполняются CLI oscript либо внедряются в приложения через HostedScript.

Слои (сверху вниз):
- Приложения и инструменты:
  - src/oscript — консольный хост, основное приложение;
  - src/VSCode.DebugAdapter — адаптер DAP;
  - src/OneScriptDocumenter — генерация документации;
  - src/TestApp, src/Component — примеры использования.
- Хостинг и сервисы: src/ScriptEngine.HostedScript, src/OneScript.DebugServices, src/OneScript.Web.Server.
- Рантайм (компиляция/исполнение, встроенные функции):
  - src/ScriptEngine (стековая ВМ)
  - src/OneScript.Native (нативный бэкенд).
- Ядро/язык:
  - OneScript.Core (типы/контексты) — основные инфраструктурные компоненты для обоих рантаймов
  - OneScript.Language (лексер/парсер/AST)
- Стандартная библиотека языка BSL:
  - OneScript.StandardLibrary — реализация стандартных прикладных классов (массивы, работа с файлами, сетью и пр.)
- Интеграции: ScriptEngine.NativeApi (Мост к внешним компонентам на C++, совместимым с NativeApi 1С).
- Инструмент генерации автодокументации OneScriptDocumenter

## 2 Быстрый старт для контрибьютора
- Где собирать: решение src/1Script.sln.
- Входной CLI: src/oscript (консольное приложение для запуска .os‑скриптов).
- Запуск тестов: проекты src/Tests/* (C#) и папка tests/* (скрипты .os). 
- Если добавляете новый контекст/тип — обычно правки в OneScript.Core/ScriptEngine/StandardLibrary, плюс модульные тесты.

Псевдонимы API приняты двуязычные: РусИмя/EngName (см. атрибуты ContextClass/Method/Property).

Соглашение по ссылкам: указываются относительные пути репозитория.

## 3 Обзор проектов (назначение, ключевые узлы)

Ниже по каждому проекту — зачем он нужен, где искать основную логику и какие классы отвечают за ключевые задачи.

### 3.1 OneScript.Language — лексер/препроцессор/парсер/AST

Назначение: преобразует исходный BSL‑код в токены и синтаксическое дерево, обрабатывает директивы препроцессора.
Проект сделан максимально независимым и отчуждаемым, и должен иметь возможность использоваться в других решениях, не связанных с 1Script, как просто парсер BSL.

- Где в коде:
  - LexicalAnalysis/* — лексер. DefaultLexer.cs, различные состояния (String/Number/Comment/PreprocessorDirective/etc).
  - SyntaxAnalysis/* — парсер и AST: DefaultBslParser.cs, BslSyntaxWalker.cs, AstNodes/* (ModuleNode, MethodNode, CallNode, TryExceptNode, *LoopNode, Binary/UnaryOperationNode и др.).
  - Препроцессор: PreprocessingLexer.cs, PreprocessorHandlers.cs, RegionDirectiveHandler.cs, ImportDirectivesHandler.cs, ModuleAnnotationDirectiveHandler.cs.
  - Диагностика: CodeError.cs, ErrorPositionInfo.cs, SyntaxErrorException.cs, LocalizedErrors.cs.
- Жизненный цикл:
  1) Лексер производит Lexem с типом/токеном.
  2) Препроцессор обрабатывает директивы (#Если/#Область/#Использовать).
  3) Парсер строит AST (BslSyntaxNode), восстанавливается после ошибок (IErrorRecoveryStrategy).
  4) AST передаётся компилятору (CompilerFrontend) рантайма.
- Точки расширения:
  - собственные директивы препроцессора (IDirectiveHandler -> зарегистрировать в DI);
  - обход AST через BslSyntaxWalker.

На выходе вы получаете ModuleNode/AST, пригодный для компиляции рантаймом/бэкендом или обработки статическим анализатором.

### 3.2 OneScript.Core — система типов, значения, отражение контекстов
Назначение: общий объектный каркас значений BSL, контекстов (объекты/методы/свойства), аннотаций и исключений.

OneScript.Core — система типов и контекстная модель
- Назначение: базовые IValue/BslValue, ссылки на значения, метаданные контекстов (классов/методов/свойств), атрибуты, исключения, символы компилятора.
- Где в коде:
  - Values/* — BslValue и производные: строки/числа/дата/Null/Undefined/Type/Object, сравнения/преобразования; ссылки: IValueReference/Variable/PropertyValueReference/IndexedValueReference.
  - Contexts/* — атрибуты ContextClass/ContextMethod/ContextProperty, GlobalContextAttribute, ScriptConstructorAttribute; построители Bsl*Info, отражение классов, поддержка устаревания (ISupportsDeprecation, DeprecatedNameAttribute).
  - Compilation/Binding/* — SymbolTable, SymbolScope, SymbolBinding, *Symbol интерфейсы.
  - Exceptions/* — RuntimeException, TypeConversionException, PropertyAccessException и др.
- Жизненный цикл контекстов:
  1) ContextDiscoverer (ScriptEngine.Machine.Contexts) сканирует сборки, находит [ContextClass]/[GlobalContext]/[EnumerationType]/[SystemEnum].
  2) Регистрирует типы/глобальные контексты в IRuntimeEnvironment/IGlobalsManager.
  3) Отражение формирует Bsl*Info для рантайма/документации.

### 3.3 ScriptEngine — движок выполнения (стековая ВМ и бэкенд компилятора для стековой машины)

Основная среда исполнения на базе стековой виртуальной машины.

Назначение: организует выполнение скриптов, стек вызовов, области видимости, глобальные функции и интеграцию с отладкой.

- Где в коде:
  - Compiler/* — CompilerFrontend, BackendSelector; StackMachineCodeGenerator (байткод), EvalCompiler; CodeGenerationFlags.
  - Machine/* — StackMachineExecutor, MachineInstance (командный цикл, стек/кадры/исключения/итераторы), ExecutionContext/Frame, BuiltinFunctions, ValueFactory, GlobalInstancesManager. CodeStat/* — статистика покрытия кода.
  - Hosting/* — DefaultEngineBuilder, DI (TinyIoC), EngineBuilderExtensions (регистрация сервисов, предобработчики).
  - ScriptingEngine.cs — фасад движка: загрузка сборок, Initialize, NewProcess, компиляция.
  - ContextValuesMarhaller — маршаллер (преобразователь) типов C# в типы BSL и обратно.
- Точки расширения:
  - дополнительные IExecutorProvider (альтернативные рантаймы);
  - предопределённые интерфейсы/итераторы (Interfaces/Iterables handlers);
  - сбор кода-статистики (ICodeStatCollector)

### 3.4 OneScript.Native — нативный бэкенд (Expression Trees) компилятор/исполнитель, встроенные функции

Альтернативная среда исполнения (не основная). Предоставляет компиляцию BSL в ExpressionTrees фреймворка .NET.
Данная среда исполнения имеет ряд ограничений и в целом является экспериментом. В какой именно среде будет исполнен скрипт решает директива в начале скрипта: 

* `#native` - скрипт будет скомпилировать в Native Runtime
* `#stack` или отсутствие директивы - скрипт будет использован основной средой исполнения.

Назначение: преобразует AST+символы в исполняемую форму и предоставляет нативный бэкенд выполнения, включая встроенные функции.
- Компиляция: Compiler/
  - ModuleCompiler.cs, MethodCompiler.cs — генерация модулей/методов.
  - ExpressionTreeGeneratorBase.cs, BinaryOperationCompiler.cs — выражения/операции.
  - BuiltInFunctionsCache.cs, ContextMethodsCache.cs — кеширование метаданных/встроенных функций.
- Исполнение: Runtime/
  - NativeExecutorProvider.cs — провайдер исполнителя.
  - BuiltInFunctions.cs — реализации встроенных функций.
  - DynamicOperations.cs — динамические операции.

- Ограничения: может не поддерживать полный паритет со стековой машиной; выбор режима делает CompilerBackendSelector по соответствующим директивам в начале файла.

7. ScriptEngine.HostedScript — хостинг, загрузка библиотек, конфигурация
- Назначение: безопасная обвязка движка для встраивания, инициализация стандартного глобального контекста, загрузка библиотек, конфигурирование.
- Где в коде:
  - HostedScriptEngine.cs — инициализация, глобальные контексты (SystemGlobalContext, DynamicLoadingFunctions), создание процессов.
  - LibraryLoader.cs — package-loader.os, подключение .os модулей/классов/макетов; FileSystemDependencyResolver.cs — поиск библиотек, цикл обработки, защита от циклических зависимостей.
  - Extensions/EngineBuilderExtensions.cs — UseSystemConfigFile/UseEnvironmentVariableConfig/UseEntrypointConfigFile; UseImports/UseFileSystemLibraries/UseNativeRuntime/UseEventHandlers.
- Жизненный цикл:
  1) Читает настройки из системного `oscript.cfg`, env, и файла `oscript.cfg` рядом с entrypoint.
  2) Инициализация HostedScriptEngine → глобальные объекты → процесс → компиляция/исполнение модуля.
  3) Загрузка библиотек: default или кастомный package-loader.os, последующая регистрация символов и компиляция задержанных модулей.

### 3.6 OneScript.StandardLibrary — стандартная библиотека
Назначение: коллекции, файлы/потоки, текст и кодировки, сеть/HTTP, JSON/XML, ZIP, процессы, таймзоны и др.
- Коллекции: Collections/
  - ArrayImpl.cs, MapImpl.cs, StructureImpl.cs, ValueListImpl.cs.
  - Таблицы/деревья значений: ValueTable.cs (+ ValueTableColumn/Row), ValueTree.cs.
- Файлы/потоки/текст: FileOperations.cs, FileContext.cs, Text/* (TextReadImpl.cs, TextWriteImpl.cs), CustomLineFeedStreamReader.cs.
- Сеть/HTTP: Http/* (HttpRequestContext.cs, HttpResponseContext.cs, HttpRequestBody*, InternetProxyContext.cs).
- JSON: Json/* (JSONReader.cs, JSONWriter.cs, JSONDataExtractor.cs, JSONWriterSettings.cs).
- XML/XSLT: Xml/* (XmlReaderImpl.cs, XmlWriterImpl.cs, XSLTransform.cs).
- ZIP: Zip/* (ZipReader.cs, ZipWriter.cs и перечисления параметров).
- Процессы: Processes/* (ProcessContext.cs, GlobalProcessesFunctions.cs).
 - StandardGlobalContext.cs — набор полезных глобальных функций/свойств (например, Символы, Приостановить/Sleep, ЗначениеЗаполнено и т.п.).
- Разное: RandomNumberGenerator.cs, StringOperations.cs, Timezones/*.

### 3.7 OneScript.Web.Server — веб-обёртки для BSL (ASP.NET Core)
Назначение: адаптеры поверх ASP.NET Core API, чтобы работать с HTTP/WebSocket из BSL.
- WebServer.cs — базовая обвязка.
- Http*Wrapper.cs — HttpContext/Request/Response/Cookies.
- WebSockets/* — WebSocketWrapper, WebSocketsManagerWrapper.

### 3.8 Отладка: OneScript.DebugProtocol, OneScript.DebugServices, VSCode.DebugAdapter
- Протокол (OneScript.DebugProtocol):
  - TcpServer/* (DefaultMessageServer.cs, JsonDtoChannel.cs, DispatchingServer.cs) — транспорт и сериализация.
  - Модель отладки: Breakpoint.cs, StackFrame.cs, Variable.cs, DebuggerSettings.cs.
- Сервисы (OneScript.DebugServices):
  - DefaultDebugService.cs, DefaultDebugController.cs — серверные сервисы отладки.
  - ThreadManager.cs, TcpDebugServer.cs — управление потоками и TCP‑сервер.
- Адаптер для VS Code (VSCode.DebugAdapter):
  - DebugSession.cs, OscriptDebugSession.cs — основная сессия и проксирование событий.
  - ServerProcess.cs, DebugeeProcess.cs — управление процессом отлаживаемого приложения.

### 3.9 Приложения/утилиты
- oscript (CLI): src/oscript
  - Program.cs — входная точка.
  - ConsoleHostBuilder.cs — сборка консольного хоста.
  - Поведения (режимы): ExecuteScriptBehavior.cs, CheckSyntaxBehavior.cs, ShowVersionBehavior.cs, DebugBehavior.cs и др.
- StandaloneRunner: сборка самодостаточных пакетов/запуск
  - Program.cs, StandaloneRunner.cs, ProcessLoader.cs.
- OneScriptDocumenter: генерация документации по сборкам
  - На вход получает список DLL (в каталоге с ними рядом должны лежать файлы xml-docs от этих dll) и файл оглавления (в репозитории уже лежит готовый файл оглавления OneScriptDocumenter\default_toc.json)
  - На выходе формирует файл с документацией формата Json и/или каталог с документацией в формате markdown.
- Примеры/демо:
  - Component — простая .NET‑библиотека/компонент и примеры использования.
  - TestApp — WPF‑приложение‑песочница (подсветка синтаксиса, запуск модулей).

### 3.10 ScriptEngine.NativeApi — C++ API для нативных расширений
- Основные файлы: NativeApiProxy.cpp, NativeInterface.cpp, include/* (AddInDefBase.h, ComponentBase.h и др.).
- Задача: писать нативные аддины, видимые в BSL как объекты/контексты.

## 4 Как компоненты связаны между собой (словесная диаграмма)
- Language → даёт AST и ошибки компиляции.
- Core → базовые типы/контексты; используется Native, ScriptEngine, StandardLibrary.
- Native → компилирует и исполняет, опирается на Language/Core.
- ScriptEngine → организует выполнение (стек‑машина), использует Native и Core.
- HostedScript → высокий уровень хостинга поверх ScriptEngine.
- StandardLibrary → реализована на Core/ScriptEngine.
- DebugProtocol/DebugServices ↔ ScriptEngine — обмен данными отладки; VSCode.DebugAdapter ↔ DebugServices.
- Web.Server ↔ ASP.NET Core API — обёртки для работы из BSL.
- oscript/StandaloneRunner/Documenter/TestApp/Component — надстройки поверх ядра/рантайма.
- NativeApi ↔ ScriptEngine/Core — нативные расширения.

## 5 Типичные сценарии доработок (куда лезть)
- Добавить объект/контекст с методами: OneScript.Core/Contexts/* (аннотации Context*Attribute).
- Добавить функцию в стандартную библиотеку: соответствующий раздел OneScript.StandardLibrary (например, Json/ или Collections/), плюс экспорт в общий контекст (StandardGlobalContext.cs или SymbolsContext.cs, если нужно).
- Встроенная функция языка/операция: OneScript.Native/Runtime/BuiltInFunctions.cs и/или Compiler/*, при необходимости — поддержка в ScriptEngine/Machine.
- Отладка: DebugServices/DebugProtocol — добавление/изменение событий или представления переменных; VSCode.DebugAdapter — проксирование.

## 6 Навигация по тестам
- C#-тесты: src/Tests/*:
  - Язык: src/Tests/OneScript.Language.Tests/* (лексер/парсер/препроцессор).
  - Ядро/типы/контексты: src/Tests/OneScript.Core.Tests/*.
  - Динамика/нативный рантайм: src/Tests/OneScript.Dynamic.Tests/*.
  - Стандартная библиотека: src/Tests/OneScript.StandardLibrary.Tests/*.
  - Отладчик: src/Tests/VSCode.DebugAdapter.Tests/, src/Tests/OneScript.DebugProtocol.Test/.
- Скриптовые тесты: tests/*.os (поведенческие сценарии языка/библиотек).
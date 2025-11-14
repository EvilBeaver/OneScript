# Архитектурный обзор OneScript (для разработчиков)
Это «путеводитель по исходникам» OneScript. Документ объясняет, как устроены слои проекта, куда смотреть в коде для типовых задач, какие есть точки расширения и как компоненты взаимодействуют. По нему новый контрибьютор должен уметь:
- сориентироваться в решении src/1Script.sln,
- добавить тип/контекст/метод/свойство или глобальную функцию,
- собрать и запустить движок/тесты,
- понимать границы интеграций (веб, отладка, native API).

Псевдонимы API приняты двуязычные: РусИмя/EngName (см. атрибуты ContextClass/Method/Property).

Соглашение по ссылкам: указываются относительные пути репозитория.

1. Введение
- Миссия: OneScript — открытая реализация языка 1С/BSL поверх .NET, со стековой ВМ и стандартной библиотекой. Сценарии исполняются CLI oscript либо внедряются в приложения через HostedScript.
- Как читать документ: сверху вниз по слоям. В каждом разделе: назначение, файлы, ключевые классы/интерфейсы, жизненный цикл, точки расширения, типичные ошибки.

2. Слои и контейнеры (карта)
- Приложения/инструменты:
  - src/oscript — консольный рантайм;
  - src/StandaloneRunner — автономный упаковщик/раннер;
  - src/VSCode.DebugAdapter — адаптер DAP;
  - src/OneScriptDocumenter — генерация документации;
  - примеры: src/TestApp, src/Component.
- Хостинг/сервисы: src/ScriptEngine.HostedScript, src/OneScript.DebugServices, src/OneScript.Web.Server.
- Рантайм: src/ScriptEngine (стековая ВМ), src/OneScript.Native (нативный бэкенд).
- Ядро/язык/библиотеки: src/OneScript.Core (типы/контексты), src/OneScript.Language (лексер/парсер/AST), src/OneScript.StandardLibrary (стандартные контексты/функции).
- Интеграции: src/ScriptEngine.NativeApi (C++ Native API).

3. OneScript.Language — лексер/препроцессор/парсер/AST
- Назначение: разбирает исходники в AST и выдаёт ошибки.
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
- Типичные ошибки:
  - пропуск точки с запятой; некорректная расстановка блоков End*; смешение #native/#stack логики на уровне языка — решается на этапе компиляции, а не парсинга.

4. OneScript.Core — система типов и контекстная модель
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
- Устаревание:
  - Свойства: предупреждения на этапе компиляции стек-машины (StackMachineCodeGenerator) — логируется SystemLogger.
  - Методы: в рантайме AutoContext<T>.CheckIfCallIsPossible — может предупреждать либо кидать исключение (ThrowOnUse).
- Точки расширения:
  - новые Bsl-типы (производные от BslValue), регистрация в TypeFactory/ITypeManager;
  - новые контексты (атрибуты + AutoContext/GlobalContextBase).

5. ScriptEngine — компиляция и стековая ВМ
- Назначение: компиляция AST в байт-код и исполнение на стековой машине; окружение, глобальные объекты, отладка.
- Где в коде:
  - Compiler/* — CompilerFrontend, BackendSelector; StackMachineCodeGenerator (байткод), EvalCompiler; CodeGenerationFlags.
  - Machine/* — StackMachineExecutor, MachineInstance (командный цикл, стек/кадры/исключения/итераторы), ExecutionContext/Frame, BuiltinFunctions, ValueFactory, GlobalInstancesManager. CodeStat/* — код-статистика.
  - Hosting/* — DefaultEngineBuilder, DI (TinyIoC), EngineBuilderExtensions (регистрация сервисов, предобработчики).
  - ScriptingEngine.cs — фасад движка: загрузка сборок, Initialize, NewProcess, компиляция.
- Жизненный цикл выполнения:
  1) HostedScriptEngine/CLI создаёт IEngineBuilder → Build → ScriptingEngine.
  2) Initialize(): глобальные контексты, DI, опции, код-статистика.
  3) Компиляция модуля (CompilerFrontend → выбранный backend).
  4) MachineInstance выполняет байткод: командная таблица OperationCode, переходы/исключения (BeginTry/EndTry/Raise), вызовы методов/глобалей, итераторы, Execute/Eval.
  5) Отладка: MachineWait/ThreadManager, Breakpoints, LineNum, Evaluate.
- Точки расширения:
  - дополнительные IExecutorProvider (альтернативные рантаймы);
  - предопределённые интерфейсы/итераторы (Interfaces/Iterables handlers);
  - сбор кода-статистики (ICodeStatCollector).
- Типичные ошибки:
  - неверное количество аргументов/по значению vs по ссылке — проверяется на компиляции и в рантайме;
  - построение глобалей без регистрации в окружении → метод/тип «не найден».

6. OneScript.Native — нативный бэкенд (Expression Trees)
- Назначение: альтернативная компиляция в System.Linq.Expressions (экспериментальный режим; директива #native).
- Где в коде:
  - Compiler/* — ModuleCompiler/MethodCompiler, ExpressionTreeGeneratorBase, DynamicOperations (динамические операции), BuiltInFunctions (нативные), *Cache.
  - Runtime/* — NativeExecutorProvider (исполнение компилированных деревьев).
- Ограничения: может не поддерживать полный паритет со стековой машиной; выбор режима делает CompilerBackendSelector по AST/директивам.

7. ScriptEngine.HostedScript — хостинг, загрузка библиотек, конфигурация
- Назначение: безопасная обвязка движка для встраивания, глобальный системный контекст, загрузка библиотек, конфигурирование.
- Где в коде:
  - HostedScriptEngine.cs — инициализация, глобальные контексты (SystemGlobalContext, DynamicLoadingFunctions), создание процессов.
  - LibraryLoader.cs — package-loader.os, подключение .os модулей/классов/макетов; FileSystemDependencyResolver.cs — поиск библиотек, цикл обработки, защита от циклических зависимостей.
  - Extensions/EngineBuilderExtensions.cs — UseSystemConfigFile/UseEnvironmentVariableConfig/UseEntrypointConfigFile; UseImports/UseFileSystemLibraries/UseNativeRuntime/UseEventHandlers.
- Жизненный цикл:
  1) Конфигурации собираются из oscript.cfg, env, и файла рядом с entrypoint.
  2) Инициализация HostedScriptEngine → глобальные объекты → процесс → компиляция/исполнение модуля.
  3) Загрузка библиотек: default или кастомный package-loader.os, последующая регистрация символов и компиляция задержанных модулей.
- Точки расширения:
  - собственные загрузчики (кастомный package-loader.os), расширение FileSystemDependencyResolver.SearchDirectories;
  - инъекция глобальных свойств и объектов (IRuntimeEnvironment.Inject*).

8. OneScript.StandardLibrary — стандартная библиотека
- Назначение: коллекции, ФС/потоки, текст/кодировки, HTTP, JSON, XML/XSLT/XSD/XDTO, ZIP, процессы, TCP, регулярные выражения, фоновые задания, типы/квалификаторы, хеши, часовые пояса и др.
- Где в коде:
  - Collections/* (ArrayImpl, MapImpl, StructureImpl, ValueTable, ValueTree, ValueList и индексы);
  - FileContext, FileOperations, Text/*;
  - Http/* (HttpRequest/Response/Body);
  - Json/*, Xml/*, XSLTransform.cs;
  - Zip/*, Processes/*, Regex/*, Tasks/*, Timezones/*, TypeDescriptions/*, Hash/*;
  - StandardGlobalContext.cs — набор полезных глобальных функций/свойств (например, Символы, Приостановить/Sleep, ЗначениеЗаполнено и т.п.).
- Депрекейшены: двуязычные имена обязательны; устаревание — через атрибуты контекстов (см. Core) с соответствующим поведением компилятора/рантайма.

9. OneScript.Web.Server — веб-сервер (ASP.NET Core)
- Назначение: запуск Kestrel и работа с HTTP/WebSocket из BSL.
- Где в коде:
  - WebServer.cs — контекст ВебСервер: порт, middleware, статические файлы, исключения;
  - *Wrapper.cs — HttpContext/Request/Response, Cookies, WebSockets.
- Важное:
  - обработчик исключений по умолчанию vs пользовательский;
  - middleware через AddRequestsHandler(Target, MethodName): вызов метода BSL с IBslProcess.

10. Отладка: DebugProtocol, DebugServices, VSCode.DebugAdapter
- Контракты: src/OneScript.DebugProtocol — Breakpoint, StackFrame, Variable, ExceptionBreakpointFilter; TcpServer/* — RPC/каналы.
- Сервисы: src/OneScript.DebugServices — TcpDebugServer, DebugSession, ThreadManager, DefaultDebugger/BreakpointManager, IVariableVisualizer.
- Адаптер VSCode: src/VSCode.DebugAdapter — OscriptDebugSession, OneScriptDebuggerClient, Transport, ProtocolVersions, обработка команд IDE.
- Как это работает:
  - машина исполняет байткод, команда LineNum триггерит проверки брейкпоинтов/шагов, MachineStopManager синхронизирует останов;
  - Evaluate в контексте кадра — компиляция и запуск выражения в «вложенном» исполнении.

11. CLI/Инструменты
- oscript (src/oscript):
  - BehaviorSelector — разбор ключей: -check, -compile, -debug, -codestat, -encoding, -version/-v, -cgi, исполнение файла.
  - ExecuteScriptBehavior — сборка HostedScriptEngine, запуск процесса, вывод ошибок, CodeStatWriter (JSON).
- StandaloneRunner — упаковка модуля в бинарь и запуск без dotnet SDK (см. Program.cs, ProcessLoader.cs).
- OneScriptDocumenter — сбор и генерация документации по сборкам (Markdown/JSON/VitePress sidebar).

12. Native API (C++)
- Где в коде: src/ScriptEngine.NativeApi (C++). Контракт IComponentBase, прокси NativeApiProxy/NativeInterface.
- Что делает: загрузка .dll/.so нативных расширений, маршаллинг, события/ошибки.
- Риски: безопасность загрузки внешнего кода, отсутствие песочницы.

13. i18n/l10n, конфигурация/сборка/CI, Observability, Security
- i18n/l10n:
  - Locale.SystemLanguageISOName, НСтр (BilingualString), кодировки (TextEncodingEnum, RegisterProvider(CodePagesEncodingProvider)).
  - Двуязычие API — всегда задавайте пары имён (рус/англ) в атрибутах контекстов.
- Конфигурация/сборка:
  - сборка — Build.csproj таргеты; локально — msbuild или IDE; решение src/1Script.sln;
  - конфиг — oscript.cfg рядом с исполняемым и/или рядом со скриптом, и переменная окружения.
- Observability:
  - SystemLogger — предупреждения/заметки (в т.ч. депрекейшены и трассировка загрузчика LRE: OS_LRE_TRACE=1);
  - -codestat — сбор статистики по строкам/времени (CodeStatProcessor → JSON).
- Security:
  - загрузка внешних .NET/Native компонентов без песочницы — ответственность на окружении/операторе;
  - не понижайте уровень ошибок: на неверных данных — бросайте исключения, логируйте контекст.

14. Типовые сценарии доработок («куда лезть»)
- Новый тип/значение:
  - src/OneScript.Core/Values/* — наследуемся от BslValue; регистрируем фабрику в TypeFactory/ITypeManager; тесты в OneScript.Core.Tests.
- Новый контекст/метод/свойство:
  - создаём класс с [ContextClass] и AutoContext<T>/GlobalContextBase<T>; методы/свойства помечаем атрибутами; регистрируем сборку в окружении (env.AddAssembly(...)) — см. ScriptEngine.HostedScript/Extensions/EngineBuilderExtensions.cs и ContextDiscoverer.
- Встроенная функция языка/операция:
  - стековая машина: добавление OperationCode и реализации в MachineInstance; парсер/генератор кода — StackMachineCodeGenerator/LanguageDef (встроенные функции); нативная ветка — OneScript.Native/Runtime/BuiltInFunctions и Compiler.
- Загрузка библиотек:
  - package-loader.os (см. install/package-loader.os и примерные tests/superpackage/*), LibraryLoader.ProcessLibrary, FileSystemDependencyResolver.SearchDirectories.
- Отладчик:
  - точки остановки/шаги: DebugServices, Machine.StopManager; переменные — IVariableVisualizer; адаптер IDE — VSCode.DebugAdapter.

15. Навигация по тестам
- C#-тесты: src/Tests/*:
  - Язык: src/Tests/OneScript.Language.Tests/* (лексер/парсер/препроцессор).
  - Ядро/типы/контексты: src/Tests/OneScript.Core.Tests/*.
  - Динамика/нативный рантайм: src/Tests/OneScript.Dynamic.Tests/*.
  - Стандартная библиотека: src/Tests/OneScript.StandardLibrary.Tests/*.
  - Отладчик: src/Tests/VSCode.DebugAdapter.Tests/*, src/Tests/OneScript.DebugProtocol.Test/*.
- Скриптовые тесты: tests/*.os (поведенческие сценарии языка/библиотек).

Приложение A. Мини‑гайд по сборке и запуску
- Сборка: откройте src/1Script.sln, соберите все проекты (или msbuild Build.csproj /t:PrepareDistributionFiles).
- CLI: src/oscript — запуск .os; ключи: -check/-compile/-debug/-codestat/-encoding/-cgi.
- Отладка: поднимите DebugServices, запустите VSCode.DebugAdapter и подключитесь из VS Code.

Приложение B. Частые ошибки и рекомендации
- Несоответствие рус/англ имён — строго указывайте пары в атрибутах.
- Нарушение byRef/byVal — учитывайте ExplicitByVal и Variable/ValueReference.
- Устаревание API — не молчите: используйте IsDeprecated/ThrowOnUse, обеспечивайте предупреждения/исключения.
- Безопасность загрузок — не подключайте непроверенные внешние .NET/Native компоненты в production окружении.
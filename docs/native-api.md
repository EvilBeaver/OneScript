# Native API — сборка, тесты и отладка

Документ для агентов и разработчиков, которые правят C++-прослойку или тестовую
компоненту Native API и должны быстро получить **актуальные бинарники** и
**воспроизвести** проблему на Windows или Linux.

**Агентам на Windows:** перед любой сборкой C++ прочитать §3.0 — C++ только через
Visual Studio MSBuild, не через `dotnet build` / `dotnet msbuild`.

См. также:

- [`.cursor/rules/runbsltests.mdc`](../.cursor/rules/runbsltests.mdc) — запуск BSL-тестов;
- [`tests/native-api.os`](../tests/native-api.os) — приёмочные тесты Native API;

## 1. Архитектура (три слоя)

| Слой | Каталог / файлы | Назначение |
|------|-----------------|------------|
| Управляемая прослойка | `src/OneScript.StandardLibrary/NativeApi/` | `NativeApiComponent`, `NativeApiProxy.cs`, фабрика типов, out-параметры |
| Нативный прокси (C++) | `src/ScriptEngine.NativeApi/` | `NativeApiProxy.cpp` — мост между .NET и `IComponentBase` |
| Тестовая компонента (C++) | `tests/native-api/` | `AddInNative.cpp` — эталонная внешняя компонента для `tests/native-api.os` |

При вызове метода компоненты цепочка такая:

```
BSL → NativeApiComponent (C#) → NativeApiProxy.cs (P/Invoke)
    → ScriptEngine.NativeApi64.dll (C++) → AddInNativeWin64.dll (C++)
```

**Важно:** `dotnet build src/oscript/oscript.csproj` пересобирает только C#.
C++-DLL **не обновляются автоматически**. Для C++ на Windows — §3.0 (Visual
Studio MSBuild, не SDK). Устаревший прокси в каталоге oscript может маскировать
или, наоборот, порождать баги.

## 2. Где лежат бинарники

### Прокси `ScriptEngine.NativeApi`

| Платформа | Артефакт после сборки | Имя рядом с oscript |
|-----------|----------------------|---------------------|
| Windows x64 | `src/ScriptEngine.NativeApi/bin/Release/x64/ScriptEngine.NativeApi64.dll` | `ScriptEngine.NativeApi64.dll` |
| Windows x86 | `src/ScriptEngine.NativeApi/bin/Release/x86/ScriptEngine.NativeApi32.dll` | `ScriptEngine.NativeApi32.dll` |
| Linux x64 | `src/ScriptEngine.NativeApi/bin/Release/x64/ScriptEngine.NativeApi64.so` | `ScriptEngine.NativeApi64.so` |
| Linux x86 | `src/ScriptEngine.NativeApi/bin/Release/x86/ScriptEngine.NativeApi32.so` | `ScriptEngine.NativeApi32.so` |

Прокси загружается из **каталога сборки oscript** (рядом с `oscript.exe`), см.
`NativeApiProxy.cs` static ctor.

### Тестовая компонента `AddInNative`

| Платформа | Артефакт | Откуда грузит `tests/native-api.os` |
|-----------|----------|--------------------------------------|
| Windows x64 | `tests/native-api/bin64/AddInNativeWin64.dll` | `native-api/bin64/AddInNativeWin64.dll` |
| Windows x86 | `tests/native-api/bin/AddInNativeWin32.dll` | `native-api/bin/AddInNativeWin32.dll` |
| Linux x64 | `tests/native-api/build64/AddInNativeLin64.so` | `native-api/build64/AddInNativeLin64.so` |
| Linux x86 | `tests/native-api/build32/AddInNativeLin32.so` | `native-api/build32/AddInNativeLin32.so` |

Release-сборка vcxproj кладёт x64 DLL сразу в `bin64/`. Debug — в `bind64/`,
оттуда нужно **скопировать** в `bin64/`, если тесты читают Release-путь.

Toolset обоих vcxproj: **v142** (VS 2019). На машине достаточно Build Tools 2022.

## 3. Сборка на Windows

### 3.0. Инструкция для агентов (обязательно)

C++-проекты (`*.vcxproj`) **нельзя** собирать через .NET SDK. В SDK есть
`dotnet build` и `dotnet msbuild`, но **в них нет компилятора C++** — команды
либо упадут, либо «соберут» что-то без обновления DLL.

**Запрещено для C++:**

- `dotnet build` (любой `.vcxproj` или `Build.csproj /t:MakeCPP`);
- `dotnet msbuild` (в том числе `dotnet msbuild Build.csproj /t:MakeCPP`);
- `msbuild` из PATH, если это MSBuild из каталога .NET SDK
  (`...\dotnet\sdk\<версия>\MSBuild.dll`).

**Единственный допустимый способ** — MSBuild из Visual Studio / Build Tools с
установленной нагрузкой **«Разработка классических приложений на C++»**:

```powershell
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe"
```

Перед сборкой проверить наличие файла:

```powershell
if (-not (Test-Path $msbuild)) {
    # СТОП. Не искать обходные пути, не пробовать dotnet msbuild, не ставить пакеты.
    # Сообщить пользователю, что не найден MSBuild с компилятором C++,
    # и спросить, где установлены Build Tools / Visual Studio или как их установить.
}
```

Если `$msbuild` не найден — **остановиться** и спросить у пользователя. Не
пытаться собрать C++ другими средствами и не продолжать расследование с
устаревшими DLL.

`dotnet build src\oscript\oscript.csproj` допустим **только для C#** (см. §3.3).
После изменений в C++ его одного недостаточно.

### 3.1. Полная сборка C++ (прокси + тестовая компонента)

Из корня репозитория, через Visual Studio MSBuild:

```powershell
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe"

& $msbuild Build.csproj /t:MakeCPP /p:Configuration=Release
```

Target `MakeCPP` в `Build.csproj` вызывает оба vcxproj для x86 и x64. Запускать
его нужно **тем же** `$msbuild`, не через `dotnet msbuild`.

### 3.2. Точечная сборка одной платформы

```powershell
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe"

& $msbuild src\ScriptEngine.NativeApi\ScriptEngine.NativeApi.vcxproj /p:Configuration=Release /p:Platform=x64 /m
& $msbuild tests\native-api\AddInNative.vcxproj /p:Configuration=Release /p:Platform=x64 /m
```

Для x86 заменить `Platform=x64` на `Platform=Win32`.

### 3.3. Сборка C# и копирование прокси к oscript

```powershell
dotnet build src\oscript\oscript.csproj

Copy-Item src\ScriptEngine.NativeApi\bin\Release\x64\ScriptEngine.NativeApi64.dll `
  src\oscript\bin\Debug\net8.0\ -Force
```

Проверить дату файла — она должна совпадать со временем MSBuild, а не быть
«застрявшей» на старой дате (типичная ловушка при расследовании).

### 3.4. Запуск тестов

**Только свежесобранный oscript**, не версия из `ovm` / PATH:

```powershell
cd tests
..\src\oscript\bin\Debug\net8.0\oscript.exe testrunner.os -run native-api.os
```

Полный прогон:

```powershell
tests\run-bsl-tests.cmd src\oscript\bin\Debug\net8.0\oscript.exe
```

## 4. Сборка на Linux

```bash
cd src/ScriptEngine.NativeApi
./build.sh    # или cmake по README в каталоге

cd ../../tests/native-api
./build.sh    # AddInNativeLin32.so / AddInNativeLin64.so
```

Затем `dotnet build src/oscript/oscript.csproj` и скопировать `.so` в
`src/oscript/bin/Debug/net8.0/`.

## 5. Типичные ловушки при воспроизведении

### 5.1. Старый прокси маскирует баг

Симптом: тесты проходят локально у одного разработчика и падают у другого;
на Linux всё зелёное, на Windows — нет.

Действия:

1. Посмотреть `LastWriteTime` у `src\oscript\bin\Debug\net8.0\ScriptEngine.NativeApi64.dll`.
2. Пересобрать vcxproj и **явно скопировать** DLL.
3. Повторить тест.

Пример из практики: прокси от 16.09 скрывал баг маршалинга `bool`; после
пересборки 18.09 тест `ТестДолжен_ПроверитьОбменПараметров` стабильно падал.

### 5.2. Запуск через oscript из ovm

`C:\Users\<user>\AppData\Local\ovm\current\bin\oscript.exe` — **старый** движок
и часто **старый** прокси. Для проверки изменений в репозитории всегда указывать
путь к `src\oscript\bin\Debug\net8.0\oscript.exe`.

### 5.3. `dotnet build` / `dotnet msbuild` вместо Visual Studio MSBuild

Out-параметры, `IsPropReadable`/`IsPropWritable`, `VTYPE_TM` и т.д. зависят от
согласованности C++ и C#. Изменения только в `.cs` без пересборки прокси
(или наоборот) дают ложные результаты.

Типичная ошибка агента: `dotnet msbuild Build.csproj /t:MakeCPP` — MSBuild из
.NET SDK **не содержит** toolset C++. Нужен MSBuild из Build Tools (§3.0).

### 5.4. Release vs Debug артефакты компоненты

| Конфигурация | x64 DLL |
|--------------|---------|
| Release | `tests/native-api/bin64/AddInNativeWin64.dll` |
| Debug | `tests/native-api/bind64/AddInNativeWin64.dll` → скопировать в `bin64/` |

## 6. Алгоритм расследования бага Native API

1. **Воспроизвести** `tests/native-api.os` на свежем oscript + свежем прокси.
2. Если падает один сценарий — **минимальный `.os`-скрипт** в `tests/` (удалить
   после отладки), подключить `bin64/AddInNativeWin64.dll` напрямую.
3. Локализовать слой:
   - свойства/методы не вызываются → C++ компонента или прокси;
   - out-параметры / исключения при записи → `NativeApiComponent.RemapOutputParameters`
     + `PropertyValueReference` (C#);
   - неверные флаги readable/writable / успех вызова → P/Invoke в `NativeApiProxy.cs`.
4. При изменении **сигнатуры экспорта** в `NativeApiProxy.cpp` синхронно менять
   делегат в `NativeApiProxy.cs` (ABI, порядок аргументов, размер возвращаемого
   значения).
5. После фикса: `native-api.os` (16 тестов), затем полный `run-bsl-tests.cmd`.

## 7. Ключевые файлы по темам

| Тема | Файлы |
|------|-------|
| P/Invoke, делегаты | `src/OneScript.StandardLibrary/NativeApi/NativeApiProxy.cs` |
| Out-параметры | `src/OneScript.StandardLibrary/NativeApi/NativeApiComponent.cs` (`RemapOutputParameters`) |
| tVariant ↔ IValue | `src/OneScript.StandardLibrary/NativeApi/NativeApiVariant.cs` |
| Экспорты прокси | `src/ScriptEngine.NativeApi/NativeApiProxy.cpp` |
| Типы Native API | `src/ScriptEngine.NativeApi/include/types.h` |
| Тестовая компонента | `tests/native-api/AddInNative.cpp` |
| BSL-тесты | `tests/native-api.os` |

## 8. Чеклист перед сдачей изменений Native API

- [ ] C++ собран через Visual Studio MSBuild (§3.0), **не** через `dotnet msbuild`.
- [ ] Пересобран `ScriptEngine.NativeApi.vcxproj` (нужные Platform/Configuration).
- [ ] При правках компоненты — пересобран `tests/native-api/AddInNative.vcxproj`.
- [ ] Свежий прокси скопирован в `src/oscript/bin/Debug/net8.0/`.
- [ ] `dotnet build src/oscript/oscript.csproj`.
- [ ] `oscript.exe testrunner.os -run native-api.os` — 16/16.
- [ ] Даты DLL прокси и oscript.dll согласованы со временем сборки.

## 9. Известные исключения при полном прогоне

См. `.cursor/rules/runbsltests.mdc`: игнорировать падения `http.os`, отдельного
HTTP-теста перемещения файла, а также ошибки из-за отсутствующей `Component.dll`
(если не в scope задачи).

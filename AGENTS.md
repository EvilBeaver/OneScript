# Инструкции для агентов — OneScript

Точка входа для AI-агентов и автоматизации: где искать правила сборки, тестов и
типовых доработок в этом репозитории.

## Быстрый выбор документа

| Задача | Документ |
|--------|----------|
| Общая карта проекта, «куда лезть» | [`docs/developer_docs.md`](docs/developer_docs.md) |
| Новый BSL-контекст, метод, свойство | [`docs/contexts.md`](docs/contexts.md) |
| Native API, C++-прокси, внешние компоненты | [`docs/native-api.md`](docs/native-api.md) |
| Установка, полная сборка, BSL-тесты (общее) | [`README.md`](README.md) |
| Стиль C# | [`CODESTYLE.md`](CODESTYLE.md) |
| Запуск BSL-тестов (детали, исключения) | [`.cursor/rules/runbsltests.mdc`](.cursor/rules/runbsltests.mdc) |
| Версия C# по проектам | [`.cursor/rules/langversion.mdc`](.cursor/rules/langversion.mdc) |
| Полная CI-сборка на Linux (без C++) | [`.github/copilot-instructions.md`](.github/copilot-instructions.md) |

## Каталог `docs/`

- **[`developer_docs.md`](docs/developer_docs.md)** — архитектура, слои, обзор
  проектов в `src/`, связи компонентов, навигация по C#- и BSL-тестам.
- **[`contexts.md`](docs/contexts.md)** — пошаговое добавление контекстов,
  атрибуты `ContextClass`/`ContextMethod`, регистрация в `package-loader.os`.
- **[`native-api.md`](docs/native-api.md)** — три слоя Native API, сборка
  `*.vcxproj` на Windows **только** через Visual Studio MSBuild (не
  `dotnet build` / `dotnet msbuild`), копирование DLL, `tests/native-api.os`.

## Правила Cursor (`.cursor/rules/`)

- **`runbsltests.mdc`** — как запускать приёмочные тесты, какой `oscript.exe`
  использовать, известные падения вне scope.
- **`langversion.mdc`** — `VSCode.DebugAdapter` ограничен C# 7.3; остальные
  проекты — .NET 8 / C# 12.

## Важные напоминания

1. **C++ на Windows** — см. §3.0 в [`docs/native-api.md`](docs/native-api.md).
   Если MSBuild с компилятором C++ не найден — остановиться и спросить у
   пользователя, не пробовать SDK.
2. **BSL-тесты** — свежесобранный `src/oscript/bin/Debug/net8.0/oscript.exe`,
   не версия из `ovm` / PATH (подробности в `native-api.md` и `runbsltests.mdc`).
3. **Коммиты** — только по явной просьбе пользователя; не коммитить бинарники
   без запроса (см. также `copilot-instructions.md`).

## Связанные каталоги в репозитории

- `tests/*.os` — приёмочные BSL-сценарии; `tests/native-api.os` — Native API.
- `tests/native-api/` — эталонная C++-компонента для тестов.
- `src/ScriptEngine.NativeApi/` — C++-прокси между .NET и `IComponentBase`.
- `src/OneScript.StandardLibrary/NativeApi/` — управляемая прослойка (C#).

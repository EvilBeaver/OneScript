Команда запуска полной сборки и прогона тестов на Linux:

```sh
dotnet msbuild Build.csproj /t:"CleanAll;MakeFDD;GatherLibrary;ComposeDistributionFolders;Test" /p:Configuration=LinuxDebug /p:NoCppCompiler=True
```

Запуск приемочных тестов:

```sh
dotnet oscript.dll tests/testrunner.os -runAll tests
```

ВСЕГДА проверяй изменения перед коммитом. Не коммить бинарные файлы (exe, ospx, и другие) если тебя об этом явно не попросили.

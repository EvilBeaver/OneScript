Команда запуска полной сборки и прогона тестов на Linux:

```sh
msbuild Build.csproj /t:CleanAll;MakeFDD;GatherLibrary;ComposeDistributionFolders;Test /p:Configuration=LinuxDebug /p:NoCppCompiler=True
```

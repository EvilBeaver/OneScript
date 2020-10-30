@echo off

dotnet clean src\1Script.sln

echo "{  ""sdk"": {    ""version"": ""5.0.100-rc.2.20479.15""  } }" > global.json

rmdir distrs /s /q

dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -f net5.0 -p:DebugType=None -r win-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\win-x64\bin
dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -f net5.0 -p:DebugType=None -r win-x86  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\win-x86\bin
dotnet publish -p:PublishTrimmed=true -f net5.0 -p:DebugType=None -r linux-x64  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\linux-X64\bin
dotnet publish -p:PublishTrimmed=true -f net5.0 -p:DebugType=None -r osx-x64  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\osx-x64\bin

rm global.json

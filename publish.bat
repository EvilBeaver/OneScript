@echo off

dotnet clean src\1Script.sln

rmdir distrs /s /q

echo "{  ""sdk"": {    ""version"": ""3.1.403""  } }" > global.json

dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -f netcoreapp3.1 -p:DebugType=None -r win-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\win-x64\bin
dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -f netcoreapp3.1 -p:DebugType=None -r win-x86  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\win-x86\bin
dotnet publish -p:PublishTrimmed=true -f netcoreapp3.1 -p:DebugType=None -r linux-x64  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\linux-X64\bin
dotnet publish -p:PublishTrimmed=true -f netcoreapp3.1 -p:DebugType=None -r osx-x64  -c Release --self-contained true --force -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\osx-x64\bin

rm global.json

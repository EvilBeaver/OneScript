@echo off

echo {  "sdk": {    "version": "5.0.100-rc.2.20479.15"  } } > global.json

dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -f net5.0 -p:DebugType=None -r win-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\win-x64\bin
dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -f net5.0 -p:DebugType=None -r win-x86  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\win-x86\bin
dotnet publish -p:PublishTrimmed=true -f net5.0 -r linux-x64  -p:DebugType=None -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\linux-x64\bin
dotnet publish -p:PublishTrimmed=true -f net5.0 -r osx-x64  -p:DebugType=None -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net50\osx-x64\bin

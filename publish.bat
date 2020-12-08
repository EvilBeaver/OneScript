@echo off

echo "Local FULL publish for each platform, with testing script-engine (not xunit) by 1testrunner and archive artifacts"

call dotnet clean src\1Script.sln

call rmdir distrs /s /q

echo {  "sdk": {    "version": "3.1.403"  } } > global.json

call dotnet publish -p:PublishTrimmed=false -p:PublishReadyToRun=false -p:PublishReadyToRunShowWarnings=true -f netcoreapp3.1 -p:DebugType=None -r win-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\win-x64\bin

cd tests
echo "Old tests run"
..\distrs\net31\win-x64\bin\oscript.exe testrunner.os -runall

cd ..\distrs\net31\win-x64\bin

set OSLIB_LOADER_TRACE=1

echo "Testing run opm - install libs from hub"
opm install asserts
opm install tempfiles
opm install delegate
opm install fs
opm install 1testrunner
opm install 1bdd

call 1testrunner -runall ..\..\..\..\tests

cd ..\..\..\..\

dotnet publish -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -f netcoreapp3.1 -p:DebugType=None -r win-x86  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\win-x86\bin
dotnet publish -p:PublishTrimmed=true -f netcoreapp3.1 -p:DebugType=None -r linux-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\linux-x64\bin
dotnet publish -p:PublishTrimmed=true -f netcoreapp3.1 -p:DebugType=None -r osx-x64  -c Release --self-contained true --force -p:IncludeNativeLibrariesInSingleFile=true -p:CopyOutputSymbolsToPublishDirectory=false .\src\oscript\oscript.csproj -o distrs\net31\osx-x64\bin

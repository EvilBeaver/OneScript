@echo off

:build
echo Build started. This CMD file will be deprecated. Use MSBUILD.exe BuildAll.csproj instead

call setlocals.cmd

pushd %SolutionPath%
%git% pull origin master
popd

if NOT ERRORLEVEL == 0 GOTO bad_exit

call "%vsvars%vsvars32.bat"
devenv "%SolutionFilename%" /Clean
devenv "%SolutionFilename%" /build Release

if NOT ERRORLEVEL == 0 GOTO bad_exit

if exist build (
erase /Q /S build\*
rmdir /Q /S build)
md build

copy "%SolutionPath%\TestApp\bin\x86\Release\ScriptEngine.dll" build\ScriptEngine.dll
copy "%SolutionPath%\TestApp\bin\x86\Release\ScriptEngine.HostedScript.dll" build\ScriptEngine.HostedScript.dll
copy "%SolutionPath%\Release\ScriptEngine.Snegopat.dll" build\ScriptEngine.Snegopat.dll
copy "%SolutionPath%\TestApp\bin\x86\Release\TestApp.exe" build\TestApp.exe
copy "%SolutionPath%\TestApp\bin\x86\Release\ICSharpCode.AvalonEdit.dll" build\ICSharpCode.AvalonEdit.dll
copy "%SolutionPath%\oscript\bin\x86\Release\oscript.exe" build\oscript.exe

"%installer%" install.iss /o./dist
if NOT ERRORLEVEL == 0 GOTO bad_exit
echo Done

exit /B 0

:bad_exit
echo Fail

exit /B 1
@echo off

:build
echo Build started

call setlocals.cmd

pushd %SolutionPath%
%git% pull origin master
popd

if NOT ERRORLEVEL == 0 GOTO bad_exit

call "%vsvars%vsvars32.bat"
devenv "%SolutionFilename%" /Clean
devenv "%SolutionFilename%" /build Release

if NOT ERRORLEVEL == 0 GOTO bad_exit

if exist built (
erase /Q /S built\*
rmdir /Q /S built)
md built

copy "%SolutionPath%\TestApp\bin\x86\Release\ScriptEngine.dll" built\ScriptEngine.dll
copy "%SolutionPath%\TestApp\bin\x86\Release\ScriptEngine.HostedScript.dll" built\ScriptEngine.HostedScript.dll
copy "%SolutionPath%\TestApp\bin\x86\Release\TestApp.exe" built\TestApp.exe
copy "%SolutionPath%\TestApp\bin\x86\Release\ICSharpCode.AvalonEdit.dll" built\ICSharpCode.AvalonEdit.dll
copy "%SolutionPath%\oscript\bin\x86\Release\oscript.exe" built\oscript.exe

"%installer%" install.iss
if NOT ERRORLEVEL == 0 GOTO bad_exit
echo Done

exit /B 0

:bad_exit
echo Fail

exit /B 1
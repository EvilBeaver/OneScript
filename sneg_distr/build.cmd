@echo off

:build
echo Build started

call setlocals.cmd

pushd %SolutionPath%
%git% pull origin snegopat
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

copy "%SolutionPath%\Release\ScriptEngine.dll" built\ScriptEngine.dll
copy "%SolutionPath%\Release\ScriptEngine.Snegopat.dll" built\ScriptEngine.Snegopat.dll

echo Done

exit /B 0

:bad_exit
echo Fail

exit /B 1
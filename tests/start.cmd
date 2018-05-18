@echo off

setlocal
set pathdir=%~dp0

rem echo сами тесты %CD%
rem echo скрипты тестирования %pathdir%

set progfiles=%ProgramFiles(x86)%
if NOT EXIST ProgramFiles(x86) set progfiles=%ProgramFiles%

echo on
"%progfiles%\OneScript\bin\oscript.exe" "%pathdir%testrunner.os" -run %1 %2 %3 %4 %5
@echo off

rem echo %ERRORLEVEL%
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit
rem echo Успешно
rem pause

exit /B 0

:bad_exit
if %ERRORLEVEL%==-1 GOTO success_exit
echo Tests failed

pause
exit /B 1
@echo off

setlocal
set pathdir=%~dp0

rem echo сами тесты %CD%
rem echo скрипты тестирования %pathdir%

for /f "tokens=*" %%i in ('where oscript') do set OSCRIPT=%%i
if NOT "%OSCRIPT%"=="" GOTO run

set OS_EXE=\OneScript\bin\oscript.exe
set OSCRIPT=%ProgramFiles(x86)%%OS_EXE%
if NOT EXIST "%OSCRIPT%" set OSCRIPT=%ProgramFiles%%OS_EXE%
if NOT EXIST "%OSCRIPT%" set OSCRIPT=%ProgramW6432%%OS_EXE%

:run
echo on
"%OSCRIPT%" "%pathdir%testrunner.os" -run %1 %2 %3 %4 %5
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
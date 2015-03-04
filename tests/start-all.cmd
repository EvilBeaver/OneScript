@echo off

chcp 866 > nul

setlocal
set pathdir=%~dp0

rem echo сами тесты %CD%
rem echo скрипты тестирования %pathdir%

@echo on
"%ProgramFiles(x86)%\OneScript\oscript.exe" "%pathdir%\testrunner.os" -runall %1 %2 %3 %4 %5
@echo off

rem echo Код возврата %ERRORLEVEL%
if %ERRORLEVEL%==2 GOTO pending_exit
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit

exit /B 0

:pending_exit

exit /B 2

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo Несколько тестов упали
echo Неудача.  Красная полоса
echo    Упавщие тесты:
type %logfile%

if ".%1"=="." pause
exit /B 1
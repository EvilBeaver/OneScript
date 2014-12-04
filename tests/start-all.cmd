@echo off

rem chcp 866 > nul
chcp 65001

setlocal
set pathdir=%~dp0
set success=1

set logfile=failtests.log
del /Q /F %logfile%
echo . > %logfile%

for %%I in (*.os) do (
if NOT "%%~nI"=="start" (
if NOT "%%~nI"=="testrunner" (
	echo ---
	echo - файл теста -   %%I
	echo ---

	rem call %1 start.os -run %%I %2 %3 %4 %5

	rem @call "%pathdir%\..\install\built\oscript.exe" "%pathdir%\start.os" -run %%I %1 %2 %3 %4 %5
	@call "%ProgramFiles(x86)%\OneScript\oscript.exe" "%pathdir%\start.os" -run %%I %1 %2 %3 %4 %5
	
	
	if NOT %ERRORLEVEL%==0 (
		set success=%ERRORLEVEL%
		echo        Упал тест "%%~nI" >> %logfile%
	)
)
)
)

if NOT "%success%"=="0" GOTO bad_exit


:success_exit

exit /B 0

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo Несколько тестов упали
echo Неудача.  Красная полоса
echo    Упавщие тесты:
type %logfile%

if ".%1"=="." pause
exit /B 0
@echo off

pushd %~dp0
"..\install\built\oscript.exe" %CD%\start.os %1 %2 %3 %4 %5
popd

rem echo %ERRORLEVEL%
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit
rem echo �ᯥ譮
rem pause

exit /B 0

:bad_exit
if %ERRORLEVEL%==-1 GOTO success_exit
echo Tests failed

pause
exit /B 1
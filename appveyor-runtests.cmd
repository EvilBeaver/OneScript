@echo off
chcp 65001

cd tests
rem ..\src\oscript\bin\x86\Release\net452\oscript.exe testrunner.os -runall xdto
..\src\oscript\bin\x86\Release\net452\oscript.exe testrunner.os -runall xmlschema
rem ..\src\oscript\bin\x86\Release\net452\oscript.exe testrunner.os -runall global-funcs.os 



if %ERRORLEVEL%==2 GOTO good_exit
if %ERRORLEVEL%==0 GOTO good_exit

exit /B 1

:good_exit
exit /B 0

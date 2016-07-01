chcp 65001
echo lib.system=%APPVEYOR_BUILD_FOLDER%\oscript-library\src > src\oscript\bin\x86\Release\oscript.cfg
cd tests
..\src\oscript\bin\x86\Release\oscript.exe testrunner.os -runall .

if %ERRORLEVEL%==2 GOTO good_exit
if %ERRORLEVEL%==0 GOTO good_exit

exit /B 1

:good_exit
exit /B 0

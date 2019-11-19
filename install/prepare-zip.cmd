@echo off

set SOURCE=%~dpfn1
set ARCH=%2

if [%ARCH%] == [] (
    echo No arch specified
    exit /B 1
)

echo Building %ARCH% zip from %SOURCE%

set DEST=%SOURCE%\zip%ARCH%
if exist %DEST% erase /S /Q %DEST%\*.*
mkdir %DEST%

xcopy %SOURCE%\lib %DEST%\lib /S /E /I
xcopy %SOURCE%\examples %DEST%\examples /S /E /I
xcopy %SOURCE%\doc %DEST%\doc /S /E /I

mkdir %DEST%\bin

if [%ARCH%]==[x86] xcopy %SOURCE%\bin32\*.* %DEST%\bin /S /E /I
if [%ARCH%]==[x64] xcopy %SOURCE%\bin\*.* %DEST%\bin /S /E /I

exit /B

:NORMALIZE_PATH
SET RETVAL=%~dpfn1
  EXIT /B
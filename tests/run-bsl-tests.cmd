@echo off
setlocal

rem Remove trailing backslash from %~dp0
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
set OVM_LIB_PATH=%USERPROFILE%\AppData\Local\ovm\current\lib

set OSCRIPT_CONFIG=lib.system=%OVM_LIB_PATH%

set OSCRIPT_BIN=%~f1
if defined OSCRIPT_BIN goto validate_bin

if defined OSCRIPT_EXE (
    set OSCRIPT_BIN=%OSCRIPT_EXE%
    goto validate_bin
)

for /f "usebackq delims=" %%i in (`where oscript.exe 2^>nul`) do (
    set "OSCRIPT_BIN=%%i"
    goto validate_bin
)

echo [ERROR] Failed to determine path to oscript.exe. >&2
echo Pass the path as the first parameter or set OSCRIPT_EXE variable. >&2
exit /b 1

:validate_bin
if not exist "%OSCRIPT_BIN%" (
    echo [ERROR] File "%OSCRIPT_BIN%" not found. >&2
    exit /b 1
)

pushd "%SCRIPT_DIR%" >nul
"%OSCRIPT_BIN%" testrunner.os -runAll .
set EXIT_CODE=%ERRORLEVEL%
popd >nul

exit /b %EXIT_CODE%


@echo off

setlocal

set lib=%~dp0..\lib
set opm=%lib%\opm\src\cmd\opm.os

"%~dp0oscript.exe" "%opm%" %*

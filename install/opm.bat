@echo off

setlocal

set lib="%~dp0..\lib"
set opm=%lib%\opm\src\opm.os

oscript.exe %opm% %*
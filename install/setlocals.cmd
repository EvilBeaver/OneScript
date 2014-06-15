@echo off

rem переменные, зависящие от машины
set git="C:\Program Files (x86)\Git\bin\git.exe"
set vsvars=C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\
pushd %~dp0\..\src
set SolutionPath=%CD%
popd
set SolutionFilename=%SolutionPath%\1Script.sln

set installer=C:\Program Files (x86)\Inno Setup 5\iscc.exe

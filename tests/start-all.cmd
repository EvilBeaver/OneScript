@echo off

chcp 866 > nul

setlocal
set pathdir=%~dp0

rem echo ᠬ� ���� %CD%
rem echo �ਯ�� ���஢���� %pathdir%

set progfiles=%ProgramFiles(x86)%
if NOT EXIST ProgramFiles(x86) set progfiles=%ProgramFiles%

@echo on
"%progfiles%\OneScript\bin\oscript.exe" "%pathdir%testrunner.os" -runall %1 %2 %3 %4 %5
@echo off

rem echo ��� ������ %ERRORLEVEL%
if %ERRORLEVEL%==2 GOTO pending_exit
if NOT %ERRORLEVEL%==0 GOTO bad_exit

:success_exit

exit /B 0

:pending_exit

exit /B 2

:bad_exit
if "-1"=="%success%" GOTO success_exit
echo .
echo ��᪮�쪮 ��⮢ 㯠��
echo ��㤠�.  ��᭠� �����
echo    ����騥 ����:
type %logfile%

if ".%1"=="." pause
exit /B 1
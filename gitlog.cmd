@echo off

if [%1]==[] (
    echo No tagname specified
    exit /B 1
)

set LC_ALL=C.UTF-8
set TAGNAME=%1

git --no-pager log --pretty="* %%s" --no-merges %TAGNAME%..HEAD
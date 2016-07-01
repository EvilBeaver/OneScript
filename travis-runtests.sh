#!/bin/bash

pushd .
cd $1
oscript=$(pwd -P)/oscript.exe
popd

cd tests
mono $oscript testrunner.os -runall .

RESULT=$?

# Все тесты прошли
if [ $RESULT = 0 ]; then
	exit 0
fi

# Нереализованные тесты
if [ $RESULT = 2 ]; then
	exit 0
fi

exit 1

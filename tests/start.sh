#!/bin/bash
../src/oscript/bin/Debug/oscript.exe testrunner.os -run $@ 

if [ $? = 0 ]; then
	# success
	exit
fi

if [ $? = -1 ]; then
	# strange success
	exit
fi

echo Tests failed

exit 1


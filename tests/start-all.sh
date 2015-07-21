#!/bin/bash
../src/oscript/bin/Debug/oscript.exe testrunner.os -runall $@

if [ $? = 0 ]; then
	# success
	exit
fi

if [ $? = -1 ]; then
	# strange success
	exit
fi

exit 1


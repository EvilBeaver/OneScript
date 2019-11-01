#!/bin/bash

echo 'Preparing environment'

DISTPATH=$(pwd)/../built/tmp
BINPATH=${DISTPATH}/bin
cd `dirname $0`

VERSIONFILE=$DISTPATH/VERSION
if [ -f "$VERSIONFILE" ] ; then
	rm $VERSIONFILE
fi

mono ${BINPATH}/oscript.exe -version | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+' \
		> ${VERSIONFILE}

if [ ! -f "$VERSIONFILE" ] ; then
	echo "No version file created"
	exit 1
fi

OUTPUT="../output"
if [ -d "$OUTPUT" ]; then
	rm -rf ${OUTPUT}
fi
mkdir ${OUTPUT}

#!/bin/bash

DISTPATH=$(cd $1; pwd)
BINPATH=$DISTPATH/bin
cd `dirname $0`

echo "Assets folder: $DISTPATH"
echo "Current dir: {$PWD}"

VERSIONFILE=$BINPATH/VERSION
if [ -f "$VERSIONFILE" ] ; then
	rm $VERSIONFILE
fi

mono ${BINPATH}/oscript.exe -version | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+' \
		> ${BINPATH}/VERSION

if [ ! -f "$VERSIONFILE" ] ; then
	echo "No version file created"
	exit 1
fi
		
mkdir -p $DISTPATH/deb
cp -r ${PWD}/builders/deb/* $DISTPATH/deb

docker build -t onescript:deb ${PWD}/builders/deb/
docker run --rm -v ${DISTPATH}:/media onescript:deb 

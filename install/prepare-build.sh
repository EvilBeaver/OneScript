#!/bin/bash

echo 'Preparing environment'

DISTPATH=$(pwd)/build
BINPATH=${DISTPATH}/bin
cd `dirname $0`

docker volume create os_bld_output
docker run --name bldxchg -v os_bld_output:/bld busybox true

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

docker cp $VERSIONFILE bldxchg:/bld
rm $VERSIONFILE
docker cp build/* bldxchg:/bld/src
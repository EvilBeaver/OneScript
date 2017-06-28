#!/bin/bash

echo 'Preparing environment'

docker volume create os_bld_output
docker run --name bldxchg -v os_bld_output:/bld busybox true

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

tar -czvf sources.tar.gz build/*

docker cp build/* bldxchg:/bld/src
docker cp $VERSIONFILE bldxchg:/bld
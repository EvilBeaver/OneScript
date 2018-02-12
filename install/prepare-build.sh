#!/bin/bash

echo 'Preparing environment'

DISTPATH=$(pwd)/../built/tmp
BINPATH=${DISTPATH}/bin
cd `dirname $0`

docker volume create os_bld_output

if [ "$(docker ps -aq -f name=bldxchg)" ]; then
        echo 'Exchange exist. Run it'
        docker start bldxchg
else
        echo 'Creating Exchange'
        docker run --name bldxchg -v os_bld_output:/bld busybox rm -rf /bld/
fi

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
docker cp $DISTPATH/ bldxchg:/bld/src/

OUTPUT="../output"
if [ -d "$OUTPUT" ]; then
	rm -rf ${OUTPUT}
fi
mkdir ${OUTPUT}

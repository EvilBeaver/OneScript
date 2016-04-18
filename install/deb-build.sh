#!/bin/bash

BINPATH=$(cd $1; pwd)
cd `dirname $0`

if [ -z "$TMP" ] ; then
	TMP=/tmp
fi

TMPDIR=$TMP/oscript-deb-builder

mkdir -p $TMPDIR/deb
mkdir -p $TMPDIR/bin
mkdir -p $TMPDIR/lib

cp -r $BINPATH/* $TMPDIR/bin
cp -r ../install/builders/deb/* $TMPDIR/deb
cp -r ../oscript-library/* $TMPDIR/lib

VERSIONFILE=$BINPATH/VERSION
if [ -f "$VERSIONFILE" ] ; then
	rm $VERSIONFILE
fi

mono ${BINPATH}/oscript.exe | head -1 | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+' \
		> ${BINPATH}/VERSION

if [ ! -f "$VERSIONFILE" ] ; then
	echo "No version file created"
	exit 1
fi
		
docker build -t onescript:deb ${PWD}/builders/deb/
docker run --rm -v ${TMPDIR}:/media onescript:deb 

cp $TMPDIR/bin/*.deb $BINPATH
rm -rd $TMPDIR


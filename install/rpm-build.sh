#!/bin/bash

#set -exf
PROJECT=oscript
echo "$0"
echo "$1"
BINPATH=$(cd $1; pwd)
cd `dirname $0`

echo $BINPATH

if [ -z "$TMP" ] ; then
	TMP=/tmp
fi

VERSION=`mono ${BINPATH}/oscript.exe | head -1 | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+'`

TMPDIR=$TMP/OneScript-$VERSION

mkdir -p $TMPDIR/bin
mkdir -p $TMPDIR/lib

echo $BINPATH
cp -a $BINPATH/. $TMPDIR/bin
cp -r ../install/builders/deb/oscript $TMPDIR/oscript
cp -r ../oscript-library/. $TMPDIR/lib

pushd $TMP
pwd
ls | grep On

tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/ 
popd

mkdir -p $TMP/$PROJECT-$VERSION-buid
BUILDDIR=$TMP/$PROJECT-$VERSION-buid

cp -ra $TMP/OneScript-$VERSION.tar.gz $BUILDDIR/
cp -rf ../install/builders/rpm/oscript.spec $BUILDDIR/
	
docker build -t onescript:rpm ${PWD}/builders/rpm/
docker run --rm -e VERSION=$VERSION -v ${BUILDDIR}:/media onescript:rpm 

cp $BUILDDIR/RPMS/noarch/*.rpm $BINPATH/
cp $BUILDDIR/SRPMS/*.rpm $BINPATH/
rm -rd $BUILDDIR
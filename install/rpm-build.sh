#!/bin/bash

#set -exf
PROJECT=oscript
echo "$0"
echo "$1"
DISTPATH=$(cd $1; pwd)
BINPATH=$DISTPATH/bin
cd `dirname $0`

echo "Assets folder: $DISTPATH"
echo "Current dir: {$PWD}"

if [ -z "$TMP" ] ; then
	TMP=/tmp
fi

VERSION=`mono ${BINPATH}/oscript.exe | head -1 | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+'`

echo "Version is $VERSION"

TMPDIR=$TMP/OneScript-$VERSION
mkdir $TMPDIR

cp -r $DISTPATH/* $TMPDIR
cp -r ../install/builders/deb/oscript $TMPDIR/oscript

pushd $TMP
tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/
popd

mkdir -p $TMP/$PROJECT-$VERSION-build
BUILDDIR=$TMP/$PROJECT-$VERSION-build

cp -ra $TMP/OneScript-$VERSION.tar.gz $BUILDDIR/
cp -rf ./builders/rpm/oscript.spec $BUILDDIR/
	
docker build -t onescript:rpm ${PWD}/builders/rpm/
docker run --rm -e VERSION=$VERSION -v ${BUILDDIR}:/media onescript:rpm 

cp $BUILDDIR/RPMS/noarch/*.rpm $BINPATH/
cp $BUILDDIR/SRPMS/*.rpm $BINPATH/
rm -rd $BUILDDIR

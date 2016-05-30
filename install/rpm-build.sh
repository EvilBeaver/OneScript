#!/bin/bash

#set -exf
PROJECT=oscript
echo "$0"
echo "$1"
DISTPATH=$(cd $1; pwd)
BINPATH=$DISTPATH/bin
cd `dirname $0`

echo $BINPATH

if [ -z "$TMP" ] ; then
	TMP=/tmp
fi

VERSION=`mono ${BINPATH}/oscript.exe | head -1 | \
		grep -oE '([[:digit:]]+\.){2}[[:digit:]]+'`

echo "Version is $VERSION"

TMPDIR=$TMP/OneScript-$VERSION
mkdir $TMPDIR
#mkdir -p $TMPDIR/bin
#mkdir -p $TMPDIR/lib

cp -r $DISTPATH/* $TMPDIR

#cp -a $BINPATH/. $TMPDIR/bin
#cp -r ../install/builders/deb/oscript $TMPDIR/oscript
#cp -r ../oscript-library/. $TMPDIR/lib

pushd $TMP
tar -czvf OneScript-$VERSION.tar.gz $TMPDIR/ 
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

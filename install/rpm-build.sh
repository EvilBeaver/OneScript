#!/bin/bash

#set -exf
echo "Building RPM"

PROJECT=oscript
echo "$0"
echo "$1"
DISTPATH=$(cd $1; pwd)
BINPATH=$DISTPATH/bin
cd `dirname $0`

echo "Assets folder: $DISTPATH"
echo "Current dir: {$PWD}"


# if [ -z "$TMP" ] ; then
# 	TMP=/tmp
# fi

# VERSION=$(cat ${DISTPATH}/VERSION)
# echo "Version is $VERSION"

# TMPDIR=$TMP/OneScript-$VERSION
# mkdir $TMPDIR
# echo "Created TMPDIR $TMPDIR"

# echo "Copying sources to tmpdir"
# cp -r -v $DISTPATH/* $TMPDIR
# cp -r -v ../install/builders/deb/oscript $TMPDIR/oscript

# pushd $TMP
# echo "Compressing OneScript-$VERSION to tar"
# tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/
# popd

# mkdir -p $TMP/$PROJECT-$VERSION-build
# BUILDDIR=$TMP/$PROJECT-$VERSION-build
# echo "Created build dir: $BUILDDIR"

# cp -ra $TMP/OneScript-$VERSION.tar.gz $BUILDDIR/
# cp -rf ./builders/rpm/oscript.spec $BUILDDIR/
	
docker build -t onescript:rpm ${PWD}/builders/rpm/
docker run --rm -v os_bld_output:/media onescript:rpm 
docker cp bldxchg:/bld/rpm/RPMS/noarch/*.rpm ../output
docker cp bldxchg:/bld/rpm/SRPMS/*.rpm ../output

#cp $BUILDDIR/RPMS/noarch/*.rpm $BINPATH/
#cp $BUILDDIR/SRPMS/*.rpm $BINPATH/
#rm -rd $BUILDDIR

#!/bin/bash
# script run inside the container


#/media/src/bin;lib;examples;etc
#/media/VERSIONFILE
VERSIONFILE=/media/VERSION
DISTPATH=/media/src

VERSION=$(cat ${VERSIONFILE})
BLDTMP=/tmp
TMPDIR=${BLDTMP}/OneScript-$VERSION

echo "Copying sources to tmpdir"
cp -r -v $DISTPATH/* $TMPDIR
cp -r -v ${BLDTMP}/oscript $TMPDIR/oscript

pushd ${BLDTMP}
echo "Compressing OneScript-$VERSION to tar"
tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/
popd

BUILDDIR=/media/rpm
sudo mkdir -p ${BUILDDIR}

#cp -ra $BLDTMP/OneScript-$VERSION.tar.gz $BUILDDIR/
#cp -rf $BLDTMP/oscript.spec $BUILDDIR/

rpmdev-setuptree
define=""
if [ -z $VERSION ]; then
    echo ""
else 
   define="${define} --define '_version ${VERSION}'"
fi

if [ -z $RELEASE ]; then
    echo ""
else  
    define='$define --define "Release $RELEASE"'
fi

echo $define
sudo cp -arv $BLDTMP/* rpmbuild/SOURCES/
sudo cp -arv $BLDTMP/*.spec rpmbuild/SPECS/ 
rpmbuild -ba \
	--define "_version ${VERSION:-1.0.13}" \
	rpmbuild/SPECS/oscript.spec || exit 1

[[ -d $BUILDDIR ]] || exit 0

sudo mkdir -p $BUILDDIR/RPMS
sudo mkdir -p $BUILDDIR/SRPMS

sudo cp -ar rpmbuild/RPMS/ $BUILDDIR/
sudo cp -ar rpmbuild/SRPMS/ $BUILDDIR/

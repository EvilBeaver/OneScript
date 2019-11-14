#!/bin/bash
# script run inside the container

DATAROOT=$(pwd)/${ARTIFACTS_ROOT}

VERSIONFILE=/media/VERSION

VERSION=$(cat ${DATAROOT}/VERSION | grep -oE '([[:digit:]]+\.){2}[[:digit:]]+')
BLDTMP=/tmp
TMPDIR=${BLDTMP}/OneScript-$VERSION
mkdir -p $TMPDIR

echo "Copying sources to tmpdir"
cp -r -v $DISTPATH/bin $TMPDIR
cp -r -v $DISTPATH/lib $TMPDIR
cp -r -v $DISTPATH/doc $TMPDIR
cp -r -v $DISTPATH/examples $TMPDIR
cp -r -v ${BLDTMP}/oscript $TMPDIR/oscript
cp -r -v ${BLDTMP}/oscript-opm $TMPDIR/oscript-opm
cp -r -v ${BLDTMP}/oscript-opm-completion $TMPDIR/oscript-opm-completion

pushd ${BLDTMP}
echo "Compressing OneScript-$VERSION to tar"
tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/
popd

BUILDDIR=/media/rpm
sudo mkdir -p ${BUILDDIR}

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

#copy results
OUTPUT=/bld/out
mkdir $OUTPUT

mv $BUILDDIR/RPMS/noarch/*.rpm $OUTPUT
mv $BUILDDIR/SRPMS/*.rpm $OUTPUT
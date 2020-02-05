#!/bin/bash
# script run inside the container

DATAROOT=$(pwd)/${ARTIFACTS_ROOT}

VERSION=$(cat ${DATAROOT}/VERSION | grep -oE '([[:digit:]]+\.){2}[[:digit:]]+')
BLDTMP=/tmp/rpm-src
TMPDIR=${BLDTMP}/OneScript-$VERSION
DISTPATH=${RPMSOURCE}
mkdir -p $TMPDIR

echo "Copying sources to tmpdir"
cp -r -v $DISTPATH/bin $TMPDIR
cp -r -v $DISTPATH/lib $TMPDIR
cp -r -v $DISTPATH/doc $TMPDIR
cp -r -v $DISTPATH/examples $TMPDIR
cp -r -v ${RPMSOURCE}/oscript $TMPDIR/oscript
cp -r -v ${RPMSOURCE}/oscript-opm $TMPDIR/oscript-opm
cp -r -v ${RPMSOURCE}/oscript-opm-completion $TMPDIR/oscript-opm-completion

pushd ${BLDTMP}
echo "Compressing OneScript-$VERSION to tar"
tar -czvf OneScript-$VERSION.tar.gz OneScript-$VERSION/
popd

BUILDDIR=/tmp/rpm-out
mkdir -p ${BUILDDIR}

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
cp -arv $RPMSOURCE/* ~/rpmbuild/SOURCES/
cp -arv $BLDTMP/*.tar.gz ~/rpmbuild/SOURCES/
cp -arv $RPMSOURCE/*.spec ~/rpmbuild/SPECS/ 
rpmbuild -ba \
	--define "_version ${VERSION:-1.0.13}" \
	~/rpmbuild/SPECS/oscript.spec || exit 1

[[ -d $BUILDDIR ]] || exit 0

mkdir -p $BUILDDIR/RPMS
mkdir -p $BUILDDIR/SRPMS

cp -ar ~/rpmbuild/RPMS/ $BUILDDIR/
cp -ar ~/rpmbuild/SRPMS/ $BUILDDIR/

#copy results
OUTPUT=$(pwd)/out/rpm
mkdir -p $OUTPUT

mv $BUILDDIR/RPMS/noarch/*.rpm $OUTPUT
mv $BUILDDIR/SRPMS/*.rpm $OUTPUT

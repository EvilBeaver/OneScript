#!/bin/sh

DATAROOT=$(pwd)/${ARTIFACTS_ROOT}
SRCPATH=${DATAROOT}
BINPATH=${SRCPATH}/bin/
DEBBUILDROOT=/tmp/deb/
BUILDERROOT=/opt/deb/

if [ -d "$DEBBUILDROOT" ]; then
    rm -rf $DEBBUILDROOT
    mkdir -p $DEBBUILDROOT
fi

ls /
ls /built
ls /built/tmp
ls /built/tmp/bin
ls BINPATH=${SRCPATH}
ls BINPATH=${SRCPATH}/bin

VERSION=$(cat ${DATAROOT}/VERSION | grep -oE '([[:digit:]]+\.){2}[[:digit:]]+')
PAKNAME=onescript-engine
DSTPATH=${DEBBUILDROOT}${PAKNAME}

mkdir -p $DSTPATH
mkdir -p $DSTPATH/DEBIAN
mkdir -p $DSTPATH/usr/bin
mkdir -p $DSTPATH/usr/share/oscript/lib
mkdir -p $DSTPATH/usr/share/oscript/bin
mkdir -p $DSTPATH/etc
mkdir -p $DSTPATH/etc/bash_completion.d

cp ${BUILDERROOT}settings/dirs $DSTPATH/DEBIAN/
cat ${BUILDERROOT}settings/control | sed -r "s/VERSION/$VERSION/g" > $DSTPATH/DEBIAN/control
cp ${BINPATH}*.exe $DSTPATH/usr/share/oscript/bin
cp ${BINPATH}*.dll $DSTPATH/usr/share/oscript/bin
cp ${BINPATH}*.so $DSTPATH/usr/share/oscript/bin
cp ${BUILDERROOT}oscript $DSTPATH/usr/bin
cp ${BUILDERROOT}oscript-cgi $DSTPATH/usr/bin
cp ${BUILDERROOT}oscript-opm $DSTPATH/usr/bin
cp ${BUILDERROOT}oscript-opm-completion $DSTPATH/etc/bash_completion.d
cp -r ${SRCPATH}/lib/* $DSTPATH/usr/share/oscript/lib
cp ${BINPATH}/oscript.cfg $DSTPATH/etc

ln -s /usr/bin/oscript-opm $DSTPATH/usr/bin/opm

# TODO: Убрать это!
cp ${BINPATH}/oscript.cfg $DSTPATH/usr/share/oscript/bin

fakeroot dpkg-deb --build $DSTPATH

rm -rf $DSTPATH
chmod 777 $DSTPATH.deb
dpkg-name -o $DSTPATH.deb

#copy results
OUTPUT=out/deb
mkdir -p $OUTPUT

cp $DEBBUILDROOT/*.deb $OUTPUT


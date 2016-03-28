#!/bin/sh

# TODO: $1
SRCPATH=/media/
BINPATH=${SRCPATH}src/oscript/bin/Release/
DEBBUILDROOT=${SRCPATH}src/oscript/bin/
BUILDERROOT=${SRCPATH}install/builders/deb/

VERSION=$(cat ${BINPATH}VERSION)
PAKNAME=onescript-engine
DSTPATH=${DEBBUILDROOT}${PAKNAME}

mkdir $DSTPATH 
mkdir -p $DSTPATH/DEBIAN
mkdir -p $DSTPATH/usr/bin
mkdir -p $DSTPATH/usr/share/oscript/lib
mkdir -p $DSTPATH/usr/share/oscript/bin
mkdir -p $DSTPATH/etc

cp ${BUILDERROOT}settings/dirs $DSTPATH/DEBIAN/
cat ${BUILDERROOT}settings/control | sed -r "s/VERSION/$VERSION/g" > $DSTPATH/DEBIAN/control
cp ${BINPATH}*.exe $DSTPATH/usr/share/oscript/bin
cp ${BINPATH}*.dll $DSTPATH/usr/share/oscript/bin
cp ${BUILDERROOT}oscript $DSTPATH/usr/bin
cp ${BUILDERROOT}oscript-cgi $DSTPATH/usr/bin
cp -r ${SRCPATH}/oscript-library/src/* $DSTPATH/usr/share/oscript/lib
cp ${BINPATH}/oscript.cfg $DSTPATH/etc

# TODO: Убрать это!
cp ${BINPATH}/oscript.cfg $DSTPATH/usr/share/oscript/bin

fakeroot dpkg-deb --build $DSTPATH

rm -rf $DSTPATH
chmod 777 $DSTPATH.deb

####
#	Тестирование. TODO: Вынести в отдельный контейнер
####

# проверим установку

dpkg-name --overwrite $DSTPATH.deb
find -name '*.deb' | xargs dpkg --force-depends --install 

# запуск тестов
oscript ${SRCPATH}tests/testrunner.os -runall ${SRCPATH}tests


#!/bin/bash

# TODO: $1
SRCPATH=/media/
DEBBUILDPATH=${SRCPATH}src/oscript/bin/

mkdir ${DEBBUILDPATH}oscript-deb-core 
mkdir -p ${DEBBUILDPATH}oscript-deb-core/DEBIAN
mkdir -p ${DEBBUILDPATH}oscript-deb-core/usr/bin
mkdir -p ${DEBBUILDPATH}oscript-deb-core/usr/lib/oscript

xbuild /p:Configuration=Release /p:Platform="Any CPU" ${SRCPATH}src/1Script_Mono.sln

cp ${SRCPATH}install/builders/deb/settings/* ${DEBBUILDPATH}oscript-deb-core/DEBIAN/

cp ${SRCPATH}src/oscript/bin/Release/*.exe ${DEBBUILDPATH}oscript-deb-core/usr/bin 
cp ${SRCPATH}src/oscript/bin/Release/*.dll ${DEBBUILDPATH}oscript-deb-core/usr/bin

fakeroot dpkg-deb --build ${DEBBUILDPATH}oscript-deb-core

# check install

dpkg --install ${DEBBUILDPATH}oscript-deb-core.deb && \
# output version
	mono /usr/bin/oscript.exe | head -1

# run tests


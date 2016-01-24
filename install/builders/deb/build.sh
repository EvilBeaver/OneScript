#!/bin/bash

# TODO: $1
SRCPATH=/media/
BINPATH=${SRCPATH}src/oscript/bin/Release/
DEBBUILDROOT=${SRCPATH}src/oscript/bin/

# Хитрый способ вычленения версии ОдноСкрипта
VERSION=$(mono ${BINPATH}oscript.exe | head -1 | grep -oE '([[:digit:]]+\.){3,3}[[:digit:]]+')
PAKNAME=onescript-engine_${VERSION}


mkdir ${DEBBUILDROOT}${PAKNAME} 
mkdir -p ${DEBBUILDROOT}${PAKNAME}/DEBIAN
mkdir -p ${DEBBUILDROOT}${PAKNAME}/usr/bin
mkdir -p ${DEBBUILDROOT}${PAKNAME}/usr/lib/oscript

cp ${SRCPATH}install/builders/deb/settings/* ${DEBBUILDROOT}${PAKNAME}/DEBIAN/
cp ${BINPATH}*.exe ${DEBBUILDROOT}${PAKNAME}/usr/bin 
cp ${BINPATH}*.dll ${DEBBUILDROOT}${PAKNAME}/usr/bin

fakeroot dpkg-deb --build ${DEBBUILDROOT}${PAKNAME}

rm -rf ${DEBBUILDROOT}${PAKNAME}
chmod 777 ${DEBBUILDROOT}${PAKNAME}.deb

# проверим установку

dpkg --install ${DEBBUILDROOT}${PAKNAME}.deb && \
# вывод версии
	mono /usr/bin/oscript.exe | head -1

# запуск тестов


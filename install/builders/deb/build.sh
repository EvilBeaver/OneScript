#!/bin/sh

# TODO: $1
SRCPATH=/media/
BINPATH=${SRCPATH}src/oscript/bin/Release/
DEBBUILDROOT=${SRCPATH}src/oscript/bin/
BUILDERROOT=${SRCPATH}install/builders/deb/

VERSION=$(cat ${BINPATH}VERSION)
PAKNAME=onescript-engine_${VERSION}

mkdir ${DEBBUILDROOT}${PAKNAME} 
mkdir -p ${DEBBUILDROOT}${PAKNAME}/DEBIAN
mkdir -p ${DEBBUILDROOT}${PAKNAME}/usr/bin
mkdir -p ${DEBBUILDROOT}${PAKNAME}/usr/lib/oscript

cp ${BUILDERROOT}settings/dirs ${DEBBUILDROOT}${PAKNAME}/DEBIAN/
cat ${BUILDERROOT}settings/control | sed -r "s/VERSION/$VERSION/g" > ${DEBBUILDROOT}${PAKNAME}/DEBIAN/control
cp ${BINPATH}*.exe ${DEBBUILDROOT}${PAKNAME}/usr/bin/ 
cp ${BINPATH}*.dll ${DEBBUILDROOT}${PAKNAME}/usr/bin/
cp ${BUILDERROOT}oscript ${DEBBUILDROOT}${PAKNAME}/usr/bin/
cp ${BUILDERROOT}oscript-cgi ${DEBBUILDROOT}${PAKNAME}/usr/bin/

fakeroot dpkg-deb --build ${DEBBUILDROOT}${PAKNAME}

rm -rf ${DEBBUILDROOT}${PAKNAME}
chmod 777 ${DEBBUILDROOT}${PAKNAME}.deb

####
#	Тестирование. TODO: Вынести в отдельный контейнер
####

# проверим установку

dpkg --force-depends --install ${DEBBUILDROOT}${PAKNAME}.deb && apt-get -f -y install

# запуск тестов
oscript ${SRCPATH}tests/testrunner.os -runall ${SRCPATH}tests


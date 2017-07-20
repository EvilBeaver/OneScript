SOLUTION_FILE=src/1Script_Mono.sln
PLATFORM="Any CPU"
CONFIGURATION=Release
SOURCEBINDIR=src/oscript/bin/${CONFIGURATION}
BIN_OUTPUTDIR=dist/bin
LIB_OUTPUTDIR=dist/lib
OSCRIPTEXE=${BIN_OUTPUTDIR}/oscript.exe
OPMOS=${LIB_OUTPUTDIR}/opm/opm.os
OPM="${OSCRIPTEXE} ${OPMOS}"
PREFIX=/usr

all: dist

dist: ${OSCRIPTEXE} lib

lib: ${OSCRIPTEXE}
	test -d ${LIB_OUTPUTDIR} || mkdir -p ${LIB_OUTPUTDIR}
	cp -r oscript-library/src/* ${LIB_OUTPUTDIR}

NUGET:
	nuget restore ${SOLUTION_FILE}

${OSCRIPTEXE}: NUGET
	msbuild /p:Platform=${PLATFORM} /p:Configuration=${CONFIGURATION} ${SOLUTION_FILE}
	test -d ${BIN_OUTPUTDIR} || mkdir -p ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.dll ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.exe ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.cfg ${BIN_OUTPUTDIR}

clean:
	rm -rf ${BIN_OUTPUTDIR}
	rm -rf ${LIB_OUTPUTDIR}

${OPMOS}:

install: install_bin install_lib

install_bin:
	mkdir -p ${PREFIX}/share/oscript/bin
	cp ${BIN_OUTPUTDIR}/* ${PREFIX}/share/oscript/bin
	cp install/builders/deb/oscript ${PREFIX}/bin/oscript
	cp install/builders/deb/oscript-cgi ${PREFIX}/bin/oscript-cgi

install_lib:
	mkdir -p ${PREFIX}/share/oscript/lib
	cp -r ${LIB_OUTPUTDIR}/* ${PREFIX}/share/oscript/lib
	cp install/builders/deb/oscript-opm ${PREFIX}/bin/oscript-opm
	ln -s ${PREFIX}/bin/oscript-opm ${PREFIX}/bin/opm

uninstall: uninstall_lib uninstall_bin

uninstall_bin:
	rm -rf ${PREFIX}/share/oscript/bin
	rm ${PREFIX}/bin/oscript
	rm ${PREFIX}/bin/oscript-cgi

uninstall_lib:
	rm ${PREFIX}/bin/opm
	rm ${PREFIX}/bin/oscript-opm
	rm -rf ${PREFIX}/share/oscript/lib

.PHONY: all install uninstall dist lib clean

PLATFORM="Any CPU"
CONFIGURATION=Release
SOURCEBINDIR=src/oscript/bin/${CONFIGURATION}
BIN_OUTPUTDIR=bin
LIB_OUTPUTDIR=lib
OSCRIPTEXE=${BIN_OUTPUTDIR}/oscript.exe
OPMOS=${LIB_OUTPUTDIR}/opm/opm.os
OPM="${OSCRIPTEXE} ${OPMOS}"

all: dist

dist: ${OSCRIPTEXE} lib

lib: ${OSCRIPTEXE}
	test -d ${LIB_OUTPUTDIR} && rm -rf ${LIB_OUTPUTDIR}
	mkdir -p ${LIB_OUTPUTDIR}
	cp -r oscript-library/src/* ${LIB_OUTPUTDIR}

${OSCRIPTEXE}:
	xbuild /p:Platform=${PLATFORM} /p:Configuration=${CONFIGURATION} src/1Script_Mono.sln
	test -d ${BIN_OUTPUTDIR} || mkdir -p ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.dll ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.exe ${BIN_OUTPUTDIR}
	cp ${SOURCEBINDIR}/*.cfg ${BIN_OUTPUTDIR}

clean:
	rm -rf ${BIN_OUTPUTDIR}
	rm -rf ${LIB_OUTPUTDIR}

${OPMOS}:

.PHONY: all install uninstall dist lib clean

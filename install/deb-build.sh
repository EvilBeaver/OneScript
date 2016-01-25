#!/bin/bash

SRCPATH=/media
BINPATH=${PWD}/../src/oscript/bin/Release/
mono ${BINPATH}oscript.exe | head -1 | \
		grep -oE '([[:digit:]]+\.){3,3}[[:digit:]]+' \
		> ${BINPATH}VERSION

docker build -t onescript:deb ${PWD}/builders/deb/
docker run -v ${PWD}/..:${SRCPATH} --name onescript_deb onescript:deb ${SRCPATH}
docker rm onescript_deb


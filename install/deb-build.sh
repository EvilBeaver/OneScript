#!/bin/bash

rm -rf ../src/oscript/bin/

SRCPATH=/media

docker build -t onescript:deb ${PWD}/builders/deb/
docker run -v ${PWD}/..:${SRCPATH} --name onescript_deb onescript:deb ${SRCPATH}
docker rm onescript_deb


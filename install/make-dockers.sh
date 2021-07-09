#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/../src/ScriptEngine.NativeApi
docker build -t gcc -f $THISDIR/builders/nativeapi/Dockerfile .
docker run gcc sh ./build.sh
docker container cp gcc:/build/src/ .

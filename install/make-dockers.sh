#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/../src/ScriptEngine.NativeApi
chmod +x build.sh
docker build --no-cache -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile .
docker run -v $(pwd):/src oscript/onescript-builder:gcc

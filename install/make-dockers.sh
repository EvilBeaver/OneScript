#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/../src/ScriptEngine.NativeApi
docker build --no-cache -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile .
docker run --mount type=bind,source=$(pwd),target=/build/src oscript/onescript-builder:gcc

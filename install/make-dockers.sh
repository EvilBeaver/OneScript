#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/builders/gcc
docker build --no-cache -t oscript/onescript-builder:gcc .

cd $THISDIR/../src/ScriptEngine.NativeApi
docker run -v $(pwd):/src oscript/onescript-builder:gcc sh ./build.sh

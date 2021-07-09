#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/../src/ScriptEngine.NativeApi
docker build -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile .
docker run --rm -i -v $(pwd):/build/src oscript/onescript-builder:gcc sh ./build.sh

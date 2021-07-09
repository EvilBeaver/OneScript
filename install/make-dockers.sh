#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/../src/ScriptEngine.NativeApi
docker build -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile .
docker cp gcc:/build/build32/ScriptEngine.NativeApi32.so .
docker cp gcc:/build/build64/ScriptEngine.NativeApi64.so .

#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/../src/ScriptEngine.NativeApi
mkdir build
cd build
cmake .. 
cmake --build .
cp ScriptEngine.NativeApi.dll $THISDIR
cd $THISDIR

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .
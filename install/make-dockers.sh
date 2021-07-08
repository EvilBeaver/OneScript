#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR/builders/nativeapi
docker build -f $THISDIR/../srv/ScriptEngine.NativeApi -t oscript/onescript-builder:gcc .

#!/bin/bash

THISDIR=$(pwd)
docker build --no-cache -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile ..

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

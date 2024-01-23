#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t oscript/onescript-builder:rpm .

cd $THISDIR
docker build -t oscript/onescript-builder:gcc-1x -f $THISDIR/builders/nativeapi/Dockerfile ..

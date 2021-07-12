#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build --no-cache -t oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build --no-cache -t oscript/onescript-builder:rpm .

cd $THISDIR
docker build --no-cache -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile ..

#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t --no-cache oscript/onescript-builder:deb .

cd $THISDIR/builders/rpm
docker build -t --no-cache oscript/onescript-builder:rpm .

cd $THISDIR
docker build -t --no-cache oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile ..

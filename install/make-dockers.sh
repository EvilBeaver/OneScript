#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR
docker build -t oscript/onescript-builder:gcc -f $THISDIR/builders/nativeapi/Dockerfile ..

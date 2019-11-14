#!/bin/bash

THISDIR=$(pwd)

cd $THISDIR/builders/deb
docker build -t onescript-build:deb .

cd $THISDIR/builders/rpm
docker build -t onescript-build:rpm .
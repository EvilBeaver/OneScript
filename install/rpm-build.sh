#!/bin/bash

#set -exf
echo "Building RPM"

PROJECT=oscript
#echo "$0"
#echo "$1"
#DISTPATH=$(cd $1; pwd)
#BINPATH=$DISTPATH/bin
#cd `dirname $0`

#echo "Assets folder: $DISTPATH"
echo "Current dir: {$PWD}"
	
docker build -t onescript:rpm ${PWD}/builders/rpm/
docker run --rm -v os_bld_output:/media onescript:rpm 

TMPOUT=../output
rm -rf $TMPOUT/rpm

docker cp bldxchg:/bld/rpm/ $TMPOUT
if [ $? -ne 0 ]; then
    exit 1
fi

mv $TMPOUT/rpm/RPMS/noarch/*.rpm $TMPOUT
mv $TMPOUT/rpm/SRPMS/*.rpm $TMPOUT
rm -rf $TMPOUT/rpm
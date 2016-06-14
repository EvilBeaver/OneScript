#!/bin/bash
# script run inside the container
rpmdev-setuptree
define=""
if [ -z $VERSION ]; then
    echo ""
else 
   define="${define} --define '_version ${VERSION}'"
fi

if [ -z $RELEASE ]; then
    echo ""
else  
    define='$define --define "Release $RELEASE"'
fi

echo $define
sudo cp -arv /media/* rpmbuild/SOURCES/
sudo cp -arv /media/*.spec rpmbuild/SPECS/ 
rpmbuild -ba \
	--define "_version ${VERSION:-1.0.13}" \
	rpmbuild/SPECS/oscript.spec || exit 1

[[ -d /media ]] || exit 0

sudo mkdir -p /media/RPMS
sudo mkdir -p /media/SRPMS

sudo cp -ar rpmbuild/RPMS/ /media/
sudo cp -ar rpmbuild/SRPMS/ /media/

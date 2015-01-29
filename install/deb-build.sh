#!/bin/bash


docker build -t ubuntu/dpkg ./builders/deb/


docker run ubuntu/dpkg lintian /workspace/oscript-deb-core.deb


docker run ubuntu/dpkg sudo dpkg -i /workspace/oscript-deb-core.deb


docker run -i -v ${PWD}/dist:/dist ubuntu/dpkg cp -u /workspace/oscript-deb-core.deb /dist/oscript-core_$BUILD_NAME_all.deb




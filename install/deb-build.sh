#!/bin/bash


docker build -t ubuntu/dpkg ${PWD}/builders/deb/


docker run ubuntu/dpkg lintian /workspace/oscript-deb-core.deb


docker run ubuntu/dpkg sudo dpkg -i /workspace/oscript-deb-core.deb


docker run -i -v ${PWD}/dist:/dist ubuntu/dpkg cp -u /workspace/oscript-deb-core.deb /dist/oscript-core_1.0.9.$BUILD_NUMBER_all.deb




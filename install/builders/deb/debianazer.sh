#!/bin/bash

docker build . -t dpkg/ubuntu

docker run -rm dpkg/ubuntu lintian /workspace/oscript-engine.deb

docker run -rm dpkg/ubuntu lintian sudo dpkg -i /workspace/oscript-engine.deb

mv ./../../dist/oscript-engine.deb ./../../dist/oscript-engine_$BUILD_NUMBER_all.deb

docker rmi dpkg/ubuntu

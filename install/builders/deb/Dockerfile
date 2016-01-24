FROM ubuntu:latest


MAINTAINER sergey.batanov@dmpas.ru 

# Add mono repository
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
		echo "deb http://download.mono-project.com/repo/debian wheezy main" > /etc/apt/sources.list.d/mono-xamarin.list

RUN apt-get update && apt-get install -y \
# mono build tools
		mono-xbuild \
		mono-devel \
# deb-package tools
        dpkg\
        debconf\
        debhelper\
        lintian\
        md5deep\
        fakeroot

RUN mkdir /workspace && mkdir ~/oscript-deb-core
ADD ./build.sh /workspace/ 
ENTRYPOINT /workspace/build.sh 


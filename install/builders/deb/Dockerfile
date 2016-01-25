FROM ubuntu:latest


MAINTAINER sergey.batanov@dmpas.ru 

# чтобы запустить тесты
RUN locale-gen --lang ru_RU.UTF-8
ENV LANG ru_RU.UTF-8

# Add mono repository
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
		echo "deb http://download.mono-project.com/repo/debian wheezy main" > /etc/apt/sources.list.d/mono-xamarin.list

RUN apt-get update
RUN apt-get install -y \
# deb-package tools
        dpkg\
        debconf\
        debhelper\
        lintian\
        md5deep\
        fakeroot

ADD ./build.sh /root/ 
ENTRYPOINT /root/build.sh 


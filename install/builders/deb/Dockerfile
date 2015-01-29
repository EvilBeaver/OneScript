FROM ubuntu:latest


MAINTAINER alustin@silverbulleters.org

RUN apt-get update && apt-get install -y \
        dpkg\
        debconf\
        debhelper\
        lintian\
        unzip\
        md5deep\
        fakeroot

RUN mkdir ~/oscript-deb-core

RUN mkdir -p ~/oscript-deb-core/DEBIAN
RUN mkdir -p ~/oscript-deb-core/usr/bin
RUN mkdir -p ~/oscript-deb-core/usr/lib/oscript

RUN mkdir -p /workspace

ADD ./*.zip /workspace/

ADD ./settings /settings

RUN ls -a /settings

RUN cp /settings/control ~/oscript-deb-core/DEBIAN/
RUN cp /settings/dirs ~/oscript-deb-core/DEBIAN/

RUN unzip -o -u /workspace/*.zip -d ~/oscript-deb-core/usr/lib/oscript

RUN ln -s ~/oscript-deb-core/usr/lib/oscript/oscript.exe ~/oscript-deb-core/usr/bin/oscript.exe

RUN chmod +x ~/oscript-deb-core/usr/lib/oscript/oscript.exe

RUN md5deep -r ~/oscript-deb-core/usr/ > ~/oscript-deb-core/DEBIAN/md5sums

RUN cat ~/oscript-deb-core/DEBIAN/md5sums


RUN cd ~/

RUN ls -a ~/oscript-deb-core/DEBIAN

RUN fakeroot dpkg-deb --build ~/oscript-deb-core

RUN ls ~/

RUN cp ~/oscript-deb-core.deb /workspace/

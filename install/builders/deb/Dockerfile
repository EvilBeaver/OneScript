FROM ubuntu

MAINTAINER alustin@silverbulleters.org

RUN apt-get install dpkg debconf debhelper lintian unzip md5deep fakeroot

RUN mkdir ~/oscript-deb-core

RUN mkdir -p ~/oscript-deb-core/DEBIAN
RUN mkdir -p ~/oscript-deb-core/usr/bin
RUN mkdir -p ~/oscript-deb-core/usr/lib/oscript

ADD ./../../dist/ /workspace/

ADD ./settings /settings

RUN unzip -o -u /workspace/*.zip -d ~/oscript-deb-core/usr/lib/oscript  

RUN ln -s ~/oscript-deb-core/usr/lib/oscript/oscript.exe ~/oscript-deb-core/usr/bin/oscript.exe

RUN chmod +x ~/oscript-deb-core/usr/lib/oscript/oscript.exe

COPY /settings/control ~/oscript-deb-core/DEBIAN/

COPY /settings/dirs ~/oscript-deb-core/DEBIAN/dirs

RUN md5deep -r ~/oscript-deb-core/usr ~/oscript-deb-core/DEBIAN/md5sums

RUN cd ~/oscript-deb-core/

RUN fakeroot dpkg-deb --build oscript-engine

COPY oscript-edgine.deb /workspace



 
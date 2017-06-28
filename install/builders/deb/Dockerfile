FROM ubuntu:16.04


MAINTAINER sergey.batanov@dmpas.ru

# Add mono repository
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
		echo "deb http://download.mono-project.com/repo/debian wheezy main" > /etc/apt/sources.list.d/mono-xamarin.list

RUN apt-get update && apt-get install -y \
# runtime dependencies
		mono-runtime \
		libmono-system-core4.0-cil \
		libmono-system4.0-cil \
		libmono-corlib4.0-cil \
		libmono-i18n4.0-all \
# deb-package tools
		dpkg\
		debconf\
		debhelper\
		lintian\
		md5deep\
		fakeroot \
		locales

# чтобы запустить тесты
RUN locale-gen --lang ru_RU.UTF-8
ENV LANG ru_RU.UTF-8

ADD ./build.sh /root/
VOLUME /media

ENTRYPOINT /root/build.sh


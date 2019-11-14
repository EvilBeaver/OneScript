FROM ubuntu:16.04
MAINTAINER sergey.batanov@dmpas.ru


RUN apt-get update && apt-get install -y \
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
ENV ARTIFACTS_ROOT built/tmp

COPY ./ /opt/deb/
RUN cp /opt/deb/build.sh /bld/build.sh && chmod +x /bld/build.sh



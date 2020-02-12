FROM fedora:26
MAINTAINER Chmouel Boudjnah <chmouel@enovance.com>


# Install various packages to get compile environment
RUN dnf update -y && dnf -y group install 'Development Tools'

# Install git command to access GitHub repository
RUN dnf -y install sudo git rpmdevtools rpm-build yum-utils dnf-plugins-core

RUN sed -i.bak -n -e '/^Defaults.*requiretty/ { s/^/# /;};/^%wheel.*ALL$/ { s/^/# / ;} ;/^#.*wheel.*NOPASSWD/ { s/^#[ ]*//;};p' /etc/sudoers

# This is an optimisation for caching, since using the auto generated one will
# make docker always run the builddep steps since new file
ENV RPMSOURCE /opt/rpm
COPY ./ ${RPMSOURCE}

RUN dnf builddep -y --spec ${RPMSOURCE}/oscript.spec

#uid 1000 is used by jenkins agent
RUN useradd -s /bin/bash -u 1000 -G adm,wheel,systemd-journal -m rpm

ENV VERSION ""
ENV RELEASE ""
ENV ARTIFACTS_ROOT built/tmp

RUN mkdir /bld && cp /opt/rpm/build.sh /bld/build.sh && chmod +x /bld/build.sh

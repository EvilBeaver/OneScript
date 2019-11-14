FROM fedora:26
MAINTAINER Chmouel Boudjnah <chmouel@enovance.com>


# Install various packages to get compile environment
RUN dnf update -y && dnf -y group install 'Development Tools'

# Install git command to access GitHub repository
RUN dnf -y install sudo git rpmdevtools rpm-build yum-utils dnf-plugins-core

RUN sed -i.bak -n -e '/^Defaults.*requiretty/ { s/^/# /;};/^%wheel.*ALL$/ { s/^/# / ;} ;/^#.*wheel.*NOPASSWD/ { s/^#[ ]*//;};p' /etc/sudoers

# This is an optimisation for caching, since using the auto generated one will
# make docker always run the builddep steps since new file
COPY ./ /tmp

RUN dnf builddep -y --spec /tmp/oscript.spec

ADD start.sh /start.sh

RUN useradd -s /bin/bash -G adm,wheel,systemd-journal -m rpm

ENV VERSION ""
ENV RELEASE ""
ENV ARTIFACTS_ROOT built/tmp

CMD /tmp/start.sh
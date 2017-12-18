FROM mono
 
MAINTAINER oscript.io team

RUN apt-get update \
        && apt-get update \
        && apt-get install mono-devel apache2 libapache2-mod-mono mono-apache-server4 -y --no-install-recommends \
        && a2enmod mod_mono \
        && service apache2 stop \
        && apt-get autoremove -y \
        && apt-get clean \
        && rm -rf /var/tmp/* \
        && rm -rf /var/lib/apt/lists/* \
        && mkdir -p /etc/mono/registry /etc/mono/registry/LocalMachine \
        && sed -ri ' \
            s!^(\s*CustomLog)\s+\S+!\1 /proc/self/fd/1!g; \
            s!^(\s*ErrorLog)\s+\S+!\1 /proc/self/fd/2!g; \
            ' /etc/apache2/apache2.conf

ADD ./nixconfig/apache2-site.conf /etc/apache2/sites-available/default

RUN mkdir -p /srv/www/mono.localhost

COPY ./bin/Debug/ /srv/www/mono.localhost
COPY ./nixconfig/Web.config /srv/www/mono.localhost

RUN ls /srv/www/mono.localhost

RUN /usr/bin/mod-mono-server4 --version

WORKDIR /var/www
EXPOSE 80
CMD ["/usr/sbin/apache2ctl", "-D", "FOREGROUND"]
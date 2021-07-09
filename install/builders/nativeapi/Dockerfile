FROM ubuntu:18.04

VOLUME [ "/build/src" ]

WORKDIR /build/src

RUN apt-get update -qq \
    && sudo dpkg --add-architecture i386 \
    && apt-get install -y -qq cmake gcc-multilib g++-multilib \
    && chmod +x ./build.sh \
    && ./build.sh

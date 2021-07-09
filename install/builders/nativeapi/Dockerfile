FROM ubuntu:18.04

VOLUME [ "/build/src" ]

WORKDIR /build/src

RUN apt-get update -qq \
    && apt-get install -y -qq \
        libc6-dev libc6-dev:i386 \
        cmake gcc-multilib g++-multilib \
    && chmod +x ./build.sh \
    && ./build.sh

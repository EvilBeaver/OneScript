FROM gcc:ubuntu18.4

VOLUME [ "/build/src" ]

WORKDIR /build/src

RUN apt-get update -qq && \
    apt-get install -y -qq \
        cmake libc6-dev libc6-dev:i386 \
        gcc-multilib g++-multilib && \
    chmod +x ./build.sh && \
    ./build.sh

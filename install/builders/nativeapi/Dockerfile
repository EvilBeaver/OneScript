FROM gcc:latest

VOLUME [ "/build/src" ]

WORKDIR /build/src

RUN apt-get update && \
    apt-get install -y cmake libc6-dev && \
    rm -rf /var/lib/apt/lists/* 

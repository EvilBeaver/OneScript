FROM gcc:latest

VOLUME [ "/build/src" ]

WORKDIR /build/src

RUN apt-get update && \
    apt-get install -y cmake && \
    rm -rf /var/lib/apt/lists/* 

FROM gcc:latest

VOLUME [ "/build/out" ]
VOLUME [ "/build/src" ]

WORKDIR /build/out

RUN apt-get update && \
    apt-get install -y cmake && \
    rm -rf /var/lib/apt/lists/* 

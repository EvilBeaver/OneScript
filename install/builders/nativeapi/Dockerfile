FROM gcc:latest

VOLUME [ "/build/src" ]

WORKDIR /build/src/ScriptEngine.NativeApi

RUN apt-get update && \
    apt-get install -y cmake libc6-dev && \
    cd /build/src/ScriptEngine.NativeApi && \
    ./build.sh && \
    rm -rf /var/lib/apt/lists/* 

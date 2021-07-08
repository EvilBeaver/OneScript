FROM gcc:latest

VOLUME [ "/build/src" ]

ADD ./src /build/src

WORKDIR /build/src/ScriptEngine.NativeApi

RUN apt-get update && \
    apt-get install -y cmake libc6-dev && \
    ./build.sh && \
    rm -rf /var/lib/apt/lists/* 

COPY --from=build /build/src/ScriptEngine.NativeApi/build64/ScriptEngine.NativeApi32.so .

COPY --from=build /build/src/ScriptEngine.NativeApi/build64/ScriptEngine.NativeApi64.so .
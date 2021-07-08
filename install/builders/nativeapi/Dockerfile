FROM gcc:latest

COPY . /build

WORKDIR /build

RUN apt-get update && \
    apt-get install -y cmake libc6-dev && \
    ./build.sh

COPY --from=build /build/src/ScriptEngine.NativeApi/build64/ScriptEngine.NativeApi32.so .

COPY --from=build /build/src/ScriptEngine.NativeApi/build64/ScriptEngine.NativeApi64.so .
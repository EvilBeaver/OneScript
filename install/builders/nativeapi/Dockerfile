FROM ubuntu:18.04 AS gcc

COPY . /build

WORKDIR /build

RUN apt-get update -qq \
    && dpkg --add-architecture i386 \
    && apt-get install -y -qq build-essential cmake uuid-dev libc6-dev gcc-multilib g++-multilib \
    && chmod +x ./build.sh \
    && ./build.sh

COPY gcc:/build/build32/ScriptEngine.NativeApi32.so .

COPY gcc:/build/build64/ScriptEngine.NativeApi64.so .

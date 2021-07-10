FROM ubuntu:18.04

RUN apt-get update -qq \
    && dpkg --add-architecture i386 \
    && apt-get install -y -qq build-essential cmake uuid-dev libc6-dev gcc-multilib g++-multilib

WORKDIR /src/proxy
COPY /src/ScriptEngine.NativeApi /src/proxy
RUN chmod +x build.sh && ./build.sh

WORKDIR /src/tests
COPY /tests/native-api /src/tests
RUN chmod +x build.sh && ./build.sh

RUN cd / \
    && mkdir built \
    && mkdir built/bin \
    && mkdir built/tests \
    && cp /src/proxy/*.so built/bin \
    && cp /src/tests/*.so built/tests \
    && ls -lh built/bin/*.so \
    && ls -lh built/tests/*.so

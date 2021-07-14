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
    && mkdir built/tmp \
    && mkdir built/tmp/bin \
    && mkdir built/tmp/tests \
    && cp /src/proxy/*.so built/tmp/bin \
    && cp /src/tests/*.so built/tmp/tests \
    && ls -lh built/tmp/bin/*.so \
    && ls -lh built/tmp/tests/*.so \
    && chmod -x built/tmp/bin/*.so

FROM gcc:latest AS gcc

COPY . /build

WORKDIR /build

RUN apt-get update -qq \
    && dpkg --add-architecture i386 \
    && apt-get install -y -qq cmake gcc-multilib g++-multilib \
    && chmod +x ./build.sh \
    && ./build.sh

COPY gcc:/build/src/ScriptEngine.NativeApi/build32/ScriptEngine.NativeApi32.so .
COPY gcc:/build/src/ScriptEngine.NativeApi/build64/ScriptEngine.NativeApi64.so .

FROM gcc:latest

COPY . /build

WORKDIR /build

RUN apt-get update -qq && \
    apt-get install -y -qq cmake libc6-dev libc6-dev-i386 gcc-multilib g++-multilib && \
    chmod +x ./build.sh && \
    ./build.sh

COPY --from=build /build/build32/ScriptEngine.NativeApi32.so .

COPY --from=build /build/build64/ScriptEngine.NativeApi64.so .

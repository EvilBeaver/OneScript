FROM ubuntu:18.04

RUN apt-get update -qq \
    && dpkg --add-architecture i386 \
    && apt-get install -y -qq build-essential cmake uuid-dev libc6-dev gcc-multilib g++-multilib
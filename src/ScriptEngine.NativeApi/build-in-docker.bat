docker build --no-cache -t oscript/onescript-builder:gcc  ^
    ..\..\install\builders\nativeapi

set SRC_PATH=%CD%
    
docker run --rm -v %SRC_PATH%:/build/src ^
    -it oscript/onescript-builder:gcc sh ./build.sh

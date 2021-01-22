docker build -t oscript/onescript-builder:gcc  ^
    ..\..\install\builders\nativeapi

set SRC_PATH=%CD%
set BUILD32_OUT=%SRC_PATH%\build32
set BUILD64_OUT=%SRC_PATH%\build64
    
docker run --rm -v %SRC_PATH%:/build/src -v %BUILD32_OUT%:/build/out  ^
    -it oscript/onescript-builder:gcc cmake ^
    -D CMAKE_BUILD_TYPE:STRING=Release -D TARGET_PLATFORM_32:BOOL=ON  ^
    --build ../src

docker run --rm -v %SRC_PATH%:/build/src -v %BUILD64_OUT%:/build/out  ^
    -it oscript/onescript-builder:gcc cmake ^
    -D CMAKE_BUILD_TYPE:STRING=Release -D TARGET_PLATFORM_32:BOOL=ON  ^
    --build ../src

#!/bin/sh

cmake -E make_directory build32
cd build32
cmake -D CMAKE_BUILD_TYPE:STRING=Release -D TARGET_PLATFORM_32:BOOL=ON --build ..
cmake --build .
cp *.so ..
cd ..

cmake -E make_directory build64
cd build64
cmake -D CMAKE_BUILD_TYPE:STRING=Release -D TARGET_PLATFORM_32:BOOL=OFF --build ..
cmake --build .
cp *.so ..
cd ..

ls *.so

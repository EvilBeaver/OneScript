cmake -E make_directory build32w
cd build32w
cmake .. -A Win32 -DMySuffix=32
cmake --build . --config Release
cd ..

cmake -E make_directory build64w
cd build64w
cmake .. -A x64 -DMySuffix=64
cmake --build . --config Release
cd ..
cmake -E make_directory build32w
cd build32w
cmake -A Win32 --build ..
cmake --config Release --build .
cd ..

cmake -E make_directory build64w
cd build64w
cmake -A x64 --build ..
cmake --config Release --build .
cd ..
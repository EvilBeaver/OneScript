cmake -E remove_directory bin
cmake -E remove_directory build32
cmake -E remove_directory build64

mkdir bin\
mkdir bin\Release\
mkdir bin\Release\net452\
mkdir bin\x86\Release\
mkdir bin\x86\Release\net452\

mkdir build64
cd build64
cmake .. -A x64
cmake --build . --config Release
cd ..

mkdir build32
cd build32
cmake .. -A Win32
cmake --build . --config Release
cd ..

copy build64\Release\*.dll bin\Release\net452\
copy build32\Release\*.dll bin\x86\Release\net452\

SET ROOT=%CD%

cmake -E remove_directory built\tmp\NativeApi
mkdir built
mkdir built\tmp
mkdir built\tmp\NativeApi\
cd built\tmp\NativeApi

mkdir build64
cd build64
cmake %ROOT%\src\ScriptEngine.NativeApi -A x64
cmake --build . --config Release
cd ..

mkdir build32
cd build32
cmake %ROOT%\src\ScriptEngine.NativeApi -A Win32
cmake --build . --config Release
cd ..

copy build64\Release\*.dll %ROOT%\built\tmp\bin\
copy build32\Release\*.dll %ROOT%\built\tmp\bin32\

cd %ROOT%

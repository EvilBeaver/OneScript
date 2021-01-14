cmake_minimum_required(VERSION 3.10)

project("ScriptEngine.NativeApi")
set(TARGET "ScriptEngine.NativeApi")

add_library(${TARGET} SHARED
	NativeApiProxy.cpp
	NativeInterface.cpp
	NativeInterface.h)

target_compile_definitions(${TARGET} PRIVATE UNICODE _UNICODE)
target_include_directories(${TARGET} PRIVATE include)

if (UNIX)
	if (TARGET_PLATFORM_32)
		set(MySuffix "32")
		set(CMAKE_LIBRARY_PATH "/usr/lib/i386-linux-gnu" CACHE PATH "<desc>")
		set(CMAKE_CXX_FLAGS "-m32 ${CMAKE_CXX_FLAGS}")
	else(UNIX)
		set(MySuffix "64")
		set(CMAKE_CXX_FLAGS "-m64 ${CMAKE_CXX_FLAGS}")
	endif ()
	set_target_properties(${TARGET} PROPERTIES PREFIX "")
	set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -s")
else(UNIX)
	if (NOT MSVC)
		message(FATAL_ERROR "Must be compiled with MSVC on Windows")
	endif(NOT MSVC)
	add_definitions(/MT)
	set(CMAKE_SUPPRESS_REGENERATION 1)
	set(CMAKE_CONFIGURATION_TYPES "Debug;Release" CACHE STRING "" FORCE)
	target_compile_definitions(${TARGET} PRIVATE _WINDOWS)
endif(UNIX)

set_target_properties(${TARGET} PROPERTIES
	OUTPUT_NAME ${PROJECT_NAME}${MySuffix}
	POSITION_INDEPENDENT_CODE ON 
	CXX_STANDARD_REQUIRED ON
	CXX_STANDARD 17)

set (LIBRARY_OUTPUT_PATH ${CMAKE_SOURCE_DIR}/bin)

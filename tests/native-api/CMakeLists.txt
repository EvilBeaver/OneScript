CMAKE_MINIMUM_REQUIRED(VERSION 3.0 FATAL_ERROR)

foreach(policy CMP0068)
  if(POLICY ${policy})
    cmake_policy(SET ${policy} OLD)
  endif()
endforeach()

PROJECT(AddInNative)

SET(AddInNative_SRC 
    AddInNative.cpp
    AddInNative.h
    dllmain.cpp
    stdafx.cpp
    stdafx.h
    )

include_directories(${CMAKE_SOURCE_DIR}/include)

SET (CMAKE_PREFIX_PATH ${CMAKE_PREFIX_PATH} ${CMAKE_SOURCE_DIR})
SET(AddInDef_SRC)

if (UNIX)
	if (TARGET_ARCH STREQUAL "x86")
		set(CMAKE_CXX_FLAGS "-m32 ${CMAKE_CXX_FLAGS}")
	else(UNIX)
		set(CMAKE_CXX_FLAGS "-m64 ${CMAKE_CXX_FLAGS}")
	endif ()
else()
    SET(AddInDef_SRC AddInNative.def)
    add_definitions(-DUNICODE -DWIN32 -D_USRDLL -D_CRT_SECURE_NO_WARNINGS -D_CRT_SECURE_NO_DEPRECATE -DADDINCPP_EXPORTS)
    IF(TARGET_PLATFORM_32)
        add_definitions(-D_USE_32BIT_TIME_T )
    ENDIF()
endif()

add_library(${PROJECT_NAME} SHARED ${AddInNative_SRC} ${AddInDef_SRC})

set_target_properties( ${PROJECT_NAME} PROPERTIES
       OUTPUT_NAME ${PROJECT_NAME}${MySuffix1}${MySuffix2} 
       POSITION_INDEPENDENT_CODE ON 
       CXX_STANDARD 11
       CXX_STANDARD_REQUIRED ON
   )

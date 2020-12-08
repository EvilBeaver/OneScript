// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//
#ifndef __STDAFX_H__
#define __STDAFX_H__

#ifdef _WINDOWS
    #include <windows.h>
#endif //_WINDOWS

#if defined(__linux__) || defined(__APPLE__)
	#define LINUX_OR_MACOS
#endif

#endif //__STDAFX_H__


#ifndef __COM_H__
#define __COM_H__

#if defined(__linux__) || defined(__APPLE__) || defined(__ANDROID__)

#ifdef __ANDROID__

typedef struct {
    unsigned int   Data1;
    unsigned short Data2;
    unsigned short Data3;
    unsigned char  Data4[ 8 ];
} uuid_t;

#else
#include <uuid/uuid.h>
#endif //__ANDROID__

#ifndef __ENVIRONMENT_IPHONE_OS_VERSION_MIN_REQUIRED__  // iOS
#include <dlfcn.h>
#endif //!__ENVIRONMENT_IPHONE_OS_VERSION_MIN_REQUIRED__

#pragma GCC system_header

typedef long HRESULT;

#ifdef __GNUC__
#define STDMETHODCALLTYPE   __attribute__ ((__stdcall__))
#define DECLSPEC_NOTHROW    __attribute__ ((nothrow))
#define STDMETHOD(method)   virtual DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE method
#else
#define STDMETHODCALLTYPE
#endif 

#define __stdcall        STDMETHODCALLTYPE
#define near
#define far
#define CONST    const
#define FAR    far

typedef unsigned long       DWORD;
#ifndef __ENVIRONMENT_IPHONE_OS_VERSION_MIN_REQUIRED__  // iOS
typedef int                 BOOL;
#elif defined(__LP64__)
typedef bool                BOOL;
#else
typedef signed char         BOOL;
#endif //!__ENVIRONMENT_IPHONE_OS_VERSION_MIN_REQUIRED__

typedef void                VOID;
typedef short               SHORT;
typedef unsigned char       BYTE;
typedef unsigned short      WORD;
typedef float               FLOAT;
typedef FLOAT               *PFLOAT;
typedef BOOL near           *PBOOL;
typedef BOOL far            *LPBOOL;
typedef BYTE near           *PBYTE;
typedef BYTE far            *LPBYTE;
typedef int near            *PINT;
typedef int far             *LPINT;
typedef WORD near           *PWORD;
typedef WORD far            *LPWORD;
typedef long far            *LPLONG;
typedef DWORD near          *PDWORD;
typedef DWORD far           *LPDWORD;
typedef void far            *LPVOID;
typedef CONST void far      *LPCVOID;
typedef wchar_t             *BSTR;
typedef long                SCODE;
typedef int                 INT;
typedef unsigned int        UINT;
typedef unsigned int        *PUINT;
typedef wchar_t             WCHAR;
typedef wchar_t             OLECHAR;
typedef wchar_t             *LPOLESTR;
typedef const wchar_t       *LPCOLESTR;
typedef DWORD               LCID;
typedef PDWORD              PLCID;
typedef long                LONG;
typedef unsigned long       ULONG;
typedef long long           LONGLONG;
typedef unsigned long long  ULONGLONG;
typedef LONG                DISPID;
typedef double              DOUBLE;
typedef double              DATE;
typedef short               VARIANT_BOOL;
typedef void                *PVOID;
typedef char                CHAR;
typedef CONST CHAR          *LPCSTR;
typedef unsigned short      USHORT;
typedef void                *HMODULE;
#define OLESTR(str) L##str

typedef uuid_t GUID;
typedef uuid_t IID;
typedef uuid_t UUID;
#define REFIID const IID &
#define MAX_PATH    260

#define IsEqualIID(x,y)    uuid_compare((x),(y))
#ifdef __GNUC__
#define LoadLibraryA(x) dlopen((x), RTLD_LAZY)
#define FreeLibrary(x) dlclose((x))
#define GetProcAddress(x, y) dlsym((x), (y))
#endif //__GNUC__

#define E_FAIL              0x80004005L
#define S_OK                0L
#define S_FALSE             1L
#define E_NOINTERFACE       0x80004002L
#define E_NOTIMPL           0x80004001L
#define E_INVALIDARG        0x80070057L
#define E_UNEXPECTED        0x8000FFFFL
#define E_OUTOFMEMORY       0x8007000EL
#define DISP_E_UNKNOWNNAME  0x80020006L
#define DISPID_UNKNOWN      ( -1 )
#define TRUE                1
#define FALSE               0

typedef long ITypeInfo;

#if defined (__GNUC__) && !defined (NONAMELESSUNION)
__extension__   /* no named members  */
#endif
union tCY {
    __extension__ struct 
    {
        unsigned long Lo;
        long      Hi;
    };
    long long int64;
};
typedef union tagCY CY;
#define CLSIDFromString(x,y) uuid_parse((x),(unsigned char*)(y))

#endif //defined(__linux__) || defined(__APPLE__)

#endif //__COM_H__

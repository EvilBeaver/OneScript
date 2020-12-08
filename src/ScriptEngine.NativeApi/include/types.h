
#ifndef __CON_TYPES_H__
#define __CON_TYPES_H__

#if defined(_WINDOWS) || defined(WINAPI_FAMILY)
#include <windows.h>
#endif

#if defined(WINAPI_FAMILY)
#include <wtypes.h>
#endif

#if __GNUC__ >=3
#pragma GCC system_header
#endif

#include "com.h"
#include <time.h>
#include <string.h>
#include <assert.h>
#include <stddef.h>

#define EXTERN_C extern "C"

#ifdef __GNUC__
#define _ANONYMOUS_UNION __extension__
#define _ANONYMOUS_STRUCT __extension__
#else
#define _ANONYMOUS_UNION 
#define _ANONYMOUS_STRUCT 
#endif //__GNUC__

#ifdef NONAMELESSUNION
#define __VARIANT_NAME_1 u
#define __VARIANT_NAME_2 iface
#define __VARIANT_NAME_3 str
#define __VARIANT_NAME_4 wstr
#else
#define __VARIANT_NAME_1
#define __VARIANT_NAME_2
#define __VARIANT_NAME_3
#define __VARIANT_NAME_4
#endif //NONAMELESSUNION

#define RESULT_FROM_ERRNO(x)     ((long)(x) <= 0 ? ((long)(x)) \
: ((long) (((x) & 0x0000FFFF) | (BASE_ERRNO << 16) | 0x80000000)))

#define ADDIN_E_NONE 1000
#define ADDIN_E_ORDINARY 1001
#define ADDIN_E_ATTENTION 1002
#define ADDIN_E_IMPORTANT 1003
#define ADDIN_E_VERY_IMPORTANT 1004
#define ADDIN_E_INFO 1005
#define ADDIN_E_FAIL 1006
#define ADDIN_E_MSGBOX_ATTENTION 1007
#define ADDIN_E_MSGBOX_INFO 1008
#define ADDIN_E_MSGBOX_FAIL 1009

#ifndef  ADDIN_API
#ifdef _WINDOWS
#define ADDIN_API __stdcall
#else
//#define ADDIN_API __attribute__ ((__stdcall__))
#define ADDIN_API
#endif //_WINDOWS
#endif //ADDIN_API

#include <stdint.h>

#ifdef _WINDOWS
#define WCHAR_T     wchar_t
#else
#define WCHAR_T     uint16_t
#endif //_WINDOWS
typedef unsigned short TYPEVAR;
enum ENUMVAR
{   
    VTYPE_EMPTY    = 0,
    VTYPE_NULL,
    VTYPE_I2,                   //int16_t
    VTYPE_I4,                   //int32_t
    VTYPE_R4,                   //float
    VTYPE_R8,                   //double
    VTYPE_DATE,                 //DATE (double)
    VTYPE_TM,                   //struct tm
    VTYPE_PSTR,                 //struct str    string
    VTYPE_INTERFACE,            //struct iface
    VTYPE_ERROR,                //int32_t errCode
    VTYPE_BOOL,                 //bool
    VTYPE_VARIANT,              //struct _tVariant *
    VTYPE_I1,                   //int8_t
    VTYPE_UI1,                  //uint8_t
    VTYPE_UI2,                  //uint16_t
    VTYPE_UI4,                  //uint32_t
    VTYPE_I8,                   //int64_t
    VTYPE_UI8,                  //uint64_t
    VTYPE_INT,                  //int   Depends on architecture
    VTYPE_UINT,                 //unsigned int  Depends on architecture
    VTYPE_HRESULT,              //long hRes
    VTYPE_PWSTR,                //struct wstr
    VTYPE_BLOB,                 //means in struct str binary data contain
    VTYPE_CLSID,                //UUID
    VTYPE_STR_BLOB    = 0xfff,
    VTYPE_VECTOR   = 0x1000,
    VTYPE_ARRAY    = 0x2000,
    VTYPE_BYREF    = 0x4000,    //Only with struct _tVariant *
    VTYPE_RESERVED = 0x8000,
    VTYPE_ILLEGAL  = 0xffff,
    VTYPE_ILLEGALMASKED    = 0xfff,
    VTYPE_TYPEMASK = 0xfff
} ;
#if defined (__GNUC__) && !defined (NONAMELESSUNION)
__extension__   /* no named members  */
#endif
struct _tVariant
{
    _ANONYMOUS_UNION union 
    {
        int8_t         i8Val;
        int16_t        shortVal;
        int32_t        lVal;
        int            intVal;
        unsigned int   uintVal;
        int64_t        llVal;
        uint8_t        ui8Val;
        uint16_t       ushortVal;
        uint32_t       ulVal;
        uint64_t       ullVal;
        int32_t        errCode;
        long           hRes;
        float          fltVal;
        double         dblVal;
        bool           bVal;
        char           chVal;
        wchar_t        wchVal;
        DATE           date;
        IID            IDVal;
        struct _tVariant *pvarVal;
        struct tm      tmVal;
        _ANONYMOUS_STRUCT struct 
        {
            void*  pInterfaceVal;
            IID        InterfaceID;
        } __VARIANT_NAME_2/*iface*/;
        _ANONYMOUS_STRUCT struct 
        {
            char*        pstrVal;
            uint32_t     strLen; //count of bytes
        } __VARIANT_NAME_3/*str*/;
        _ANONYMOUS_STRUCT struct 
        {
            WCHAR_T*    pwstrVal;
            uint32_t    wstrLen; //count of symbol
        } __VARIANT_NAME_4/*wstr*/;
    } __VARIANT_NAME_1;
    uint32_t      cbElements;    //Dimension for an one-dimensional array in pvarVal
    TYPEVAR       vt;
};
typedef struct _tVariant    tVariant;
typedef tVariant    tVariantArg;


#if defined(NONAMELESSUNION)
#define TV_JOIN(X, Y)   ((X)->u.Y)
#else
#define TV_JOIN(X, Y)   ((X)->Y)
#endif

#define TV_VT(X)          ((X)->vt)
#define TV_ISBYREF(X)     (TV_VT(X)&VT_BYREF)
#define TV_ISARRAY(X)     (TV_VT(X)&VT_ARRAY)
#define TV_ISVECTOR(X)    (TV_VT(X)&VT_VECTOR)
#define TV_NONE(X)        TV_I2(X)

#define TV_UI1(X)         TV_JOIN(X, ui8Val)
#define TV_I2(X)          TV_JOIN(X, shortVal)
#define TV_I4(X)          TV_JOIN(X, lVal)
#define TV_I8(X)          TV_JOIN(X, llVal)
#define TV_R4(X)          TV_JOIN(X, fltVal)
#define TV_R8(X)          TV_JOIN(X, dblVal)
#define TV_I1(X)          TV_JOIN(X, i8Val)
#define TV_UI2(X)         TV_JOIN(X, ushortVal)
#define TV_UI4(X)         TV_JOIN(X, ulVal)
#define TV_UI8(X)         TV_JOIN(X, ullVal)
#define TV_INT(X)         TV_JOIN(X, intVal)
#define TV_UINT(X)        TV_JOIN(X, uintVal)

#ifdef _WIN64
#define TV_INT_PTR(X)        TV_JOIN(X, llVal)
#define TV_UINT_PTR(X)       TV_JOIN(X, ullVal)
#else
#define TV_INT_PTR(X)        TV_JOIN(X, lVal)
#define TV_UINT_PTR(X)       TV_JOIN(X, ulVal)
#endif


#define TV_DATE(X)        TV_JOIN(X, date)
#define TV_STR(X)         TV_JOIN(X, pstrVal)
#define TV_WSTR(X)        TV_JOIN(X, pwstrVal)
#define TV_BOOL(X)        TV_JOIN(X, bVal)
#define TV_UNKNOWN(X)     TV_JOIN(X, pInterfaceVal)
#define TV_VARIANTREF(X)  TV_JOIN(X, pvarVal)

void tVarInit(tVariant* tvar);

inline
void tVarInit(tVariant* tvar)
{
    assert(tvar != NULL);
    memset(tvar, 0, sizeof(tVariant));
    TV_VT(tvar) = VTYPE_EMPTY;
}
//----------------------------------------------------------------------------//
// static setter functions...

#define DATA_SET_BEGIN(data_)                                         \
    tVarInit(data_);

#define DATA_SET_END(data_, type_)                                    \
    TV_VT(data_) = type_;
    

#define DATA_SET(data_, type_, member_, value_)                       \
    DATA_SET_BEGIN(data_)                                             \
    TV_JOIN(data_, member_) = value_;                                 \
    DATA_SET_END(data_, type_)

#define DATA_SET_WITH_CAST(data_, type_, member_, cast_, value_)      \
    DATA_SET_BEGIN(data_)                                             \
    TV_JOIN(data_, member_) = cast_ value_;                           \
    DATA_SET_END(data_, type_)

#endif //__CON_TYPES_H__

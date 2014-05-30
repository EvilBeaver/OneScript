

/* this ALWAYS GENERATED file contains the IIDs and CLSIDs */

/* link this file in with the server and any clients */


 /* File created by MIDL compiler version 8.00.0595 */
/* at Sat May 31 00:05:35 2014
 */
/* Compiler settings for Snegopat.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.00.0595 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


#ifdef __cplusplus
extern "C"{
#endif 


#include <rpc.h>
#include <rpcndr.h>

#ifdef _MIDL_USE_GUIDDEF_

#ifndef INITGUID
#define INITGUID
#include <guiddef.h>
#undef INITGUID
#else
#include <guiddef.h>
#endif

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        DEFINE_GUID(name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8)

#else // !_MIDL_USE_GUIDDEF_

#ifndef __IID_DEFINED__
#define __IID_DEFINED__

typedef struct _IID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
typedef IID CLSID;
#endif // CLSID_DEFINED

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}

#endif !_MIDL_USE_GUIDDEF_

MIDL_DEFINE_GUID(IID, IID_IAddinLoader,0x2BEEF9E6,0xAF34,0x4593,0x9E,0x73,0x3D,0x07,0xEA,0xA4,0xCF,0x0D);


MIDL_DEFINE_GUID(IID, IID_IAddinGroup,0x80864878,0x1618,0x46BA,0xB7,0x21,0xF8,0xF1,0xEA,0x16,0x26,0x09);


MIDL_DEFINE_GUID(IID, IID_IAddin,0x74D4C89D,0xCFB1,0x4074,0xA4,0x1E,0x49,0xC7,0xA0,0x3E,0xD8,0x62);


MIDL_DEFINE_GUID(IID, LIBID_OneScriptSnegopat,0x64CE2CE5,0xA523,0x40A4,0x85,0x27,0x82,0x47,0x15,0xAF,0xE9,0x29);

#undef MIDL_DEFINE_GUID

#ifdef __cplusplus
}
#endif






/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0595 */
/* at Wed Jul 09 18:07:33 2014
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


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __Snegopat_h_h__
#define __Snegopat_h_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IAddinLoader_FWD_DEFINED__
#define __IAddinLoader_FWD_DEFINED__
typedef interface IAddinLoader IAddinLoader;

#endif 	/* __IAddinLoader_FWD_DEFINED__ */


#ifndef __IAddinMacroses_FWD_DEFINED__
#define __IAddinMacroses_FWD_DEFINED__
typedef interface IAddinMacroses IAddinMacroses;

#endif 	/* __IAddinMacroses_FWD_DEFINED__ */


#ifndef __IParamsWrapper_FWD_DEFINED__
#define __IParamsWrapper_FWD_DEFINED__
typedef interface IParamsWrapper IParamsWrapper;

#endif 	/* __IParamsWrapper_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


/* interface __MIDL_itf_Snegopat_0000_0000 */
/* [local] */ 




extern RPC_IF_HANDLE __MIDL_itf_Snegopat_0000_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_Snegopat_0000_0000_v0_0_s_ifspec;

#ifndef __IAddinLoader_INTERFACE_DEFINED__
#define __IAddinLoader_INTERFACE_DEFINED__

/* interface IAddinLoader */
/* [object][nonextensible][helpstring][version][uuid] */ 


EXTERN_C const IID IID_IAddinLoader;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("2BEEF9E6-AF34-4593-9E73-3D07EAA4CF0D")
    IAddinLoader : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT __stdcall proto( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall load( 
            /* [in] */ BSTR uri,
            /* [out] */ BSTR *fullPath,
            /* [out] */ BSTR *uniqueName,
            /* [out] */ BSTR *displayName,
            /* [retval][out] */ IUnknown **result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall canUnload( 
            /* [in] */ BSTR fullPath,
            /* [in] */ IUnknown *addin,
            /* [retval][out] */ VARIANT_BOOL *result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall unload( 
            /* [in] */ BSTR fullPath,
            /* [in] */ IUnknown *addin,
            /* [retval][out] */ VARIANT_BOOL *result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall loadCommandName( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall selectLoadURI( 
            /* [retval][out] */ BSTR *result) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IAddinLoaderVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IAddinLoader * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IAddinLoader * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IAddinLoader * This);
        
        /* [helpstring] */ HRESULT ( __stdcall *proto )( 
            IAddinLoader * This,
            /* [retval][out] */ BSTR *result);
        
        /* [helpstring] */ HRESULT ( __stdcall *load )( 
            IAddinLoader * This,
            /* [in] */ BSTR uri,
            /* [out] */ BSTR *fullPath,
            /* [out] */ BSTR *uniqueName,
            /* [out] */ BSTR *displayName,
            /* [retval][out] */ IUnknown **result);
        
        /* [helpstring] */ HRESULT ( __stdcall *canUnload )( 
            IAddinLoader * This,
            /* [in] */ BSTR fullPath,
            /* [in] */ IUnknown *addin,
            /* [retval][out] */ VARIANT_BOOL *result);
        
        /* [helpstring] */ HRESULT ( __stdcall *unload )( 
            IAddinLoader * This,
            /* [in] */ BSTR fullPath,
            /* [in] */ IUnknown *addin,
            /* [retval][out] */ VARIANT_BOOL *result);
        
        /* [helpstring] */ HRESULT ( __stdcall *loadCommandName )( 
            IAddinLoader * This,
            /* [retval][out] */ BSTR *result);
        
        /* [helpstring] */ HRESULT ( __stdcall *selectLoadURI )( 
            IAddinLoader * This,
            /* [retval][out] */ BSTR *result);
        
        END_INTERFACE
    } IAddinLoaderVtbl;

    interface IAddinLoader
    {
        CONST_VTBL struct IAddinLoaderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IAddinLoader_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IAddinLoader_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IAddinLoader_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IAddinLoader_proto(This,result)	\
    ( (This)->lpVtbl -> proto(This,result) ) 

#define IAddinLoader_load(This,uri,fullPath,uniqueName,displayName,result)	\
    ( (This)->lpVtbl -> load(This,uri,fullPath,uniqueName,displayName,result) ) 

#define IAddinLoader_canUnload(This,fullPath,addin,result)	\
    ( (This)->lpVtbl -> canUnload(This,fullPath,addin,result) ) 

#define IAddinLoader_unload(This,fullPath,addin,result)	\
    ( (This)->lpVtbl -> unload(This,fullPath,addin,result) ) 

#define IAddinLoader_loadCommandName(This,result)	\
    ( (This)->lpVtbl -> loadCommandName(This,result) ) 

#define IAddinLoader_selectLoadURI(This,result)	\
    ( (This)->lpVtbl -> selectLoadURI(This,result) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IAddinLoader_INTERFACE_DEFINED__ */


#ifndef __IAddinMacroses_INTERFACE_DEFINED__
#define __IAddinMacroses_INTERFACE_DEFINED__

/* interface IAddinMacroses */
/* [object][nonextensible][helpstring][version][uuid] */ 


EXTERN_C const IID IID_IAddinMacroses;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("0CCEA8E6-D187-4B84-B74D-85079F8E2008")
    IAddinMacroses : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT __stdcall macroses( 
            /* [retval][out] */ SAFEARRAY * *result) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall invokeMacros( 
            /* [in] */ BSTR MacrosName,
            /* [retval][out] */ VARIANT *result) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IAddinMacrosesVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IAddinMacroses * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IAddinMacroses * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IAddinMacroses * This);
        
        /* [helpstring] */ HRESULT ( __stdcall *macroses )( 
            IAddinMacroses * This,
            /* [retval][out] */ SAFEARRAY * *result);
        
        /* [helpstring] */ HRESULT ( __stdcall *invokeMacros )( 
            IAddinMacroses * This,
            /* [in] */ BSTR MacrosName,
            /* [retval][out] */ VARIANT *result);
        
        END_INTERFACE
    } IAddinMacrosesVtbl;

    interface IAddinMacroses
    {
        CONST_VTBL struct IAddinMacrosesVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IAddinMacroses_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IAddinMacroses_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IAddinMacroses_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IAddinMacroses_macroses(This,result)	\
    ( (This)->lpVtbl -> macroses(This,result) ) 

#define IAddinMacroses_invokeMacros(This,MacrosName,result)	\
    ( (This)->lpVtbl -> invokeMacros(This,MacrosName,result) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IAddinMacroses_INTERFACE_DEFINED__ */


#ifndef __IParamsWrapper_INTERFACE_DEFINED__
#define __IParamsWrapper_INTERFACE_DEFINED__

/* interface IParamsWrapper */
/* [object][oleautomation][nonextensible][dual][helpstring][version][uuid] */ 


EXTERN_C const IID IID_IParamsWrapper;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("F80C31B9-1D9F-4438-BEF3-10D829664EFD")
    IParamsWrapper : public IDispatch
    {
    public:
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_val( 
            /* [retval][out] */ VARIANT *pVal) = 0;
        
        virtual /* [helpstring][propput][id] */ HRESULT STDMETHODCALLTYPE put_val( 
            /* [in] */ VARIANT pVal) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IParamsWrapperVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IParamsWrapper * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IParamsWrapper * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IParamsWrapper * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IParamsWrapper * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IParamsWrapper * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IParamsWrapper * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IParamsWrapper * This,
            /* [annotation][in] */ 
            _In_  DISPID dispIdMember,
            /* [annotation][in] */ 
            _In_  REFIID riid,
            /* [annotation][in] */ 
            _In_  LCID lcid,
            /* [annotation][in] */ 
            _In_  WORD wFlags,
            /* [annotation][out][in] */ 
            _In_  DISPPARAMS *pDispParams,
            /* [annotation][out] */ 
            _Out_opt_  VARIANT *pVarResult,
            /* [annotation][out] */ 
            _Out_opt_  EXCEPINFO *pExcepInfo,
            /* [annotation][out] */ 
            _Out_opt_  UINT *puArgErr);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_val )( 
            IParamsWrapper * This,
            /* [retval][out] */ VARIANT *pVal);
        
        /* [helpstring][propput][id] */ HRESULT ( STDMETHODCALLTYPE *put_val )( 
            IParamsWrapper * This,
            /* [in] */ VARIANT pVal);
        
        END_INTERFACE
    } IParamsWrapperVtbl;

    interface IParamsWrapper
    {
        CONST_VTBL struct IParamsWrapperVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IParamsWrapper_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IParamsWrapper_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IParamsWrapper_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IParamsWrapper_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IParamsWrapper_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IParamsWrapper_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IParamsWrapper_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IParamsWrapper_get_val(This,pVal)	\
    ( (This)->lpVtbl -> get_val(This,pVal) ) 

#define IParamsWrapper_put_val(This,pVal)	\
    ( (This)->lpVtbl -> put_val(This,pVal) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IParamsWrapper_INTERFACE_DEFINED__ */



#ifndef __OneScriptSnegopat_LIBRARY_DEFINED__
#define __OneScriptSnegopat_LIBRARY_DEFINED__

/* library OneScriptSnegopat */
/* [helpstring][uuid] */ 




EXTERN_C const IID LIBID_OneScriptSnegopat;
#endif /* __OneScriptSnegopat_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

unsigned long             __RPC_USER  LPSAFEARRAY_UserSize(     unsigned long *, unsigned long            , LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserMarshal(  unsigned long *, unsigned char *, LPSAFEARRAY * ); 
unsigned char * __RPC_USER  LPSAFEARRAY_UserUnmarshal(unsigned long *, unsigned char *, LPSAFEARRAY * ); 
void                      __RPC_USER  LPSAFEARRAY_UserFree(     unsigned long *, LPSAFEARRAY * ); 

unsigned long             __RPC_USER  VARIANT_UserSize(     unsigned long *, unsigned long            , VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserMarshal(  unsigned long *, unsigned char *, VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserUnmarshal(unsigned long *, unsigned char *, VARIANT * ); 
void                      __RPC_USER  VARIANT_UserFree(     unsigned long *, VARIANT * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif





/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0595 */
/* at Fri May 30 16:14:16 2014
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


#ifndef __IAddinGroup_FWD_DEFINED__
#define __IAddinGroup_FWD_DEFINED__
typedef interface IAddinGroup IAddinGroup;

#endif 	/* __IAddinGroup_FWD_DEFINED__ */


#ifndef __IAddin_FWD_DEFINED__
#define __IAddin_FWD_DEFINED__
typedef interface IAddin IAddin;

#endif 	/* __IAddin_FWD_DEFINED__ */


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


#ifndef __IAddinGroup_INTERFACE_DEFINED__
#define __IAddinGroup_INTERFACE_DEFINED__

/* interface IAddinGroup */
/* [object][oleautomation][nonextensible][dual][helpstring][version][uuid] */ 


EXTERN_C const IID IID_IAddinGroup;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("80864878-1618-46BA-B721-F8F1EA162609")
    IAddinGroup : public IDispatch
    {
    public:
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_name( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_parent( 
            /* [retval][out] */ IAddinGroup **pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_child( 
            /* [retval][out] */ IAddinGroup **pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_next( 
            /* [retval][out] */ IAddinGroup **pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_addinsCount( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE addin( 
            /* [in] */ unsigned long Idx,
            /* [retval][out] */ IAddin **pVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE addGroup( 
            /* [in] */ BSTR name,
            /* [retval][out] */ IAddinGroup **ppResult) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IAddinGroupVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IAddinGroup * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IAddinGroup * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IAddinGroup * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IAddinGroup * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IAddinGroup * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IAddinGroup * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IAddinGroup * This,
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
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_name )( 
            IAddinGroup * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_parent )( 
            IAddinGroup * This,
            /* [retval][out] */ IAddinGroup **pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_child )( 
            IAddinGroup * This,
            /* [retval][out] */ IAddinGroup **pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_next )( 
            IAddinGroup * This,
            /* [retval][out] */ IAddinGroup **pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_addinsCount )( 
            IAddinGroup * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *addin )( 
            IAddinGroup * This,
            /* [in] */ unsigned long Idx,
            /* [retval][out] */ IAddin **pVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *addGroup )( 
            IAddinGroup * This,
            /* [in] */ BSTR name,
            /* [retval][out] */ IAddinGroup **ppResult);
        
        END_INTERFACE
    } IAddinGroupVtbl;

    interface IAddinGroup
    {
        CONST_VTBL struct IAddinGroupVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IAddinGroup_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IAddinGroup_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IAddinGroup_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IAddinGroup_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IAddinGroup_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IAddinGroup_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IAddinGroup_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IAddinGroup_get_name(This,pVal)	\
    ( (This)->lpVtbl -> get_name(This,pVal) ) 

#define IAddinGroup_get_parent(This,pVal)	\
    ( (This)->lpVtbl -> get_parent(This,pVal) ) 

#define IAddinGroup_get_child(This,pVal)	\
    ( (This)->lpVtbl -> get_child(This,pVal) ) 

#define IAddinGroup_get_next(This,pVal)	\
    ( (This)->lpVtbl -> get_next(This,pVal) ) 

#define IAddinGroup_get_addinsCount(This,pVal)	\
    ( (This)->lpVtbl -> get_addinsCount(This,pVal) ) 

#define IAddinGroup_addin(This,Idx,pVal)	\
    ( (This)->lpVtbl -> addin(This,Idx,pVal) ) 

#define IAddinGroup_addGroup(This,name,ppResult)	\
    ( (This)->lpVtbl -> addGroup(This,name,ppResult) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IAddinGroup_INTERFACE_DEFINED__ */


#ifndef __IAddin_INTERFACE_DEFINED__
#define __IAddin_INTERFACE_DEFINED__

/* interface IAddin */
/* [object][oleautomation][nonextensible][dual][helpstring][version][uuid] */ 


EXTERN_C const IID IID_IAddin;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("74D4C89D-CFB1-4074-A41E-49C7A03ED862")
    IAddin : public IDispatch
    {
    public:
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_displayName( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_uniqueName( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_fullPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_object( 
            /* [retval][out] */ IDispatch **pVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE macroses( 
            /* [retval][out] */ VARIANT *pVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE invokeMacros( 
            /* [in] */ BSTR MacrosName,
            /* [retval][out] */ VARIANT *result) = 0;
        
        virtual /* [helpstring][propget][id] */ HRESULT STDMETHODCALLTYPE get_group( 
            /* [retval][out] */ IAddinGroup **pVal) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IAddinVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IAddin * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IAddin * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IAddin * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IAddin * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IAddin * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IAddin * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IAddin * This,
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
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_displayName )( 
            IAddin * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_uniqueName )( 
            IAddin * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_fullPath )( 
            IAddin * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_object )( 
            IAddin * This,
            /* [retval][out] */ IDispatch **pVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *macroses )( 
            IAddin * This,
            /* [retval][out] */ VARIANT *pVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *invokeMacros )( 
            IAddin * This,
            /* [in] */ BSTR MacrosName,
            /* [retval][out] */ VARIANT *result);
        
        /* [helpstring][propget][id] */ HRESULT ( STDMETHODCALLTYPE *get_group )( 
            IAddin * This,
            /* [retval][out] */ IAddinGroup **pVal);
        
        END_INTERFACE
    } IAddinVtbl;

    interface IAddin
    {
        CONST_VTBL struct IAddinVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IAddin_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IAddin_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IAddin_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IAddin_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IAddin_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IAddin_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IAddin_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IAddin_get_displayName(This,pVal)	\
    ( (This)->lpVtbl -> get_displayName(This,pVal) ) 

#define IAddin_get_uniqueName(This,pVal)	\
    ( (This)->lpVtbl -> get_uniqueName(This,pVal) ) 

#define IAddin_get_fullPath(This,pVal)	\
    ( (This)->lpVtbl -> get_fullPath(This,pVal) ) 

#define IAddin_get_object(This,pVal)	\
    ( (This)->lpVtbl -> get_object(This,pVal) ) 

#define IAddin_macroses(This,pVal)	\
    ( (This)->lpVtbl -> macroses(This,pVal) ) 

#define IAddin_invokeMacros(This,MacrosName,result)	\
    ( (This)->lpVtbl -> invokeMacros(This,MacrosName,result) ) 

#define IAddin_get_group(This,pVal)	\
    ( (This)->lpVtbl -> get_group(This,pVal) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IAddin_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

unsigned long             __RPC_USER  VARIANT_UserSize(     unsigned long *, unsigned long            , VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserMarshal(  unsigned long *, unsigned char *, VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserUnmarshal(unsigned long *, unsigned char *, VARIANT * ); 
void                      __RPC_USER  VARIANT_UserFree(     unsigned long *, VARIANT * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif



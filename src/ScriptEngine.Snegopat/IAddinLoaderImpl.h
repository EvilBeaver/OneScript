#pragma once

#include "Stdafx.h"
#include "Snegopat_h.h"

class IAddinLoaderImpl :
	public IAddinLoader
{
private:
	ULONG m_refCount;
	IDispatch* m_pDesigner;

public:
	IAddinLoaderImpl(IDispatch* pDesigner);

	//IUnknown interface 
    virtual HRESULT  __stdcall QueryInterface(
                                REFIID riid, 
                                void **ppObj);
    virtual ULONG   __stdcall AddRef();
    virtual ULONG   __stdcall Release();

	virtual HRESULT __stdcall proto( 
            BSTR *result);
        
    virtual HRESULT __stdcall load( 
        BSTR uri,
        BSTR *fullPath,
        BSTR *uniqueName,
        BSTR *displayName,
        IUnknown **result);
        
    virtual HRESULT __stdcall canUnload( 
        BSTR fullPath,
        IUnknown *addin,
        VARIANT_BOOL *result);
        
    virtual HRESULT __stdcall unload( 
        BSTR fullPath,
        IUnknown *addin,
        VARIANT_BOOL *result);
        
    virtual HRESULT __stdcall loadCommandName( 
        BSTR *result);
        
    virtual HRESULT __stdcall selectLoadURI( 
        BSTR *result);

	virtual ~IAddinLoaderImpl(void);
};


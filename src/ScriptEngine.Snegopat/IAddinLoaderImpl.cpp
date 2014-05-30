#include "stdafx.h"
#include "IAddinLoaderImpl.h"
#include "Snegopat_i.c"

//extern const IID IID_IAddinLoader;
//extern const IID IID_IAddinGroup;

IAddinLoaderImpl::IAddinLoaderImpl(IDispatch* pDesigner)
{
	m_refCount = 0;
	m_pDesigner = pDesigner;
}


IAddinLoaderImpl::~IAddinLoaderImpl(void)
{
	
}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinLoaderImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IUnknown*>(this); 
		AddRef() ;
		return S_OK;
	}
	if (riid == IID_IAddinLoader)
	{
		*ppObj = static_cast<IUnknown*>(this);
		AddRef() ;
		return S_OK;
	}
	//
	//if control reaches here then , let the client know that
	//we do not satisfy the required interface
	//
	*ppObj = NULL ;
	return E_NOINTERFACE ;
}

ULONG   __stdcall IAddinLoaderImpl::AddRef()
{
	return InterlockedIncrement(&m_refCount) ;
}

ULONG   __stdcall IAddinLoaderImpl::Release()
{
	long nRefCount = 0;
	nRefCount = InterlockedDecrement(&m_refCount) ;
	if (nRefCount == 0)
	{
		m_pDesigner->Release();
		delete this;
	}

	return nRefCount;
}

#pragma endregion

HRESULT __stdcall  IAddinLoaderImpl::proto( 
            BSTR *result)
{
	*result = SysAllocString(L"1s");
	return S_OK;
}
        
HRESULT __stdcall  IAddinLoaderImpl::load( 
    BSTR uri,
    BSTR *fullPath,
    BSTR *uniqueName,
    BSTR *displayName,
    IUnknown **result)
{
	return E_NOTIMPL;
}
        
HRESULT __stdcall  IAddinLoaderImpl::canUnload( 
    BSTR fullPath,
    IUnknown *addin,
    VARIANT_BOOL *result)
{
	return E_NOTIMPL;
}
        
HRESULT __stdcall  IAddinLoaderImpl::unload( 
    BSTR fullPath,
    IUnknown *addin,
    VARIANT_BOOL *result)
{
	return E_NOTIMPL;
}
        
HRESULT __stdcall  IAddinLoaderImpl::loadCommandName( 
    BSTR *result)
{
	return E_NOTIMPL;
}
        
HRESULT __stdcall  IAddinLoaderImpl::selectLoadURI( 
    BSTR *result)
{
	return E_NOTIMPL;
}

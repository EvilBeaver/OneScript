#include "stdafx.h"
#include "IAddinImpl.h"
#include <OleAuto.h>

ITypeInfo* IAddinImpl::m_typeInfo = NULL;

void IAddinImpl::CreateTypeInfo(ITypeLib* lib)
{
	lib->GetTypeInfoOfGuid(IID_IAddin, &m_typeInfo);
}

IAddinImpl::IAddinImpl(ScriptEngine::Machine::IRuntimeContextInstance^ innerObject) : RefCountable()
{
	m_innerObject = innerObject;
}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if(riid == IID_IAddin)
	{
		*ppObj = static_cast<IAddinImpl*>(this);
		AddRef();
		return S_OK;
	}
	else if(riid == IID_IDispatch)
	{
		*ppObj = static_cast<IDispatch*>(this);
		AddRef();
		return S_OK;
	}
	else
	{
		return IUnknownQueried(riid, ppObj);
	}
}

ULONG   __stdcall IAddinImpl::AddRef()
{
	return RefCountable::AddRef();
}

ULONG   __stdcall IAddinImpl::Release()
{
	return RefCountable::Release();
}

#pragma endregion

#pragma region IDispatch impl

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeInfoCount( 
	UINT *pctinfo)
{
	*pctinfo = 1;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeInfo( 
	UINT iTInfo,
	LCID lcid,
	ITypeInfo **ppTInfo)
{
	*ppTInfo = m_typeInfo;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetIDsOfNames( 
	REFIID riid,
	LPOLESTR *rgszNames,
	UINT cNames,
	LCID lcid,
	DISPID *rgDispId)
{
	return DispGetIDsOfNames(m_typeInfo, rgszNames, cNames, rgDispId);
}

HRESULT STDMETHODCALLTYPE IAddinImpl::Invoke( 
	DISPID dispIdMember,
	REFIID riid,
	LCID lcid,
	WORD wFlags,
	DISPPARAMS *pDispParams,
	VARIANT *pVarResult,
	EXCEPINFO *pExcepInfo,
	UINT *puArgErr)
{
	return DispInvoke(this, m_typeInfo, dispIdMember, wFlags, pDispParams, pVarResult, pExcepInfo, puArgErr);
}

#pragma endregion


IAddinImpl::~IAddinImpl(void)
{
}

HRESULT STDMETHODCALLTYPE IAddinImpl::get_displayName( 
            BSTR *pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_uniqueName( 
    BSTR *pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_fullPath( 
    BSTR *pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_object( 
    IDispatch **pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::macroses( 
    VARIANT *pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::invokeMacros( 
    BSTR MacrosName,
    VARIANT *result)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_group( 
    IAddinGroup **pVal)
{
	return E_NOTIMPL;
}

void IAddinImpl::OnZeroCount()
{
	m_innerObject = nullptr;
}
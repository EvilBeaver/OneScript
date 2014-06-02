#include "stdafx.h"
#include "IAddinImpl.h"
#include "MarshalingHelpers.h"
#include <OleAuto.h>

ITypeInfo* IAddinImpl::m_typeInfo = NULL;

void IAddinImpl::CreateTypeInfo(ITypeLib* lib)
{
	lib->GetTypeInfoOfGuid(IID_IAddin, &m_typeInfo);
}

IAddinImpl::IAddinImpl(ScriptEngine::Machine::Contexts::UserScriptContextInstance^ innerObject) : RefCountable()
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
	else if(riid == IID_IAddinInit)
	{
		*ppObj = static_cast<IAddinInit*>(this);
		AddRef();
		return S_OK;
	}
	else if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IAddin*>(this);
		AddRef();
		return S_OK;
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
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

HRESULT _stdcall IAddinImpl::initAddin(IDispatch* designer)
{
	return S_OK;
}

IAddinImpl::~IAddinImpl(void)
{
}

HRESULT STDMETHODCALLTYPE IAddinImpl::get_displayName( 
            BSTR *pVal)
{
	*pVal = m_displayName;
	return S_OK;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_uniqueName( 
    BSTR *pVal)
{
	*pVal = m_uniqueName;
	return S_OK;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_fullPath( 
    BSTR *pVal)
{
	*pVal = m_fullPath;
	return S_OK;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::get_object( 
    IDispatch **pVal)
{
	return E_NOTIMPL;
}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::macroses( 
    VARIANT *pVal)
{
	array<System::String^>^ macrosArray = m_innerObject->GetExportedMethods();
	
	SAFEARRAYBOUND  Bound;
    Bound.lLbound   = 0;
	Bound.cElements = macrosArray->Length;
	SAFEARRAY* saData = SafeArrayCreate(VT_R8, 1, &Bound);

	V_VT(pVal) = VT_ARRAY;
	V_ARRAY(pVal) = saData;

	return S_OK;

}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::invokeMacros( 
    BSTR MacrosName,
    VARIANT *result)
{
	System::String^ strMacro = gcnew System::String(MacrosName);

	try
	{
		int mNum = m_innerObject->FindMethod(strMacro);
		auto mInfo = m_innerObject->GetMethodInfo(mNum);
		// пока без возвратов
		m_innerObject->CallAsProcedure(mNum, gcnew array<ScriptEngine::Machine::IValue^, 1>(0));
	}
	catch(System::Exception^ e)
	{
		auto buf = stringBuf(e->Message);
		MessageBox(0, buf, L"Error", MB_OK);
		delete[] buf;
	}

	return S_OK;

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
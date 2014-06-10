#include "stdafx.h"
#include "IAddinImpl.h"
#include "MarshalingHelpers.h"
#include <OleAuto.h>

IAddinImpl::IAddinImpl(ScriptEngine::Machine::Contexts::UserScriptContextInstance^ innerObject) : RefCountable()
{
	m_innerObject = innerObject;
}

IAddinImpl::~IAddinImpl(void)
{
	m_innerObject = nullptr;
}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IAddinMacroses*>(this);
		AddRef();
		return S_OK;
	}
	if (riid == IID_IAddinMacroses)
	{
		*ppObj = static_cast<IAddinMacroses*>(this);
		AddRef();
		return S_OK;
	}
	if (riid == IID_IDispatch)
	{
		*ppObj = static_cast<IDispatch*>(this);
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
	//*ppTInfo = m_typeInfo;
	return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetIDsOfNames( 
	REFIID riid,
	LPOLESTR *rgszNames,
	UINT cNames,
	LCID lcid,
	DISPID *rgDispId)
{
	return E_NOTIMPL;
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
	return E_NOTIMPL;
}

#pragma endregion
        
HRESULT STDMETHODCALLTYPE IAddinImpl::macroses(SAFEARRAY **result)
{
	array<System::String^>^ macrosArray = m_innerObject->GetExportedMethods();
	
	SAFEARRAYBOUND  Bound[1];
    Bound[0].lLbound   = 0;
	Bound[0].cElements = macrosArray->Length;
	*result = SafeArrayCreate(VT_VARIANT, 1, Bound);
	LONG idx[1];
	for (int i = 0; i < macrosArray->Length; i++)
	{
		WCHAR* buf = stringBuf(macrosArray[i]);
		BSTR allocString = SysAllocString(buf);
		delete[] buf;
		
		VARIANT val;
		V_VT(&val) = VT_BSTR;
		V_BSTR(&val) = allocString;

		idx[0] = i;
		HRESULT hr = SafeArrayPutElement(*result, idx, &val);
	}

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
		auto buf = stringBuf(e->ToString());
		MessageBox(0, buf, L"Error", MB_OK);
		delete[] buf;
	}

	return S_OK;

}

void IAddinImpl::OnZeroCount()
{
	m_innerObject = nullptr;
}

#pragma region ITypeInfo members

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeAttr(TYPEATTR **ppTypeAttr)
{
	TYPEATTR* ta = new TYPEATTR();
	memset(ta, 0, sizeof(TYPEATTR));

	array<System::String^>^ exportNames = m_innerObject->GetExportedMethods();
	m_exportedMeths = gcnew array<ScriptEngine::Machine::MethodInfo>(exportNames->Length);
	
	for (int i = 0; i < m_exportedMeths->Length; i++)
	{
		int mId = m_innerObject->FindMethod(exportNames[i]);
		m_exportedMeths[i] = m_innerObject->GetMethodInfo(mId);
	}

	ta->cFuncs = m_exportedMeths->Length;
	ta->typekind = TKIND_DISPATCH;
	*ppTypeAttr = ta;

	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetFuncDesc( 
        UINT index,
        FUNCDESC **ppFuncDesc)
{
	
	auto mi = m_exportedMeths[index];

	FUNCDESC* fd = new FUNCDESC();
	memset(fd, 0, sizeof(FUNCDESC));
	fd->memid = index;
	fd->funckind = FUNC_DISPATCH;
	fd->invkind = INVOKE_FUNC;
	fd->cParams = mi.Params->Length;

	*ppFuncDesc = fd;

	return S_OK;
}

#pragma endregion
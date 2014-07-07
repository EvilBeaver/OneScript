#include "stdafx.h"
#include "IAddinImpl.h"
#include "MarshalingHelpers.h"
#include "DispatchHelpers.h"
#include <OleAuto.h>

IAddinImpl::IAddinImpl(ScriptEngine::Machine::Contexts::ScriptDrivenObject^ innerObject) : RefCountable()
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
        
HRESULT STDMETHODCALLTYPE IAddinImpl::macroses(SAFEARRAY **result)
{
	String^ prefix = L"Макрос_";
	
	array<System::String^>^ macrosArray = m_innerObject->GetExportedMethods();
	int macroCount = 0;
	for each (String^ name in macrosArray)
	{
		if(name->StartsWith(prefix))
		{
			macroCount++;
		}
	}
	SAFEARRAYBOUND  Bound[1];
    Bound[0].lLbound   = 0;
	Bound[0].cElements = macroCount;
	*result = SafeArrayCreate(VT_VARIANT, 1, Bound);
	LONG idx[1];

	for (int i = 0, j = 0; i < macrosArray->Length; i++)
	{
		if(macrosArray[i]->StartsWith(prefix) && macrosArray[i]->Length > prefix->Length)
		{
			WCHAR* buf = stringBuf(macrosArray[i]->Substring(prefix->Length));
			BSTR allocString = SysAllocString(buf);
			delete[] buf;
		
			VARIANT val;
			V_VT(&val) = VT_BSTR;
			V_BSTR(&val) = allocString;

			idx[0] = j++;
			HRESULT hr = SafeArrayPutElement(*result, idx, &val);
		}
	}

	return S_OK;

}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::invokeMacros( 
    BSTR MacrosName,
    VARIANT *result)
{
	String^ prefix = L"Макрос_";
	System::String^ strMacro = prefix + (gcnew System::String(MacrosName));

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

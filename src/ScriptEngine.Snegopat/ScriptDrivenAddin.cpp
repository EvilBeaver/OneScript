#include "stdafx.h"
#include "ScriptDrivenAddin.h"


ScriptDrivenAddin::ScriptDrivenAddin(LoadedModuleHandle module) 
	: ScriptDrivenObject(module)
{
	m_scriptDispatcher = NULL;
	m_marshalledReference = nullptr;
}

ScriptDrivenAddin::~ScriptDrivenAddin()
{
	if(m_scriptDispatcher != NULL)
	{
		m_scriptDispatcher->Release();
		m_scriptDispatcher = NULL;
	}

	if(m_marshalledReference != nullptr)
	{
		System::Runtime::InteropServices::Marshal::ReleaseComObject(m_marshalledReference);
		m_marshalledReference = nullptr;
	}
}

Object^ ScriptDrivenAddin::UnderlyingObject::get()
{
	if(m_scriptDispatcher == NULL)
	{
		m_scriptDispatcher = new SelfScriptIDispatch(this);
		m_scriptDispatcher->AddRef();
	
		IUnknown* pUnk;
		m_scriptDispatcher->QueryInterface(IID_IUnknown, (void**)&pUnk);
		IntPtr pointer = IntPtr(pUnk);
		m_marshalledReference = System::Runtime::InteropServices::Marshal::GetObjectForIUnknown(pointer); // increments refCount
		pUnk->Release();
	}

	return m_marshalledReference;
}

int ScriptDrivenAddin::FindOwnProperty(String^ name)
{
	if(name->ToLower() == L"ЭтотОбъект")
	{
		return 0;
	}
	else
	{
		return -1;
	}
}
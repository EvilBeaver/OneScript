#include "stdafx.h"
#include "IAddinLoaderImpl.h"
#include "IAddinImpl.h"
#include "Snegopat_i.c"

WCHAR* stringBuf(System::String^ str)
{
	int len = str->Length;
	WCHAR* buf = new WCHAR[len+1];
	memset(buf, 0, (len+1) * sizeof(WCHAR));
	for(int i = 0; i < len; i++)
	{
		buf[i] = str[i];
	}

	return buf;
}

IAddinLoaderImpl::IAddinLoaderImpl(IDispatch* pDesigner) : RefCountable()
{
	m_pDesigner = pDesigner;
	m_pDesigner->AddRef();
	
	ScriptEngine::ScriptingEngine^ a = gcnew ScriptEngine::ScriptingEngine();

	/*ScriptEngine::RuntimeEnvironment^ env = gcnew ScriptEngine::RuntimeEnvironment();
	m_engine->Initialize(env);*/

}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinLoaderImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if(riid == IID_IAddinLoader)
	{
		*ppObj = static_cast<IAddinLoader*>(this);
		return S_OK;
	}
	else
	{
		return IUnknownQueried(riid, ppObj);
	}
}

ULONG   __stdcall IAddinLoaderImpl::AddRef()
{
	return RefCountable::AddRef();
}

ULONG   __stdcall IAddinLoaderImpl::Release()
{
	return RefCountable::AddRef();
}

#pragma endregion

IAddinLoaderImpl::~IAddinLoaderImpl(void)
{
	if(nullptr != (ScriptEngine::ScriptingEngine^)m_engine)
	{
		m_engine = nullptr;
		m_engine = nullptr;
	}
}

void IAddinLoaderImpl::OnZeroCount()
{
	m_pDesigner->Release();
}

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
	*fullPath = SysAllocString(L"fullpath");
	*uniqueName = SysAllocString(L"uniqueName");
	*displayName = SysAllocString(L"displayName");

	IAddin* obj = new IAddinImpl(nullptr);

	return S_OK;

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
	*result = SysAllocString(L"load 1c|1c");
	return S_OK;
}
        
HRESULT __stdcall  IAddinLoaderImpl::selectLoadURI( 
    BSTR *result)
{
	return E_NOTIMPL;
}

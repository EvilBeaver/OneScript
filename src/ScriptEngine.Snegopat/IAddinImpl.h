#pragma once

#include <comdef.h>
#include "Snegopat_h.h"
#include "RefCountable.h"
#include <vcclr.h>

using namespace System;
using namespace ScriptEngine;

class IAddinImpl :
	public RefCountable,
	public IAddinMacroses
{
private:
	gcroot<Machine::Contexts::ScriptDrivenObject^> m_innerObject;
	
	BSTR m_uniqueName;
	BSTR m_displayName;
	BSTR m_fullPath;

public:
	
	IAddinImpl(Machine::Contexts::ScriptDrivenObject^ innerObject);

	void SetNames(BSTR uniqueName, BSTR displayName, BSTR fullPath)
	{
		m_uniqueName = uniqueName;
		m_displayName = displayName;
		m_fullPath = fullPath;
	}

	ScriptEngine::Machine::Contexts::ScriptDrivenObject^
		GetManagedInstance()
	{
		return m_innerObject;
	}

	//IUnknown interface 
    virtual HRESULT  __stdcall QueryInterface(
                                REFIID riid, 
                                void **ppObj);
    virtual ULONG   __stdcall AddRef();
    virtual ULONG   __stdcall Release();
    
	// IAddIn interface

    virtual HRESULT STDMETHODCALLTYPE macroses(SAFEARRAY **result);
        
    virtual HRESULT STDMETHODCALLTYPE invokeMacros(BSTR MacrosName, VARIANT *result);
        
	virtual void OnZeroCount();

	~IAddinImpl(void);

};


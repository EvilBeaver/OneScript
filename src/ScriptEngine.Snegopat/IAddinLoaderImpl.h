#pragma once

#include "Stdafx.h"
#include "Snegopat_h.h"
#include "RefCountable.h"
#include "IAddinImpl.h"
#include "IAddinLoaderImpl.h"
#include "SnegopatAttachedContext.h"

#include <vcclr.h>

using namespace System;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Compiler;
using namespace ScriptEngine::Environment;
using namespace System::Runtime::InteropServices;

class IAddinLoaderImpl :
	public RefCountable,
	public IAddinLoader
{
private:
	
	IDispatch* m_pDesigner;
	gcroot<ScriptEngine::ScriptingEngine^> m_engine;

protected:
	virtual void OnZeroCount();

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


#pragma once
#include "StdAfx.h"
#include <comdef.h>
#include "IAddinImpl.h"
#include "IAddinLoaderImpl.h"

using namespace System;

ref class CriticalResourceLoader
{
private:
	HMODULE m_module;
	WCHAR* m_modulePath;

	Reflection::Assembly^ DependencyHandler(Object^ sender, ResolveEventArgs^ args);

public:
	CriticalResourceLoader(HMODULE);
	bool PrepareTypeInfo();
	IUnknown* GetLoader(IDispatch* pDesigner);
	~CriticalResourceLoader(void);
};


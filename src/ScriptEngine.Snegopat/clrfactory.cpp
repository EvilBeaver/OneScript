#include "StdAfx.h"
#include <comdef.h>
#include <vcclr.h>

#include "CriticalResourceLoader.h"

gcroot<CriticalResourceLoader^> g_ResLoader;

void InitLibrary(HMODULE module)
{
	g_ResLoader = gcnew CriticalResourceLoader(module);
}

IUnknown* GetLoader(IDispatch* pDesigner)
{
	return g_ResLoader->GetLoader(pDesigner);
}

bool PrepareTypeInfo()
{
	return g_ResLoader->PrepareTypeInfo();
}
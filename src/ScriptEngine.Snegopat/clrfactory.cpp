#include "StdAfx.h"
#include <comdef.h>
#include "IAddinImpl.h"
#include "IAddinLoaderImpl.h"

IUnknown* GetLoader(IDispatch* pDesigner)
{
	IAddinLoader* loader = new IAddinLoaderImpl(pDesigner);
	return loader;
}

bool PrepareTypeInfo(HMODULE libHandle)
{
	HRESULT hr;
	WCHAR path[MAX_PATH+1];
	memset(path, 0, (MAX_PATH+1) * sizeof(WCHAR));

	GetModuleFileName(libHandle, path, MAX_PATH);
	ITypeLib* lib;
	hr = LoadTypeLib(path, &lib);
	if(FAILED(hr))
	{
		return false;
	}

	IAddinImpl::CreateTypeInfo(lib);

}
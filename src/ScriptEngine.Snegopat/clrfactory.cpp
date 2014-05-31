#include "StdAfx.h"
#include <comdef.h>
#include "IAddinImpl.h"
#include "IAddinLoaderImpl.h"

HMODULE g_ñurrentModule;

System::Reflection::Assembly^ Handler(System::Object^ sender, System::ResolveEventArgs^ args)
{
	System::String^ str = args->Name;
	if(str->IndexOf("ScriptEngine",0) >= 0)
	{
		WCHAR path[MAX_PATH+1];
		memset(path, 0, (MAX_PATH+1) * sizeof(WCHAR));
		HRESULT hr = GetModuleFileName(g_ñurrentModule, path, MAX_PATH);
		if(FAILED(hr))
		{
			return nullptr;
		}

		System::IntPtr^ ptr = gcnew System::IntPtr(path);
		str = System::Runtime::InteropServices::Marshal::PtrToStringUni(*ptr, MAX_PATH);
		System::String^ pathBuild = str;
		try
		{
			int idx = pathBuild->LastIndexOf('\\');
			pathBuild = pathBuild->Substring(0, idx + 1);
			//pathBuild = System::IO::Path::GetDirectoryName(pathBuild);
			pathBuild = System::IO::Path::Combine(pathBuild, "ScriptEngine.dll");
		}
		catch(System::Exception^ e)
		{
			int f = 2;
		}
		return System::Reflection::Assembly::LoadFrom(pathBuild);

	}
	else
	{
		return nullptr;
	}
}

IUnknown* GetLoader(IDispatch* pDesigner)
{
	System::AppDomain::CurrentDomain->AssemblyResolve += 
		gcnew System::ResolveEventHandler(&Handler);
	IAddinLoader* loader = new IAddinLoaderImpl(pDesigner);
	return loader;
}

bool PrepareTypeInfo(HMODULE libHandle)
{
	g_ñurrentModule = libHandle;
	HRESULT hr;
	WCHAR path[MAX_PATH+1];
	memset(path, 0, (MAX_PATH+1) * sizeof(WCHAR));
	hr = GetModuleFileName(libHandle, path, MAX_PATH);
	if(FAILED(hr))
	{
		return false;
	}

	ITypeLib* lib;
	hr = LoadTypeLib(path, &lib);
	if(FAILED(hr))
	{
		return false;
	}

	IAddinImpl::CreateTypeInfo(lib);

	return true;

}
#include "stdafx.h"
#include "CriticalResourceLoader.h"

CriticalResourceLoader::CriticalResourceLoader(HMODULE mod)
{
	m_module = mod;
	m_modulePath = new WCHAR[MAX_PATH+1];
	memset(m_modulePath, 0, (MAX_PATH+1) * sizeof(WCHAR));
	GetModuleFileName(mod, m_modulePath, MAX_PATH);

	System::AppDomain::CurrentDomain->AssemblyResolve += 
		gcnew System::ResolveEventHandler(this, &CriticalResourceLoader::DependencyHandler);

}

Reflection::Assembly^ CriticalResourceLoader::DependencyHandler(Object^ sender, ResolveEventArgs^ args)
{
	System::String^ str = args->Name;
	if(str->IndexOf(L"ScriptEngine",0) >= 0)
	{
		
		System::IntPtr^ ptr = gcnew System::IntPtr(m_modulePath);
		System::String^ pathBuild = System::Runtime::InteropServices::Marshal::PtrToStringUni(*ptr, MAX_PATH);

		int idx = pathBuild->LastIndexOf('\\');
		System::String^ dir = pathBuild->Substring(0, idx + 1);

		idx = str->IndexOf(',');
		System::String^ dll = str->Substring(0, idx) + ".dll";
		pathBuild = System::IO::Path::Combine(dir, dll);
		
		return System::Reflection::Assembly::LoadFrom(pathBuild);

	}
	else
	{
		return nullptr;
	}
}

CriticalResourceLoader::~CriticalResourceLoader(void)
{
	delete[] m_modulePath;
}

bool CriticalResourceLoader::PrepareTypeInfo()
{
	return true;
}

IUnknown* CriticalResourceLoader::GetLoader(IDispatch* pDesigner)
{
	IAddinLoader* loader = new IAddinLoaderImpl(pDesigner);
	return loader;
}
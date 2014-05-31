
#include "Stdafx.h"
#include <comdef.h>
#include <sstream>
#include "DispatchHelpers.h";

void InitLibrary(HMODULE module);
IUnknown* GetLoader(IDispatch*);
bool PrepareTypeInfo();

HMODULE g_CurrentModule;

BOOL WINAPI DllMain(
  HINSTANCE hinstDLL,
  DWORD fdwReason,
  LPVOID lpvReserved
)
{
	if(fdwReason == DLL_PROCESS_ATTACH)
	{
		g_CurrentModule = hinstDLL;
	}

	return TRUE;
}

extern "C" void __declspec(dllexport) addinInfo(BSTR* uniqueName, BSTR* displayName)
{
    *uniqueName = SysAllocString(L"1ScriptLoader");
    *displayName = SysAllocString(L"Загрузка скриптов на языке 1С");

}

extern "C" void __declspec(dllexport) initAddin(IDispatch* pDesigner)
{
#ifdef _DEBUG
	MessageBox(0, L"attach debugger and press ok", L"Debug message", MB_OK);
#endif

	InitLibrary(g_CurrentModule);
	if(!PrepareTypeInfo())
	{
		return;
	}

	HRESULT hr;
	VARIANT addins;
	addins.vt = VT_DISPATCH;
	VariantInit(&addins);
	hr = invoke(pDesigner, DISPATCH_PROPERTYGET, &addins, NULL, NULL, L"addins", NULL);
	if(FAILED(hr))
	{
		return;
	}
	
	IDispatch* addinsObj = V_DISPATCH(&addins);

	IUnknown* loader = GetLoader(pDesigner);
	hr = invoke(addinsObj, DISPATCH_METHOD, NULL, NULL, NULL, L"registerLoader", L"U", loader);
}

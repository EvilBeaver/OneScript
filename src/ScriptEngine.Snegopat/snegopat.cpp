
#include "Stdafx.h"
#include <comdef.h>

extern "C" void __declspec(dllexport) addinInfo(BSTR* uniqueName, BSTR* displayName)
{
    *uniqueName = SysAllocString(L"MyDllAddin");
    *displayName = SysAllocString(L"Ìîé ÄËË àääèí");
}

extern "C" void __declspec(dllexport) initAddin(IDispatch* pDesigner)
{
	//g_pDesigner = pDesigner;
	pDesigner->AddRef();
}
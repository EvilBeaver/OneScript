#include <windows.h>

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

#define DllExport extern "C" __declspec(dllexport)

#include "include/types.h"
#include "include/ComponentBase.h"
#include "include/AddInDefBase.h"
#include "include/IMemoryManager.h"

class ProxyComponent : public IMemoryManager {
private:
	IComponentBase* pInterface = nullptr;
public:
	ProxyComponent(IComponentBase* pInterface) : pInterface(pInterface) {
		//		pInterface->Init(this);
		pInterface->setMemManager(this);
	}
	virtual ~ProxyComponent() override {
		pInterface->Done();
		delete pInterface;
	}
	virtual bool ADDIN_API AllocMemory(void** pMemory, unsigned long ulCountByte) override {
		return *pMemory = malloc(ulCountByte);
	}
	virtual void ADDIN_API FreeMemory(void** pMemory) override {
		free(*pMemory);
	}
	IComponentBase& Interface() {
		return *pInterface;
	}
};

static void ClearVariant(tVariant &variant) 
{
	switch (variant.vt) {
	case VT_BLOB:
	case VT_LPSTR:
		free(variant.pstrVal);
		variant.pstrVal = nullptr;
		variant.strLen = 0;
		break;
	case VT_LPWSTR:
		free(variant.pwstrVal);
		variant.pwstrVal = nullptr;
		variant.wstrLen = 0;
		break;
	}
	variant.vt = VT_EMPTY;
}

typedef void(_stdcall* StringFuncRespond) (const WCHAR_T* s);

typedef void(_stdcall* VariantFuncRespond) (tVariant* variant);

typedef void(_stdcall* VariantListRespond) (tVariant* variant, long number);

DllExport ProxyComponent* GetClassObject(HMODULE hModule, const WCHAR_T* wsName)
{
	auto proc = (GetClassObjectPtr)GetProcAddress(hModule, "GetClassObject");
	if (proc == nullptr) return nullptr;
	IComponentBase* pComponent = nullptr;
	auto ok = proc(wsName, &pComponent);
	if (ok == 0) return nullptr;
	return new ProxyComponent(pComponent);
}

DllExport long DestroyObject(ProxyComponent* proxy)
{
	if (proxy) delete proxy;
	return 0;
}

DllExport long GetNProps(ProxyComponent* proxy)
{
	if (proxy == nullptr) return 0;
	return proxy->Interface().GetNProps();
}

DllExport long FindProp(ProxyComponent* proxy, const WCHAR_T* wsPropName)
{
	if (proxy == nullptr) return -1;
	return proxy->Interface().FindProp(wsPropName);
}

DllExport void GetPropName(ProxyComponent* proxy, long lPropNum, long lPropAlias, StringFuncRespond respond)
{
	if (proxy == nullptr) return;
	auto name = proxy->Interface().GetPropName(lPropNum, lPropAlias);
	if (name) {
		respond(name);
		delete name;
	}
}

DllExport bool GetPropVal(ProxyComponent* proxy, long lPropNum, VariantFuncRespond respond)
{
	if (proxy == nullptr) return false;
	tVariant variant = { 0 };
	auto ok = proxy->Interface().GetPropVal(lPropNum, &variant);
	if (ok) respond(&variant);
	ClearVariant(variant);
	return ok;
}

DllExport bool SetPropVal(ProxyComponent* proxy, long lPropNum, VariantFuncRespond respond)
{
	if (proxy == nullptr) return false;
	tVariant variant = { 0 };
	respond(&variant);
	auto ok = proxy->Interface().SetPropVal(lPropNum, &variant);
	ClearVariant(variant);
	return ok;
}

DllExport bool IsPropReadable(ProxyComponent* proxy, long lPropNum)
{
	if (proxy == nullptr) return false;
	return proxy->Interface().IsPropReadable(lPropNum);
}

DllExport bool IsPropWritable(ProxyComponent* proxy, long lPropNum)
{
	if (proxy == nullptr) return false;
	auto res = proxy->Interface().IsPropWritable(lPropNum);
	return res;
}

DllExport long GetNMethods(ProxyComponent* proxy)
{
	if (proxy == nullptr) return 0;
	return proxy->Interface().GetNMethods();
}

DllExport long FindMethod(ProxyComponent* proxy, const WCHAR_T* wsMethodName)
{
	if (proxy == nullptr) return -1;
	return proxy->Interface().FindMethod(wsMethodName);
}

DllExport void GetMethodName(ProxyComponent* proxy, long lMethodNum, long lMethodAlias, StringFuncRespond respond)
{
	if (proxy == nullptr) return;
	auto name = proxy->Interface().GetMethodName(lMethodNum, lMethodAlias);
	if (name) {
		respond(name);
		delete name;
	}
}

DllExport long GetNParams(ProxyComponent* proxy, long lMethodNum)
{
	if (proxy == nullptr) return 0;
	return proxy->Interface().GetNParams(lMethodNum);
}

DllExport bool HasRetVal(ProxyComponent* proxy, long lMethodNum)
{
	if (proxy == nullptr) return false;
	return proxy->Interface().HasRetVal(lMethodNum);
}

DllExport bool ADDIN_API CallAsProc(ProxyComponent* proxy, long lMethodNum, VariantListRespond getter, VariantListRespond setter, long lSizeArray)
{
	if (proxy == nullptr) return false;
	tVariant* paParams = new tVariant[lSizeArray];
	for (long i = 0; i < lSizeArray; i++) {
		ZeroMemory(&paParams[i], sizeof(tVariant));
		getter(&paParams[i], i);
	}
	bool ok = proxy->Interface().CallAsProc(lMethodNum, paParams, lSizeArray);
	for (long i = 0; i < lSizeArray; i++) {
		setter(&paParams[i], i);
		ClearVariant(paParams[i]);
	}
	delete[] paParams;
	return ok;
}

DllExport bool ADDIN_API CallAsFunc(ProxyComponent* proxy, long lMethodNum, VariantListRespond getter, VariantListRespond setter, long lSizeArray, VariantFuncRespond respond)
{
	if (proxy == nullptr) return false;
	tVariant variant = { 0 };
	tVariant* paParams = new tVariant[lSizeArray];
	for (long i = 0; i < lSizeArray; i++) {
		ZeroMemory(&paParams[i], sizeof(tVariant));
		getter(&paParams[i], i);
	}
	bool ok = proxy->Interface().CallAsFunc(lMethodNum, &variant, paParams, lSizeArray);
	if (ok) {
		respond(&variant);
		for (long i = 0; i < lSizeArray; i++) {
			setter(&paParams[i], i);
			ClearVariant(paParams[i]);
		}
	}
	ClearVariant(variant);
	delete[] paParams;
	return ok;
}

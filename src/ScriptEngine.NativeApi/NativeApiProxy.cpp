/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#include "include/types.h"
#include "include/ComponentBase.h"
#include "include/AddInDefBase.h"
#include "include/IMemoryManager.h"
#include "NativeInterface.h"

#ifdef _WINDOWS

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		::DisableThreadLibraryCalls(hModule);
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

#define DllExport extern "C" __declspec(dllexport)

#else//_WINDOWS

#define DllExport extern "C"

#endif//_WINDOWS

#define CHECK_PROXY(result) { if (proxy == nullptr) return result; }

#define EMPTY_DEF

typedef void(_stdcall* StringFuncRespond) (const WCHAR_T* s);
typedef void(_stdcall* VariantFuncRespond) (const tVariant* variant);

class ProxyComponent : public IMemoryManager {
private:
	IComponentBase* pComponent = nullptr;
	NativeInterface mInterface;
public:
	ProxyComponent(
		IComponentBase* pComponent,
		ErrorFuncRespond onError,
		EventFuncRespond onEvent,
		StatusFuncRespond onStatus
	) :
		pComponent(pComponent),
		mInterface(onError, onEvent, onStatus)
	{
		pComponent->setMemManager(this);
		pComponent->Init(&mInterface);
	}
	virtual ~ProxyComponent() override {
		pComponent->Done();
		delete pComponent;
	}
	virtual bool ADDIN_API AllocMemory(void** pMemory, unsigned long ulCountByte) override {
#ifdef _WINDOWS
		return *pMemory = LocalAlloc(LMEM_FIXED, ulCountByte);
#else
		return *pMemory = calloc(1, ulCountByte);
#endif//_WINDOWS
	}
	virtual void ADDIN_API FreeMemory(void** pMemory) override {
#ifdef _WINDOWS
		LocalFree(*pMemory);
		*pMemory = nullptr;
#else
		free(*pMemory);
		*pMemory = nullptr;
#endif//_WINDOWS
	}
	IComponentBase& Component() {
		return *pComponent;
	}
};

static void ClearVariant(tVariant& variant)
{
	switch (variant.vt) {
	case VTYPE_BLOB:
	case VTYPE_PSTR:
		free(variant.pstrVal);
		variant.pstrVal = nullptr;
		variant.strLen = 0;
		break;
	case VTYPE_PWSTR:
		free(variant.pwstrVal);
		variant.pwstrVal = nullptr;
		variant.wstrLen = 0;
		break;
	}
	variant.vt = VTYPE_EMPTY;
}

DllExport ProxyComponent* GetClassObject(
	HMODULE hModule,
	const WCHAR_T* wsName,
	ErrorFuncRespond onError = nullptr,
	EventFuncRespond onEvent = nullptr,
	StatusFuncRespond onStatus = nullptr
)
{
	auto proc = (GetClassObjectPtr)GetProcAddress(hModule, "GetClassObject");
	if (proc == nullptr) return nullptr;
	IComponentBase* pComponent = nullptr;
	auto ok = proc(wsName, &pComponent);
	if (ok == 0) return nullptr;
	return new ProxyComponent(pComponent, onError, onEvent, onStatus);
}

DllExport void DestroyObject(ProxyComponent* proxy)
{
	if (proxy) delete proxy;
}

DllExport int32_t GetNProps(ProxyComponent* proxy)
{
	CHECK_PROXY(0);
	return (int32_t)proxy->Component().GetNProps();
}

DllExport int32_t FindProp(ProxyComponent* proxy, const WCHAR_T* wsPropName)
{
	CHECK_PROXY(-1);
	return (int32_t)proxy->Component().FindProp(wsPropName);
}

DllExport void GetPropName(ProxyComponent* proxy, int32_t lPropNum, int32_t lPropAlias, StringFuncRespond respond)
{
	CHECK_PROXY(EMPTY_DEF);
	auto name = proxy->Component().GetPropName(lPropNum, lPropAlias);
	if (name) {
		respond(name);
		proxy->FreeMemory((void**)&name);
	}
}

DllExport bool GetPropVal(ProxyComponent* proxy, int32_t lPropNum, VariantFuncRespond respond)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	auto ok = proxy->Component().GetPropVal(lPropNum, &variant);
	if (ok) respond(&variant);
	ClearVariant(variant);
	return ok;
}

DllExport bool SetPropVal(ProxyComponent* proxy, int32_t lPropNum, tVariant* variant)
{
	CHECK_PROXY(false);
	auto ok = proxy->Component().SetPropVal(lPropNum, variant);
	return ok;
}

DllExport bool IsPropReadable(ProxyComponent* proxy, int32_t lPropNum)
{
	CHECK_PROXY(false);
	return proxy->Component().IsPropReadable(lPropNum);
}

DllExport bool IsPropWritable(ProxyComponent* proxy, int32_t lPropNum)
{
	CHECK_PROXY(false);
	auto res = proxy->Component().IsPropWritable(lPropNum);
	return res;
}

DllExport int32_t GetNMethods(ProxyComponent* proxy)
{
	CHECK_PROXY(0);
	return (int32_t)proxy->Component().GetNMethods();
}

DllExport int32_t FindMethod(ProxyComponent* proxy, const WCHAR_T* wsMethodName)
{
	CHECK_PROXY(-1);
	return (int32_t)proxy->Component().FindMethod(wsMethodName);
}

DllExport void GetMethodName(ProxyComponent* proxy, int32_t lMethodNum, int32_t lMethodAlias, StringFuncRespond respond)
{
	CHECK_PROXY(EMPTY_DEF);
	auto name = proxy->Component().GetMethodName(lMethodNum, lMethodAlias);
	if (name) {
		respond(name);
		proxy->FreeMemory((void**)&name);
	}
}

DllExport int32_t GetNParams(ProxyComponent* proxy, int32_t lMethodNum)
{
	CHECK_PROXY(0);
	return (int32_t)proxy->Component().GetNParams(lMethodNum);
}

DllExport bool ADDIN_API GetParamDefValue(ProxyComponent* proxy, int32_t lMethodNum, int32_t lParamNum, VariantFuncRespond respond)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	auto ok = proxy->Component().GetParamDefValue(lMethodNum, lParamNum, &variant);
	if (ok) respond(&variant);
	ClearVariant(variant);
	return ok;
}

DllExport bool HasRetVal(ProxyComponent* proxy, int32_t lMethodNum)
{
	CHECK_PROXY(false);
	return proxy->Component().HasRetVal(lMethodNum);
}

DllExport bool ADDIN_API CallAsProc(ProxyComponent* proxy, int32_t lMethodNum, tVariant* paParams)
{
	CHECK_PROXY(false);
	auto lSizeArray = GetNParams(proxy, lMethodNum);
	bool ok = proxy->Component().CallAsProc(lMethodNum, paParams, lSizeArray);
	return ok;
}

DllExport bool ADDIN_API CallAsFunc(ProxyComponent* proxy, int32_t lMethodNum, tVariant* paParams, VariantFuncRespond respond)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	auto lSizeArray = GetNParams(proxy, lMethodNum);
	bool ok = proxy->Component().CallAsFunc(lMethodNum, &variant, paParams, lSizeArray);
	if (ok) respond(&variant);
	ClearVariant(variant);
	return ok;
}

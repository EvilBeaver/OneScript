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

#include <stdlib.h>

#endif//_WINDOWS

#define CHECK_PROXY(result) { if (proxy == nullptr) return result; }

#define EMPTY_DEF

typedef void(_stdcall* StringFuncRespond) (const WCHAR_T* s);
typedef void(_stdcall* VariantFuncRespond) (const tVariant* variant);

static bool AllocMemory(void** pMemory, unsigned long ulCountByte) {
#ifdef _WINDOWS
	return *pMemory = LocalAlloc(LMEM_FIXED, ulCountByte);
#else
	return *pMemory = calloc(1, ulCountByte);
#endif//_WINDOWS
}

void ADDIN_API FreeMemory(void** pMemory) {
#ifdef _WINDOWS
	LocalFree(*pMemory);
	*pMemory = nullptr;
#else
	free(*pMemory);
	*pMemory = nullptr;
#endif//_WINDOWS
}

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
		return ::AllocMemory(pMemory, ulCountByte);
	}
	virtual void ADDIN_API FreeMemory(void** pMemory) override {
		if (*pMemory) ::FreeMemory(pMemory);
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
		FreeMemory((void**)&variant.pstrVal);
		variant.strLen = 0;
		break;
	case VTYPE_PWSTR:
		FreeMemory((void**)&variant.pwstrVal);
		variant.wstrLen = 0;
		break;
	}
	variant.vt = VTYPE_EMPTY;
}

DllExport tVariant* CreateVariant(int32_t lSizeArray)
{
	if (lSizeArray <= 0) return nullptr;
	void* ptr = nullptr;
	::AllocMemory(&ptr, sizeof(tVariant) * lSizeArray);
	return (tVariant*)ptr;
}

DllExport void FreeVariant(tVariant* variant)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	::FreeMemory((void**)&variant);
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

DllExport void SetVariantEmpty(tVariant* variant, int32_t number)
{
	tVariant* v = variant + number;
	TV_VT(v) = VTYPE_EMPTY;
}

DllExport void SetVariantBool(tVariant* variant, int32_t number, bool value)
{
	tVariant* v = variant + number;
	TV_BOOL(v) = value;
	TV_VT(v) = VTYPE_BOOL;
}

DllExport void SetVariantReal(tVariant* variant, int32_t number, double value)
{
	tVariant* v = variant + number;
	TV_R8(v) = value;
	TV_VT(v) = VTYPE_R8;
}

DllExport void SetVariantInt(tVariant* variant, int32_t number, int32_t value)
{
	tVariant* v = variant + number;
	TV_I4(v) = value;
	TV_VT(v) = VTYPE_I4;
}

DllExport void SetVariantStr(tVariant* variant, int32_t number, const WCHAR_T* value, int32_t length)
{
	tVariant* v = variant + number;
	unsigned long size = sizeof(WCHAR_T) * (length + 1);
	if (::AllocMemory((void**)&v->pwstrVal, size)) {
		memcpy(v->pwstrVal, value, size);
		v->wstrLen = length;
		while (v->wstrLen && v->pwstrVal[v->wstrLen - 1] == 0) v->wstrLen--;
		TV_VT(v) = VTYPE_PWSTR;
	}
}

DllExport void SetVariantBlob(tVariant* variant, int32_t number, const char* value, int32_t length)
{
	tVariant* v = variant + number;
	if (::AllocMemory((void**)&v->pstrVal, length)) {
		memcpy(v->pstrVal, value, length);
		v->strLen = length;
		TV_VT(v) = VTYPE_BLOB;
	}
}

typedef void(_stdcall* TSetVariantEmpty)(tVariant*, int32_t);
typedef void(_stdcall* TSetVariantBool)(tVariant*, int32_t, bool);
typedef void(_stdcall* TSetVariantReal)(tVariant*, int32_t, double);
typedef void(_stdcall* TSetVariantInt)(tVariant*, int32_t, int32_t);
typedef void(_stdcall* TSetVariantBlob)(tVariant*, int32_t, void*, int32_t);

DllExport void GetVariant(tVariant* variant, int32_t number
	, TSetVariantEmpty e
	, TSetVariantBool b
	, TSetVariantInt i
	, TSetVariantReal r
	, TSetVariantBlob s
	, TSetVariantBlob x
)
{
	if (variant == nullptr) return;
	switch (variant->vt) {
	case VTYPE_EMPTY:
		e(variant, number);
		break;
	case VTYPE_I2:
	case VTYPE_I4:
	case VTYPE_ERROR:
	case VTYPE_UI1:
		i(variant, number, variant->lVal);
		break;
	case VTYPE_BOOL:
		b(variant, number, variant->bVal);
		break;
	case VTYPE_R4:
	case VTYPE_R8:
		r(variant, number, variant->dblVal);
		break;
	case VTYPE_DATE:
	case VTYPE_TM:
		e(variant, number);
		break;
	case VTYPE_PSTR:
		e(variant, number);
		break;
	case VTYPE_PWSTR:
		s(variant, number, variant->pwstrVal, variant->strLen);
		break;
	case VTYPE_BLOB:
		x(variant, number, variant->pstrVal, variant->strLen);
		break;
	default:
		e(variant, number);
	}
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

DllExport bool ADDIN_API HasParamDefValue(ProxyComponent* proxy, int32_t lMethodNum, int32_t lParamNum)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	bool result = proxy->Component().GetParamDefValue(lMethodNum, lParamNum, &variant) && variant.vt != VTYPE_EMPTY;
	ClearVariant(variant);
	return result;
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

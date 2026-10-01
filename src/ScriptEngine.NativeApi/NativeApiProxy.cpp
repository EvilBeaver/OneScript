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
	return *pMemory = LocalAlloc(LMEM_FIXED | LMEM_ZEROINIT, ulCountByte);
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
	HMODULE hModule = nullptr;
	IComponentBase* pComponent = nullptr;
	NativeInterface mInterface;
public:
	ProxyComponent(
		HMODULE hModule,
		IComponentBase* pComponent,
		ErrorFuncRespond onError,
		EventFuncRespond onEvent,
		StatusFuncRespond onStatus
	) :
		hModule(hModule),
		pComponent(pComponent),
		mInterface(onError, onEvent, onStatus)
	{
		pComponent->setMemManager(this);
		pComponent->Init(&mInterface);
	}
	virtual ~ProxyComponent() override {
		if (!pComponent)
			return;

		try {
			pComponent->Done();
		} catch (...) {}

		try {
			auto proc = (DestroyObjectPtr)GetProcAddress(hModule, "DestroyObject");
			if (proc) {
				// Non-zero return: object not deleted, component memory leaked.
				proc(&pComponent);
			} else {
				delete pComponent;
				pComponent = nullptr;
			}
		} catch (...) {
			if (pComponent) {
				delete pComponent;
				pComponent = nullptr;
			}
		}
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
	if (!::AllocMemory(&ptr, sizeof(tVariant) * lSizeArray))
		return nullptr;
	return (tVariant*)ptr;
}

DllExport void FreeVariant(tVariant* variant, int32_t count)
{
	if (variant == nullptr) return;
	for (int32_t i = 0; i < count; i++)
		::ClearVariant(variant[i]);
	::FreeMemory((void**)&variant);
}

DllExport tVariant* VariantElement(tVariant* variant, int32_t index)
{
	if (variant == nullptr || index < 0) return nullptr;
	return variant + index;
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
	return new ProxyComponent(hModule, pComponent, onError, onEvent, onStatus);
}

DllExport void DestroyObject(ProxyComponent* proxy)
{
	if (proxy) delete proxy;
}

DllExport void GetExtensionName(ProxyComponent* proxy, StringFuncRespond respond)
{
	CHECK_PROXY(EMPTY_DEF);
	WCHAR_T* name = nullptr;
	auto ok = proxy->Component().RegisterExtensionAs(&name);
	if (ok && name) respond(name);
	if (name) proxy->FreeMemory((void**)&name);
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

DllExport void SetVariantEmpty(tVariant* variant)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
}

DllExport void SetVariantBool(tVariant* variant, bool value)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	TV_BOOL(variant) = value;
	TV_VT(variant) = VTYPE_BOOL;
}

DllExport void SetVariantReal(tVariant* variant, double value)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	TV_R8(variant) = value;
	TV_VT(variant) = VTYPE_R8;
}

DllExport void SetVariantInt(tVariant* variant, int32_t value)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	TV_I4(variant) = value;
	TV_VT(variant) = VTYPE_I4;
}

DllExport void SetVariantStr(tVariant* variant, const WCHAR_T* value, int32_t length)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	unsigned long size = sizeof(WCHAR_T) * (length + 1);
	if (::AllocMemory((void**)&variant->pwstrVal, size)) {
		memcpy(variant->pwstrVal, value, size);
		variant->wstrLen = length;
		while (variant->wstrLen && variant->pwstrVal[variant->wstrLen - 1] == 0) variant->wstrLen--;
		TV_VT(variant) = VTYPE_PWSTR;
	}
}

DllExport void SetVariantBlob(tVariant* variant, const char* value, int32_t length)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);
	if (::AllocMemory((void**)&variant->pstrVal, length)) {
		memcpy(variant->pstrVal, value, length);
		variant->strLen = length;
		TV_VT(variant) = VTYPE_BLOB;
	}
}

static void FillTmExtraFields(struct tm& t)
{
	int y = t.tm_year + 1900;
	int m = t.tm_mon + 1;
	int d = t.tm_mday;

	static const int monthOffset[] = { 0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };
	int yAdj = y - (m < 3);
	t.tm_wday = (yAdj + yAdj / 4 - yAdj / 100 + yAdj / 400 + monthOffset[m - 1] + d) % 7;

	static const int daysBefore[] = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
	int leap = (m > 2 && ((y % 4 == 0 && y % 100 != 0) || y % 400 == 0));
	t.tm_yday = daysBefore[m - 1] + d - 1 + leap;
	t.tm_isdst = -1;
}

DllExport void SetVariantTm(tVariant* variant,
	int32_t year, int32_t month, int32_t day,
	int32_t hour, int32_t minute, int32_t second)
{
	if (variant == nullptr) return;
	::ClearVariant(*variant);

	variant->tmVal = {};
	variant->tmVal.tm_year = year - 1900;
	variant->tmVal.tm_mon = month - 1;
	variant->tmVal.tm_mday = day;
	variant->tmVal.tm_hour = hour;
	variant->tmVal.tm_min = minute;
	variant->tmVal.tm_sec = second;
	FillTmExtraFields(variant->tmVal);
	TV_VT(variant) = VTYPE_TM;
}

typedef void(_stdcall* TVariantEmptyRespond)();
typedef void(_stdcall* TVariantBoolRespond)(bool);
typedef void(_stdcall* TVariantIntRespond)(int32_t);
typedef void(_stdcall* TVariantRealRespond)(double);
typedef void(_stdcall* TVariantDateRespond)(double);
typedef void(_stdcall* TVariantTmRespond)(int32_t, int32_t, int32_t, int32_t, int32_t, int32_t);
typedef void(_stdcall* TVariantBlobRespond)(void*, int32_t);

DllExport void GetVariant(tVariant* variant
	, TVariantEmptyRespond e
	, TVariantBoolRespond b
	, TVariantIntRespond i
	, TVariantRealRespond r
	, TVariantDateRespond d
	, TVariantTmRespond tm
	, TVariantBlobRespond s
	, TVariantBlobRespond x
	, TVariantBlobRespond p
)
{
	if (variant == nullptr) return;
	switch (variant->vt) {
	case VTYPE_EMPTY:
		e();
		break;
	case VTYPE_I2:
	case VTYPE_I4:
	case VTYPE_ERROR:
	case VTYPE_UI1:
		i(variant->lVal);
		break;
	case VTYPE_BOOL:
		b(variant->bVal);
		break;
	case VTYPE_R4:
	case VTYPE_R8:
		r(variant->dblVal);
		break;
	case VTYPE_DATE:
		d(variant->dblVal);
		break;
	case VTYPE_TM:
		tm(
			variant->tmVal.tm_year + 1900,
			variant->tmVal.tm_mon + 1,
			variant->tmVal.tm_mday,
			variant->tmVal.tm_hour,
			variant->tmVal.tm_min,
			variant->tmVal.tm_sec);
		break;
	case VTYPE_PSTR:
		p(variant->pstrVal, variant->strLen);
		break;
	case VTYPE_PWSTR:
		s(variant->pwstrVal, variant->strLen);
		break;
	case VTYPE_BLOB:
		x(variant->pstrVal, variant->strLen);
		break;
	default:
		e();
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

DllExport bool HasParamDefValue(ProxyComponent* proxy, int32_t lMethodNum, int32_t lParamNum)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	bool result = proxy->Component().GetParamDefValue(lMethodNum, lParamNum, &variant) && variant.vt != VTYPE_EMPTY;
	ClearVariant(variant);
	return result;
}

DllExport bool GetParamDefValue(ProxyComponent* proxy, int32_t lMethodNum, int32_t lParamNum, VariantFuncRespond respond)
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

DllExport bool CallAsProc(ProxyComponent* proxy, int32_t lMethodNum, tVariant* paParams)
{
	CHECK_PROXY(false);
	auto lSizeArray = GetNParams(proxy, lMethodNum);
	bool ok = proxy->Component().CallAsProc(lMethodNum, paParams, lSizeArray);
	return ok;
}

DllExport bool CallAsFunc(ProxyComponent* proxy, int32_t lMethodNum, tVariant* paParams, VariantFuncRespond respond)
{
	CHECK_PROXY(false);
	tVariant variant = { 0 };
	auto lSizeArray = GetNParams(proxy, lMethodNum);
	bool ok = proxy->Component().CallAsFunc(lMethodNum, &variant, paParams, lSizeArray);
	if (ok) respond(&variant);
	ClearVariant(variant);
	return ok;
}

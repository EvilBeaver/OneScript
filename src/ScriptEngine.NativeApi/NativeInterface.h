#ifndef NATIVEINTERFACE_H
#define NATIVEINTERFACE_H

#ifndef _WINDOWS
#define _stdcall
#endif//_WINDOWS

#include "include/AddInDefBase.h"

typedef void(_stdcall* ErrorFuncRespond) (unsigned short wcode, const WCHAR_T* source, const WCHAR_T* descr, long scode);
typedef void(_stdcall* EventFuncRespond) (WCHAR_T* wszSource, WCHAR_T* wszMessage, WCHAR_T* wszData);
typedef void(_stdcall* StatusFuncRespond) (WCHAR_T* wszStatusLine);

class NativeInterface
	: public IAddInDefBaseEx
{
private:
	ErrorFuncRespond onError;
	EventFuncRespond onEvent;
	StatusFuncRespond onStatus;
	long lEventBufferDepth;
public:
	NativeInterface(
		ErrorFuncRespond onError,
		EventFuncRespond onEvent,
		StatusFuncRespond onStatus
	) :
		onError(onError),
		onEvent(onEvent),
		onStatus(onStatus),
		lEventBufferDepth(1)
	{
	}

	virtual ~NativeInterface() override {}
	virtual bool ADDIN_API AddError(unsigned short wcode, const WCHAR_T* source, const WCHAR_T* descr, long scode) override;
	virtual bool ADDIN_API Read(WCHAR_T* wszPropName, tVariant* pVal, long* pErrCode, WCHAR_T** errDescriptor) override;
	virtual bool ADDIN_API Write(WCHAR_T* wszPropName, tVariant* pVar) override;
	virtual bool ADDIN_API RegisterProfileAs(WCHAR_T* wszProfileName) override;
	virtual bool ADDIN_API SetEventBufferDepth(long lDepth) override;
	virtual long ADDIN_API GetEventBufferDepth() override;
	virtual bool ADDIN_API ExternalEvent(WCHAR_T* wszSource, WCHAR_T* wszMessage, WCHAR_T* wszData) override;
	virtual void ADDIN_API CleanEventBuffer() override;
	virtual bool ADDIN_API SetStatusLine(WCHAR_T* wszStatusLine) override;
	virtual void ADDIN_API ResetStatusLine() override;
	virtual IInterface* ADDIN_API GetInterface(Interfaces iface) override;
};

#endif//NATIVEINTERFACE_H

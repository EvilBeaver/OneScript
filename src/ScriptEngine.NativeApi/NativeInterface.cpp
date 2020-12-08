#include "NativeInterface.h"

bool NativeInterface::AddError(unsigned short wcode, const WCHAR_T* source, const WCHAR_T* descr, long scode) 
{
	if (onError != nullptr) onError(wcode, source, descr, scode);
	return true;
}

bool NativeInterface::Read(WCHAR_T* wszPropName, tVariant* pVal, long* pErrCode, WCHAR_T** errDescriptor)
{
	return true;
}

bool NativeInterface::Write(WCHAR_T* wszPropName, tVariant* pVar)
{
	return true;
}

bool NativeInterface::RegisterProfileAs(WCHAR_T* wszProfileName)
{
	return true;
}

bool NativeInterface::SetEventBufferDepth(long lDepth)
{
	lEventBufferDepth = lDepth;
	return true;
}

long NativeInterface::GetEventBufferDepth()
{
	return lEventBufferDepth;
}

bool NativeInterface::ExternalEvent(WCHAR_T* wszSource, WCHAR_T* wszMessage, WCHAR_T* wszData)
{
	if (onEvent != nullptr) onEvent(wszSource, wszMessage, wszData);
	return true;
}

void NativeInterface::CleanEventBuffer() 
{
}

bool NativeInterface::SetStatusLine(WCHAR_T* wszStatusLine)
{
	if (onStatus != nullptr) onStatus(wszStatusLine);
	return true;
}

void NativeInterface::ResetStatusLine()
{
}

IInterface* NativeInterface::GetInterface(Interfaces iface)
{
	return nullptr;
}

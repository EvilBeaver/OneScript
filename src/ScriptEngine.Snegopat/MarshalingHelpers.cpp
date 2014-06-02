#include "Stdafx.h"
#include "MarshalingHelpers.h"

WCHAR* stringBuf(System::String^ str)
{
	int len = str->Length;
	WCHAR* buf = new WCHAR[len+1];
	memset(buf, 0, (len+1) * sizeof(WCHAR));
	for(int i = 0; i < len; i++)
	{
		buf[i] = str[i];
	}

	return buf;
}

BSTR stringToBSTR(System::String^ str)
{
	WCHAR* buf = stringBuf(str);
	BSTR ret = SysAllocString(buf);
	delete[] buf;

	return ret;
}
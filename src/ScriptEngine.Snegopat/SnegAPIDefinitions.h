#pragma once

#include "Stdafx.h"

using namespace System;
using namespace System::Runtime::InteropServices;

[ComVisible(true)]
[Guid("F80C31B9-1D9F-4438-BEF3-10D829664EFD")]
[InterfaceType(ComInterfaceType::InterfaceIsDual)]
interface class IParamsWrapper
{
public:
	
	[DispId(0x60020000)]
	property Object^ val
	{
		Object^ get();
		void set(Object^ value);
	}

	/*[DispId(0x60020000), propget, helpstring("Значение")]
    HRESULT val([out, retval] VARIANT* pVal);
    [id(0x60020000), propput, helpstring("Значение")]
    HRESULT val([in] VARIANT pVal);*/
};
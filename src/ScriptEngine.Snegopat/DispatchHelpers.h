#pragma once

#include <comdef.h>

HRESULT invoke(LPDISPATCH pdisp, 
    WORD wFlags,
    LPVARIANT pvRet,
    EXCEPINFO FAR* pexcepinfo,
    UINT FAR* pnArgErr, 
    LPOLESTR pszName,
    LPCTSTR pszFmt, 
    ...);

LPCTSTR getNextVarType(LPCTSTR pszFmt, VARTYPE FAR* pvt);

HRESULT countArgsInFormat(LPCTSTR pszFmt, UINT FAR *pn);
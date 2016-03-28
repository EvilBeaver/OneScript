#include "Stdafx.h"
#include "DispatchHelpers.h"

#pragma unmanaged

LPCTSTR getNextVarType(LPCTSTR pszFmt, VARTYPE FAR* pvt)
{   
    *pvt = 0;
    if (*pszFmt == '&') 
    {
         *pvt = VT_BYREF; 
         pszFmt++;    
         if (!*pszFmt)
             return NULL;    
    } 
    switch(*pszFmt)
    {
        case 'b':
            *pvt |= VT_BOOL;
            break;
        case 'i': 
            *pvt |= VT_I2;
            break;
        case 'I': 
            *pvt |= VT_I4;
            break;
        case 'r': 
            *pvt |= VT_R4;
            break;
        case 'R': 
            *pvt |= VT_R8;
            break;
        case 'c':
            *pvt |= VT_CY;
            break;
        case 's': 
            *pvt |= VT_BSTR;
            break;
        case 'e': 
            *pvt |= VT_ERROR;
            break;
        case 'd': 
            *pvt |= VT_DATE; 
            break;
        case 'v': 
            *pvt |= VT_VARIANT;
            break;
        case 'U': 
            *pvt |= VT_UNKNOWN; 
            break;
        case 'D': 
            *pvt |= VT_DISPATCH;
            break;  
        case '\0':
             return NULL;     // End of Format string
        default:
            return NULL;
    } 
    return ++pszFmt;  
}

HRESULT countArgsInFormat(LPCTSTR pszFmt, UINT FAR *pn)
{
    *pn = 0;

    if(pszFmt == NULL)
      return NOERROR;
    
    while (*pszFmt)  
    {
       if (*pszFmt == '&')
           pszFmt++;

       switch(*pszFmt)
       {
           case 'b':
           case 'i': 
           case 'I':
           case 'r': 
           case 'R':
           case 'c':
           case 's':
           case 'e':
           case 'd':
           case 'v':
           case 'D':
           case 'U':
               ++*pn; 
               pszFmt++;
               break;
           case '\0':  
           default:
               return ResultFromScode(E_INVALIDARG);   
        }
    }
    return NOERROR;
}

HRESULT invoke(LPDISPATCH pdisp, 
    WORD wFlags,
    LPVARIANT pvRet,
    EXCEPINFO FAR* pexcepinfo,
    UINT FAR* pnArgErr, 
    LPOLESTR pszName,
    LPCTSTR pszFmt, 
    ...)
{
    va_list argList;
    va_start(argList, pszFmt);  
    DISPID dispid;
    HRESULT hr;
    VARIANTARG* pvarg = NULL;
  
    if (pdisp == NULL)
        return ResultFromScode(E_INVALIDARG);
    
    // Get DISPID of property/method
    hr = pdisp->GetIDsOfNames(IID_NULL, &pszName, 1, LOCALE_USER_DEFAULT, &dispid);
    if(FAILED(hr))
	{
		return hr;
	}
               
    DISPPARAMS dispparams;
    memset(&dispparams, 0, sizeof dispparams);

    // determine number of arguments
    if (pszFmt != NULL)
        countArgsInFormat(pszFmt, &dispparams.cArgs);
    
    // Property puts have a named argument that represents the value that the property is
    // being assigned.
    DISPID dispidNamed = DISPID_PROPERTYPUT;
    if (wFlags & DISPATCH_PROPERTYPUT)
    {
        if (dispparams.cArgs == 0)
            return ResultFromScode(E_INVALIDARG);
        dispparams.cNamedArgs = 1;
        dispparams.rgdispidNamedArgs = &dispidNamed;
    }

    if (dispparams.cArgs != 0)
    {
        // allocate memory for all VARIANTARG parameters
        pvarg = new VARIANTARG[dispparams.cArgs];
        if(pvarg == NULL)
            return ResultFromScode(E_OUTOFMEMORY);   
        dispparams.rgvarg = pvarg;
        memset(pvarg, 0, sizeof(VARIANTARG) * dispparams.cArgs);

        // get ready to walk vararg list
        LPCTSTR psz = pszFmt;
        pvarg += dispparams.cArgs - 1;   // params go in opposite order
        
        while (psz = getNextVarType(psz, &pvarg->vt))
        {
            if (pvarg < dispparams.rgvarg)
            {
                hr = ResultFromScode(E_INVALIDARG);
                goto cleanup;  
            }
            switch (pvarg->vt)
            {
            case VT_I2:
                V_I2(pvarg) = va_arg(argList, short);
                break;
            case VT_I4:
                V_I4(pvarg) = va_arg(argList, long);
                break;
            case VT_R4:
                V_R4(pvarg) = va_arg(argList, float);
                break; 
            case VT_DATE:
            case VT_R8:
                V_R8(pvarg) = va_arg(argList, double);
                break;
            case VT_CY:
                V_CY(pvarg) = va_arg(argList, CY);
                break;
            case VT_BSTR:
                V_BSTR(pvarg) = SysAllocString(va_arg(argList, OLECHAR FAR*));
                if (pvarg->bstrVal == NULL) 
                {
                    hr = ResultFromScode(E_OUTOFMEMORY);  
                    pvarg->vt = VT_EMPTY;
                    goto cleanup;  
                }
                break;
            case VT_DISPATCH:
                V_DISPATCH(pvarg) = va_arg(argList, LPDISPATCH);
                break;
            case VT_ERROR:
                V_ERROR(pvarg) = va_arg(argList, SCODE);
                break;
            case VT_BOOL:
                V_BOOL(pvarg) = va_arg(argList, BOOL) ? -1 : 0;
                break;
            case VT_VARIANT:
                *pvarg = va_arg(argList, VARIANTARG); 
                break;
            case VT_UNKNOWN:
                V_UNKNOWN(pvarg) = va_arg(argList, LPUNKNOWN);
                break;

            case VT_I2|VT_BYREF:
                V_I2REF(pvarg) = va_arg(argList, short FAR*);
                break;
            case VT_I4|VT_BYREF:
                V_I4REF(pvarg) = va_arg(argList, long FAR*);
                break;
            case VT_R4|VT_BYREF:
                V_R4REF(pvarg) = va_arg(argList, float FAR*);
                break;
            case VT_R8|VT_BYREF:
                V_R8REF(pvarg) = va_arg(argList, double FAR*);
                break;
            case VT_DATE|VT_BYREF:
                V_DATEREF(pvarg) = va_arg(argList, DATE FAR*);
                break;
            case VT_CY|VT_BYREF:
                V_CYREF(pvarg) = va_arg(argList, CY FAR*);
                break;
            case VT_BSTR|VT_BYREF:
                V_BSTRREF(pvarg) = va_arg(argList, BSTR FAR*);
                break;
            case VT_DISPATCH|VT_BYREF:
                V_DISPATCHREF(pvarg) = va_arg(argList, LPDISPATCH FAR*);
                break;
            case VT_ERROR|VT_BYREF:
                V_ERRORREF(pvarg) = va_arg(argList, SCODE FAR*);
                break;
            case VT_BOOL|VT_BYREF: 
                {
                    BOOL FAR* pbool = va_arg(argList, BOOL FAR*);
                    *pbool = 0;
                    V_BOOLREF(pvarg) = (VARIANT_BOOL FAR*)pbool;
                } 
                break;              
            case VT_VARIANT|VT_BYREF: 
                V_VARIANTREF(pvarg) = va_arg(argList, VARIANTARG FAR*);
                break;
            case VT_UNKNOWN|VT_BYREF:
                V_UNKNOWNREF(pvarg) = va_arg(argList, LPUNKNOWN FAR*);
                break;

            default:
                {
                    hr = ResultFromScode(E_INVALIDARG);
                    goto cleanup;  
                }
                break;
            }

            --pvarg; // get ready to fill next argument
        } //while
    } //if
    
    // Initialize return variant, in case caller forgot. Caller can pass NULL if return
    // value is not expected.
    if (pvRet)
        VariantInit(pvRet); 
    // make the call 
        try {
                hr = pdisp->Invoke(dispid, IID_NULL, LOCALE_USER_DEFAULT, wFlags,
                        &dispparams, pvRet, pexcepinfo, pnArgErr);
        }
        catch (...) {
                hr = E_FAIL;
        }

cleanup:
    // cleanup any arguments that need cleanup
    if (dispparams.cArgs != 0)
    {
        VARIANTARG FAR* pvarg = dispparams.rgvarg;
        UINT cArgs = dispparams.cArgs;   
        
        while (cArgs--)
        {
            switch (pvarg->vt)
            {
            case VT_BSTR:
                VariantClear(pvarg);
                break;
            }
            ++pvarg;
        }
    }
//    delete dispparams.rgvarg;
    va_end(argList);
    return hr;   
}
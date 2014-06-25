#include "stdafx.h"
#include "IAddinImpl.h"
#include "MarshalingHelpers.h"
#include "DispatchHelpers.h"
#include <OleAuto.h>

IAddinImpl::IAddinImpl(ScriptEngine::Machine::Contexts::ScriptDrivenObject^ innerObject) : RefCountable()
{
	m_innerObject = innerObject;
}

IAddinImpl::~IAddinImpl(void)
{
	m_innerObject = nullptr;
}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IAddinMacroses*>(this);
		AddRef();
		return S_OK;
	}
	if (riid == IID_IAddinMacroses)
	{
		*ppObj = static_cast<IAddinMacroses*>(this);
		AddRef();
		return S_OK;
	}
	if (riid == IID_IDispatch)
	{
		*ppObj = static_cast<IDispatch*>(this);
		AddRef();
		return S_OK;
	}
	if (riid == IID_ITypeInfo)
	{
		*ppObj = static_cast<ITypeInfo*>(this);
		AddRef();
		return S_OK;
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
	}
}

ULONG   __stdcall IAddinImpl::AddRef()
{
	return RefCountable::AddRef();
}

ULONG   __stdcall IAddinImpl::Release()
{
	return RefCountable::Release();
}

#pragma endregion
#pragma region IDispatch impl

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeInfoCount( 
	UINT *pctinfo)
{
	*pctinfo = 1;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeInfo( 
	UINT iTInfo,
	LCID lcid,
	ITypeInfo **ppTInfo)
{
	*ppTInfo = static_cast<ITypeInfo*>(this);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetIDsOfNames( 
	REFIID riid,
	LPOLESTR *rgszNames,
	UINT cNames,
	LCID lcid,
	DISPID *rgDispId)
{
	HRESULT res = S_OK;
	for (unsigned int i = 0; i < cNames; i++)
	{
		LPOLESTR name = rgszNames[i];
		try
		{
			rgDispId[i] = m_innerObject->FindMethod(gcnew System::String(name));
		}
		catch(ScriptEngine::Machine::RuntimeException^)
		{
			rgDispId[i] = DISPID_UNKNOWN;
			res = DISP_E_UNKNOWNNAME;
		}
	}

	return res;

}

HRESULT STDMETHODCALLTYPE IAddinImpl::Invoke( 
	DISPID dispIdMember,
	REFIID riid,
	LCID lcid,
	WORD wFlags,
	DISPPARAMS *pDispParams,
	VARIANT *pVarResult,
	EXCEPINFO *pExcepInfo,
	UINT *puArgErr)
{
	array<ScriptEngine::Machine::IValue^>^ scriptArgs = gcnew array<ScriptEngine::Machine::IValue^>(pDispParams->cArgs);
	VARIANT **argWrappers = new VARIANT*[pDispParams->cArgs];
	for (int i = pDispParams->cArgs-1, j = 0; i >= 0; i--, j++)
	{
		VARIANT varArg = pDispParams->rgvarg[j];
		IDispatch* wrapper = V_DISPATCH(&varArg);
		VARIANT valVariant;
		invoke(wrapper, DISPATCH_PROPERTYGET, &valVariant, NULL, NULL, L"val", L"");
		Machine::IValue^ scriptArgValue;
		argWrappers[j] = &valVariant;
		switch (valVariant.vt)
		{
		case VT_DISPATCH:
		{
			Machine::Contexts::COMWrapperContext^ comCtx;
			IntPtr pDisp = IntPtr(V_DISPATCH(&valVariant));
			Object^ obj = Runtime::InteropServices::Marshal::GetObjectForIUnknown(pDisp);
			comCtx = gcnew Machine::Contexts::COMWrapperContext(obj);
			scriptArgValue = Machine::ValueFactory::Create(comCtx);
			break;
		}
		case VT_BSTR:
		{
			IntPtr pBstr = IntPtr(V_BSTR(&valVariant));
			System::String^ str = Runtime::InteropServices::Marshal::PtrToStringBSTR(pBstr);
			scriptArgValue = Machine::ValueFactory::Create(str);
			break;
		}
		case VT_INT:
		{
			double intArg = V_INT(&valVariant);
			scriptArgValue = Machine::ValueFactory::Create(intArg);
			break;
		}
		case VT_R4:
		{
			double intArg = V_R4(&valVariant);
			scriptArgValue = Machine::ValueFactory::Create(intArg);
		}
		case VT_R8:
		{
			double intArg = V_R8(&valVariant);
			scriptArgValue = Machine::ValueFactory::Create(intArg);
			break;
		}
		case VT_BOOL:
		{
			VARIANT_BOOL bArg = V_BOOL(&valVariant);
			scriptArgValue = Machine::ValueFactory::Create((bArg == VARIANT_TRUE) == true);
			break;
		}
		default:
			scriptArgValue = Machine::ValueFactory::Create();
			break;
		}

		Machine::IVariable^ ref = Machine::Variable::Create(scriptArgValue);
		scriptArgs[i] = Machine::Variable::CreateReference(ref);
	}

	try
	{
		m_innerObject->CallAsProcedure(dispIdMember, scriptArgs);

		// back conversion
		for (int i = pDispParams->cArgs-1, j = 0; i >=0; i--, j++)
		{
			Machine::IValue^ scriptArgValue = scriptArgs[i]->GetRawValue();
			VARIANT varVal;
			switch (scriptArgValue->DataType)
			{
			case Machine::DataType::Boolean:
				{
					V_VT(&varVal) = VT_BOOL;
					V_BOOL(&varVal) = scriptArgValue->AsBoolean() == true? VARIANT_TRUE:VARIANT_FALSE;
					break;
				}
			case Machine::DataType::Number:
				{
					switch (argWrappers[j]->vt)
					{
					case VT_INT:
					case VT_I2:
					case VT_I4:
					case VT_I8:
						V_VT(&varVal) = VT_INT;
						V_INT(&varVal) = (int)scriptArgValue->AsNumber();
						break;
					default:
						V_VT(&varVal) = VT_R8;
						V_R8(&varVal) = scriptArgValue->AsNumber();
						break;
					}
					break;
				}
			case Machine::DataType::Date:
				{
					V_VT(&varVal) = VT_DATE;
					V_DATE(&varVal) = scriptArgValue->AsDate().ToOADate();
					break;
				}
			case Machine::DataType::String:
				{
					V_VT(&varVal) = VT_BSTR;
					IntPtr pBstr = Runtime::InteropServices::Marshal::StringToBSTR(scriptArgValue->AsString());
					V_BSTR(&varVal) = (BSTR)(void*)pBstr;
					break;
				}
			case Machine::DataType::Object:
				{
					Machine::Contexts::COMWrapperContext^ ctx = safe_cast<Machine::Contexts::COMWrapperContext^>(scriptArgValue->AsObject());
					if(ctx != nullptr)
					{
						V_VT(&varVal) = VT_DISPATCH;
						V_DISPATCH(&varVal) = (IDispatch*)(void*)Runtime::InteropServices::Marshal::GetIDispatchForObject(ctx->UnderlyingObject);
					}

					break;
				}
			default:
				continue;
			}

			invoke(pDispParams->rgvarg[j].pdispVal, DISPATCH_PROPERTYPUT, NULL, NULL, NULL, L"val", L"v", varVal);
			
		}

		delete[] argWrappers;

	}
	catch(System::Exception^ e)
	{
		auto buf = stringBuf(e->ToString());
		MessageBox(0, buf, L"Error", MB_OK);
		delete[] buf;
		delete[] argWrappers;
	}

	return S_OK;
}

#pragma endregion
        
HRESULT STDMETHODCALLTYPE IAddinImpl::macroses(SAFEARRAY **result)
{
	String^ prefix = L"Макрос_";
	
	array<System::String^>^ macrosArray = m_innerObject->GetExportedMethods();
	int macroCount = 0;
	for each (String^ name in macrosArray)
	{
		if(name->StartsWith(prefix))
		{
			macroCount++;
		}
	}
	SAFEARRAYBOUND  Bound[1];
    Bound[0].lLbound   = 0;
	Bound[0].cElements = macroCount;
	*result = SafeArrayCreate(VT_VARIANT, 1, Bound);
	LONG idx[1];

	for (int i = 0, j = 0; i < macrosArray->Length; i++)
	{
		if(macrosArray[i]->StartsWith(prefix) && macrosArray[i]->Length > prefix->Length)
		{
			WCHAR* buf = stringBuf(macrosArray[i]->Substring(prefix->Length));
			BSTR allocString = SysAllocString(buf);
			delete[] buf;
		
			VARIANT val;
			V_VT(&val) = VT_BSTR;
			V_BSTR(&val) = allocString;

			idx[0] = j++;
			HRESULT hr = SafeArrayPutElement(*result, idx, &val);
		}
	}

	return S_OK;

}
        
HRESULT STDMETHODCALLTYPE IAddinImpl::invokeMacros( 
    BSTR MacrosName,
    VARIANT *result)
{
	String^ prefix = L"Макрос_";
	System::String^ strMacro = prefix + (gcnew System::String(MacrosName));

	try
	{
		int mNum = m_innerObject->FindMethod(strMacro);
		auto mInfo = m_innerObject->GetMethodInfo(mNum);
		// пока без возвратов
		m_innerObject->CallAsProcedure(mNum, gcnew array<ScriptEngine::Machine::IValue^, 1>(0));
	}
	catch(System::Exception^ e)
	{
		auto buf = stringBuf(e->ToString());
		MessageBox(0, buf, L"Error", MB_OK);
		delete[] buf;
	}

	return S_OK;

}

void IAddinImpl::OnZeroCount()
{
	m_innerObject = nullptr;
}

#pragma region ITypeInfo members

HRESULT STDMETHODCALLTYPE IAddinImpl::GetTypeAttr(TYPEATTR **ppTypeAttr)
{
	TYPEATTR* ta = new TYPEATTR();
	memset(ta, 0, sizeof(TYPEATTR));

	array<System::String^>^ exportNames = m_innerObject->GetExportedMethods();
	m_exportedMeths = gcnew array<ScriptEngine::Machine::MethodInfo>(exportNames->Length);
	
	for (int i = 0; i < m_exportedMeths->Length; i++)
	{
		int mId = m_innerObject->FindMethod(exportNames[i]);
		m_exportedMeths[i] = m_innerObject->GetMethodInfo(mId);
	}

	ta->cFuncs = m_exportedMeths->Length;
	ta->typekind = TKIND_DISPATCH;
	*ppTypeAttr = ta;

	return S_OK;
}

HRESULT STDMETHODCALLTYPE IAddinImpl::GetFuncDesc( 
        UINT index,
        FUNCDESC **ppFuncDesc)
{
	
	auto mi = m_exportedMeths[index];

	FUNCDESC* fd = new FUNCDESC();
	memset(fd, 0, sizeof(FUNCDESC));
	fd->memid = index;
	fd->funckind = FUNC_DISPATCH;
	fd->invkind = INVOKE_FUNC;
	fd->cParams = mi.Params->Length;

	*ppFuncDesc = fd;

	return S_OK;
}

#pragma endregion
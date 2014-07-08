#include "stdafx.h"
#include "SelfScriptIDispatch.h"
#include "MarshalingHelpers.h"
#include "DispatchHelpers.h"

SelfScriptIDispatch::SelfScriptIDispatch(ScriptEngine::Machine::Contexts::ScriptDrivenObject^ self)
{
	m_scriptDrivenObject = self;
}


SelfScriptIDispatch::~SelfScriptIDispatch(void)
{
	m_exportedMeths = nullptr;
	m_scriptDrivenObject = nullptr;
}

ScriptEngine::Machine::IValue^ SelfScriptIDispatch::ConvertVariantToScriptArg(VARIANT valVariant)
{
	ScriptEngine::Machine::IValue^ scriptArgValue;

	switch (valVariant.vt)
	{
	case VT_DISPATCH:
	{
		ScriptEngine::Machine::Contexts::COMWrapperContext^ comCtx;
		System::IntPtr pDisp = System::IntPtr(V_DISPATCH(&valVariant));
		System::Object^ obj = System::Runtime::InteropServices::Marshal::GetObjectForIUnknown(pDisp);
		comCtx = gcnew ScriptEngine::Machine::Contexts::COMWrapperContext(obj);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create(comCtx);
		break;
	}
	case VT_BSTR:
	{
		System::IntPtr pBstr = System::IntPtr(V_BSTR(&valVariant));
		System::String^ str = System::Runtime::InteropServices::Marshal::PtrToStringBSTR(pBstr);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create(str);
		break;
	}
	case VT_INT:
	{
		double intArg = V_INT(&valVariant);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create(intArg);
		break;
	}
	case VT_R4:
	{
		double intArg = V_R4(&valVariant);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create(intArg);
	}
	case VT_R8:
	{
		double intArg = V_R8(&valVariant);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create(intArg);
		break;
	}
	case VT_BOOL:
	{
		VARIANT_BOOL bArg = V_BOOL(&valVariant);
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create((bArg == VARIANT_TRUE) == true);
		break;
	}
	default:
		scriptArgValue = ScriptEngine::Machine::ValueFactory::Create();
		break;
	}

	return scriptArgValue;

}

VARIANT SelfScriptIDispatch::ConvertScriptArgToVariant(ScriptEngine::Machine::IValue^ scriptArgValue, VARTYPE initialType)
{
	VARIANT varVal;
	VariantInit(&varVal);

	switch (scriptArgValue->DataType)
	{
	case ScriptEngine::Machine::DataType::Boolean:
		{
			V_VT(&varVal) = VT_BOOL;
			V_BOOL(&varVal) = scriptArgValue->AsBoolean() == true? VARIANT_TRUE:VARIANT_FALSE;
			break;
		}
	case ScriptEngine::Machine::DataType::Number:
		{
			switch (initialType)
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
	case ScriptEngine::Machine::DataType::Date:
		{
			V_VT(&varVal) = VT_DATE;
			V_DATE(&varVal) = scriptArgValue->AsDate().ToOADate();
			break;
		}
	case ScriptEngine::Machine::DataType::String:
		{
			V_VT(&varVal) = VT_BSTR;
			System::IntPtr pBstr = System::Runtime::InteropServices::Marshal::StringToBSTR(scriptArgValue->AsString());
			V_BSTR(&varVal) = (BSTR)(void*)pBstr;
			break;
		}
	case ScriptEngine::Machine::DataType::Object:
		{
			ScriptEngine::Machine::Contexts::COMWrapperContext^ ctx = safe_cast<ScriptEngine::Machine::Contexts::COMWrapperContext^>(scriptArgValue->AsObject());
			if(ctx != nullptr)
			{
				V_VT(&varVal) = VT_DISPATCH;
				V_DISPATCH(&varVal) = (IDispatch*)(void*)System::Runtime::InteropServices::Marshal::GetIDispatchForObject(ctx->UnderlyingObject);
			}

			break;
		}
	default:
		break;
	}

	return varVal;

}

#pragma region IUnknown implementation

HRESULT __stdcall SelfScriptIDispatch::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IDispatch*>(this);
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

ULONG   __stdcall SelfScriptIDispatch::AddRef()
{
	return RefCountable::AddRef();
}

ULONG   __stdcall SelfScriptIDispatch::Release()
{
	return RefCountable::Release();
}

#pragma endregion


#pragma region IDispatch impl

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::GetTypeInfoCount( 
	UINT *pctinfo)
{
	*pctinfo = 1;
	return S_OK;
}

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::GetTypeInfo( 
	UINT iTInfo,
	LCID lcid,
	ITypeInfo **ppTInfo)
{
	*ppTInfo = static_cast<ITypeInfo*>(this);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::GetIDsOfNames( 
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
			rgDispId[i] = m_scriptDrivenObject->FindMethod(gcnew System::String(name));
		}
		catch(ScriptEngine::Machine::RuntimeException^)
		{
			rgDispId[i] = DISPID_UNKNOWN;
			res = DISP_E_UNKNOWNNAME;
		}
	}

	return res;

}

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::Invoke( 
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
	IParamsWrapper **argWrappers = new IParamsWrapper*[pDispParams->cArgs];
	for (size_t i = 0; i < pDispParams->cArgs; i++)
	{
		argWrappers[i] = NULL;
	};

	VARTYPE *argTypes = new VARTYPE[pDispParams->cArgs];

	bool wrappersByDispatch = false;

	for (int i = pDispParams->cArgs-1, j = 0; i >= 0; i--, j++)
	{
		VARIANT varArg = pDispParams->rgvarg[j];
		ScriptEngine::Machine::IValue^ scriptArgValue;

		if(V_VT(&varArg) == VT_DISPATCH)
		{
			IDispatch* wrapper = V_DISPATCH(&varArg);
			IParamsWrapper* pWrapperQueried;
			if(wrapper->QueryInterface(IID_IParamsWrapper, (void**)&pWrapperQueried) == S_OK)
			{
				argWrappers[j] = pWrapperQueried;
				VARIANT vScriptArg;
				VariantInit(&vScriptArg);
				pWrapperQueried->get_val(&vScriptArg);
				scriptArgValue = ConvertVariantToScriptArg(vScriptArg);
			}
			else
			{
				// проверка на wrapper иным способом
				VARIANT vScriptArg;
				HRESULT hr = invoke(V_DISPATCH(&varArg), DISPATCH_PROPERTYGET, &vScriptArg, NULL, NULL, L"val", L"");
				if(hr == S_OK)
				{
					wrappersByDispatch = true;
					scriptArgValue = ConvertVariantToScriptArg(vScriptArg);
					argWrappers[j] = (IParamsWrapper *)V_DISPATCH(&varArg);
					argTypes[j] = V_VT(&varArg);
				}
				else
				{	
					scriptArgValue = ConvertVariantToScriptArg(varArg);
				}
			}
		}
		else
		{
			scriptArgValue = ConvertVariantToScriptArg(varArg);
		}
				
		ScriptEngine::Machine::IVariable^ ref = ScriptEngine::Machine::Variable::Create(scriptArgValue);
		scriptArgs[i] = ScriptEngine::Machine::Variable::CreateReference(ref);
	}

	try
	{
		m_scriptDrivenObject->CallAsProcedure(dispIdMember, scriptArgs);
		
		// back conversion
		for (int i = pDispParams->cArgs-1, j = 0; i >=0; i--, j++)
		{
			if(argWrappers[j] != NULL)
			{
				if(wrappersByDispatch)
				{
					VARIANT varVal = ConvertScriptArgToVariant(scriptArgs[i], argTypes[j]);
					invoke(pDispParams->rgvarg[j].pdispVal, DISPATCH_PROPERTYPUT, NULL, NULL, NULL, L"val", L"v", varVal);
				}
				else
				{
					VARIANT varVal = ConvertScriptArgToVariant(scriptArgs[i], argTypes[j]);
					argWrappers[j]->put_val(varVal);
					argWrappers[j]->Release();
				}
			}

		}

		delete[] argWrappers;
		delete[] argTypes;

	}
	catch(System::Exception^ e)
	{
		auto buf = stringBuf(e->ToString());
		MessageBox(0, buf, L"Error", MB_OK);
		delete[] buf;
		
		delete[] argWrappers;
		delete[] argTypes;
	}

	return S_OK;
}

#pragma endregion


#pragma region ITypeInfo members

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::GetTypeAttr(TYPEATTR **ppTypeAttr)
{
	TYPEATTR* ta = new TYPEATTR();
	memset(ta, 0, sizeof(TYPEATTR));

	array<System::String^>^ exportNames = m_scriptDrivenObject->GetExportedMethods();
	m_exportedMeths = gcnew array<ScriptEngine::Machine::MethodInfo>(exportNames->Length);
	
	for (int i = 0; i < m_exportedMeths->Length; i++)
	{
		int mId = m_scriptDrivenObject->FindMethod(exportNames[i]);
		m_exportedMeths[i] = m_scriptDrivenObject->GetMethodInfo(mId);
	}

	ta->cFuncs = m_exportedMeths->Length;
	ta->typekind = TKIND_DISPATCH;
	*ppTypeAttr = ta;

	return S_OK;
}

HRESULT STDMETHODCALLTYPE SelfScriptIDispatch::GetFuncDesc( 
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
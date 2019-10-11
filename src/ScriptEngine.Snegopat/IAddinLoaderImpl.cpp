#include "stdafx.h"
#include "IAddinLoaderImpl.h"
#include "Snegopat_i.c"
#include "MarshalingHelpers.h"
#include "DispatchHelpers.h"
#include "ScriptDrivenAddin.h"
#include <commdlg.h>
#include <string>

IAddinLoaderImpl::IAddinLoaderImpl(IDispatch* pDesigner) : RefCountable()
{
	m_pDesigner = pDesigner;
	m_pDesigner->AddRef();
	m_engine = gcnew ScriptEngine::ScriptingEngine();

	ScriptEngine::RuntimeEnvironment^ env = gcnew ScriptEngine::RuntimeEnvironment();
	
	IntPtr handle = IntPtr(pDesigner); 
	Object^ managedObject = System::Runtime::InteropServices::Marshal::GetObjectForIUnknown(handle);
	IRuntimeContextInstance^ designerWrapper = Contexts::COMWrapperContext::Create(managedObject);

	env->InjectGlobalProperty((IValue^)designerWrapper, L"Designer", true);
	env->InjectGlobalProperty((IValue^)designerWrapper, L"������������", true);

	SnegopatAttachedContext^ importedProperties = gcnew SnegopatAttachedContext(designerWrapper);
	TypeManager::NewInstanceHandler = importedProperties->GetType();
	env->InjectObject(importedProperties, true);

	LibraryAttachedContext^ stdLib = gcnew LibraryAttachedContext(m_engine);
	env->InjectObject(stdLib);
	m_engine->Environment = env;
	m_engine->Initialize();

}

IAddinLoaderImpl::~IAddinLoaderImpl(void)
{
	if(nullptr != (ScriptEngine::ScriptingEngine^)m_engine)
	{
		m_engine = nullptr;
		m_pDesigner = NULL;
	}
}

void IAddinLoaderImpl::OnZeroCount()
{
	m_pDesigner->Release();
}

ScriptDrivenAddin^ IAddinLoaderImpl::LoadFromScriptFile(String^ path, addinNames* names)
{	
	String^ strDisplayName = nullptr;
	String^ strUniqueName = nullptr;

	System::IO::StreamReader^ rd = nullptr;
	
	rd = ScriptEngine::Environment::FileOpener::OpenReader(path);
	ScriptDrivenAddin^ scriptObject = nullptr;

	try
	{
		Char ch = rd->Peek();
		while(ch > -1 && ch == '$') 
		{
			String^ macro = rd->ReadLine();
			if(macro->Length > 0)
			{
				array<String^>^ parts = macro->Split(gcnew array<Char>(2){' ', '\t'}, 2);
				parts[0] = parts[0]->Trim();
				parts[1] = parts[1]->Trim();
				if(parts->Length < 2)
				{
					continue;
				}

				if(parts[0] == "$uname")
				{
					strUniqueName = parts[1];
				}
				else if(parts[0] == "$dname")
				{
					strDisplayName = parts[1];
				}
			}
			ch = rd->Peek();
		}

		if(!rd->EndOfStream)
		{
			if(strDisplayName == nullptr)
				strDisplayName = System::IO::Path::GetFileNameWithoutExtension(path);
			if(strUniqueName == nullptr)
				strUniqueName = System::IO::Path::GetFileNameWithoutExtension(path);

			names->displayName = stringToBSTR(strDisplayName);
			names->uniqueName = stringToBSTR(strUniqueName);
			
			String^ code = rd->ReadToEnd();
			ICodeSource^ src = m_engine->Loader->FromString(code);
			CompilerService^ compiler = m_engine->GetCompilerService();
			
			String^ thisName = L"����������";
			compiler->DefineVariable(thisName, SymbolType::ContextProperty);
			ScriptEngine::Environment::ScriptModuleHandle mHandle = compiler->CreateModule(src);
			LoadedModuleHandle mh = m_engine->LoadModuleImage(mHandle);

			scriptObject = gcnew ScriptDrivenAddin(mh);
			scriptObject->AddProperty(thisName, scriptObject);
			scriptObject->InitOwnData();

		}
	}
	finally
	{
		delete rd;
	}

	return scriptObject;
}

ScriptDrivenAddin^ IAddinLoaderImpl::LoadFromDialog(String^ path, addinNames* names)
{
	IDispatch* files = NULL;
	IDispatch* storage = NULL;
	IDispatch* intFile = NULL;
	
	BSTR tmp_bstr = NULL;

	HRESULT hr;

	VARIANT retVal;
	hr = invoke(m_pDesigner, DISPATCH_PROPERTYGET, &retVal, NULL, NULL, L"v8files", NULL);
	if(SUCCEEDED(hr))
		files = V_DISPATCH(&retVal);
	else
		return nullptr;

	String^ uri = L"file://" + path;
	tmp_bstr = stringToBSTR(uri);
	try
	{
		hr = invoke(files, DISPATCH_METHOD, &retVal, NULL, NULL, L"open", L"si", tmp_bstr, 8);
	
		if(SUCCEEDED(hr))
			intFile = V_DISPATCH(&retVal);
		else
			return nullptr;

		hr = invoke(files, DISPATCH_METHOD, &retVal, NULL, NULL, L"attachStorage", L"U", intFile);

		if(SUCCEEDED(hr))
			storage = V_DISPATCH(&retVal);
		else
			return nullptr;

		hr = invoke(storage, DISPATCH_METHOD, &retVal, NULL, NULL, L"open", L"si", L"module", 8);

		IDispatch* moduleFile = NULL;
		if(SUCCEEDED(hr))
			moduleFile = V_DISPATCH(&retVal);
		else
			return nullptr;

		hr = invoke(moduleFile, DISPATCH_METHOD, &retVal, NULL, NULL, L"getString", L"i", 2);
		moduleFile->Release();
		storage->Release();
		storage = NULL;
		invoke(files, DISPATCH_METHOD, NULL, NULL, NULL, L"close", L"U", intFile);
		intFile->Release();
		intFile = NULL;

		String^ code;
		if(SUCCEEDED(hr))
		{
			tmp_bstr = V_BSTR(&retVal);
		}
		else
			return nullptr;
		
		if(tmp_bstr[0] == 65279) // � ��������� ������ ���������� ���� ������ ������.
			tmp_bstr[0] = ' ';

		code = gcnew String(tmp_bstr);

		ICodeSource^ src = m_engine->Loader->FromString(code);
		CompilerService^ compiler = m_engine->GetCompilerService();
		String^ thisProp = L"����������";
		String^ formProp = L"��������";
		
		compiler->DefineVariable(thisProp, SymbolType::ContextProperty);
		compiler->DefineVariable(formProp, SymbolType::ContextProperty);
		LoadedModuleHandle mh = m_engine->LoadModuleImage(compiler->CreateModule(src));

		ScriptDrivenAddin^ scriptObject = gcnew ScriptDrivenAddin(mh);

		SysFreeString(tmp_bstr);
		tmp_bstr = stringToBSTR(path);
		
		Object^ managedDesigner = Marshal::GetObjectForIUnknown(IntPtr(m_pDesigner));
		array<Object^>^ args = gcnew array<Object^>(2)
		{
			gcnew String(tmp_bstr),
			scriptObject->UnderlyingObject
		};

		Object^ form = nullptr;
		try
		{
			form = managedDesigner->GetType()->InvokeMember("loadScriptForm", 
			System::Reflection::BindingFlags::InvokeMethod,
			nullptr,
			managedDesigner,
			args);
		}
		finally
		{
			Marshal::ReleaseComObject(managedDesigner);
		}

		scriptObject->AddProperty(thisProp, scriptObject);
		scriptObject->AddProperty(formProp, COMWrapperContext::Create(form));
		scriptObject->InitOwnData();

		String^ title = safe_cast<String^>(form->GetType()->InvokeMember(L"���������", System::Reflection::BindingFlags::GetProperty, nullptr, form, nullptr));
		if(String::IsNullOrWhiteSpace(title))
		{
			names->displayName = stringToBSTR(System::IO::Path::GetFileNameWithoutExtension(path));
			names->uniqueName = stringToBSTR(System::IO::Path::GetFileNameWithoutExtension(path));
		}
		else
		{
			auto split = title->Split('/');
			if(split->Length > 1)
			{
				names->uniqueName = stringToBSTR(split[0]);
				names->displayName = stringToBSTR(split[1]);
				array<String^>^ titleArgs = gcnew array<String^>(1);
				titleArgs[0] = split[1];
				form->GetType()->InvokeMember(L"���������", System::Reflection::BindingFlags::SetProperty, nullptr, form, titleArgs);
			}
			else
			{
				names->displayName = stringToBSTR(title);
				names->uniqueName = stringToBSTR(System::IO::Path::GetFileNameWithoutExtension(path));
			}
		}
		
		return scriptObject;

	}
	finally
	{
		if(storage != NULL)
			storage->Release();
		if(intFile != NULL)
			intFile->Release();

		files->Release();

		if(tmp_bstr != NULL)
			SysFreeString(tmp_bstr);
	}

}

//IUnknown interface 
#pragma region IUnknown implementation

HRESULT __stdcall IAddinLoaderImpl::QueryInterface(
	REFIID riid , 
	void **ppObj)
{
	if(riid == IID_IAddinLoader)
	{
		*ppObj = static_cast<IAddinLoader*>(this);
		AddRef();
		return S_OK;
	}
	else if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IAddinLoader*>(this);
		AddRef();
		return S_OK;
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
	}
}

ULONG   __stdcall IAddinLoaderImpl::AddRef()
{
	return RefCountable::AddRef();
}

ULONG   __stdcall IAddinLoaderImpl::Release()
{
	return RefCountable::Release();
}

#pragma endregion

#pragma region IAddinLoader implementation

HRESULT __stdcall  IAddinLoaderImpl::proto( 
	BSTR *result)
{
	*result = SysAllocString(L"1clang");
	return S_OK;
}

HRESULT __stdcall  IAddinLoaderImpl::load( 
	BSTR uri,
	BSTR *fullPath,
	BSTR *uniqueName,
	BSTR *displayName,
	IUnknown **result)
{
	String^ strDisplayName = nullptr;
	String^ strUniqueName = nullptr;

	HRESULT res = E_FAIL;

	std::wstring wsUri = uri;
	int pos = wsUri.find_first_of(':', 0);
	if(pos != std::wstring::npos)
	{
		String^ path = gcnew String(wsUri.substr(pos+1).c_str());
		String^ protocol = gcnew String(wsUri.substr(0, pos+1).c_str());
		path = System::IO::Path::GetFullPath(path);
		String^ extension = System::IO::Path::GetExtension(path)->ToLower();
		ScriptDrivenAddin^ scriptObject;
		addinNames names;
		try
		{
			if(extension == ".os" || extension == ".1scr")
			{
				scriptObject = LoadFromScriptFile(path, &names);
			}
			else if(extension == ".osf" || extension == ".ssf")
			{
				scriptObject = LoadFromDialog(path, &names);
				res = S_OK;
			}
			else
			{
				return E_FAIL;
			}

			if(scriptObject == nullptr)
				return E_FAIL;

			*displayName = names.displayName;
			*uniqueName = names.uniqueName;
			*fullPath = stringToBSTR(protocol+path);

			IAddinImpl* snegopatAddin = new IAddinImpl(scriptObject);
			m_engine->InitializeSDO(scriptObject);

			snegopatAddin->SetNames(*uniqueName, *displayName, *fullPath);
			snegopatAddin->QueryInterface(IID_IUnknown, (void**)result);

			res = S_OK;
		}
		catch(Exception^ e)
		{
			WCHAR* msg = stringBuf(e->Message);
			MessageBox(0, msg, L"Load error", MB_ICONERROR);
			delete[] msg;

			res = E_FAIL;
		}
		
	}

	return res;

}

HRESULT __stdcall  IAddinLoaderImpl::canUnload( 
	BSTR fullPath,
	IUnknown *addin,
	VARIANT_BOOL *result)
{
	*result = VARIANT_TRUE;
	return S_OK;
}

HRESULT __stdcall  IAddinLoaderImpl::unload( 
	BSTR fullPath,
	IUnknown *addin,
	VARIANT_BOOL *result)
{
	*result = VARIANT_TRUE;
	return S_OK;
}

HRESULT __stdcall  IAddinLoaderImpl::loadCommandName( 
	BSTR *result)
{
	*result = SysAllocString(L"��������� ������ 1�|1clang");
	return S_OK;
}

HRESULT __stdcall  IAddinLoaderImpl::selectLoadURI( 
	BSTR *result)
{
	OPENFILENAME ofn;
	const int PREFIX_LEN = 7;
	const int BUFFER_SIZE = PREFIX_LEN + MAX_PATH + 1;
	WCHAR pUri[BUFFER_SIZE];
	memset(pUri, 0, (BUFFER_SIZE) * sizeof(WCHAR));
	wcsncat_s(pUri, L"1clang:", PREFIX_LEN);
	WCHAR* file = pUri + PREFIX_LEN;

	memset(&ofn,0,sizeof(OPENFILENAME));
	ofn.lStructSize = sizeof(OPENFILENAME);
	ofn.lpstrFilter = L"�������/����� OneScript\0*.1scr;*.os;*.osf\0������� 1� (*.os; *.1scr)\0*.os;*.1scr\0����� 1� (*.osf; *.ssf)\0*.osf;*.ssf\0��� �����\0*.*\0\0";
	ofn.lpstrFile = file;
	ofn.nMaxFile = MAX_PATH;
	ofn.Flags = OFN_EXPLORER|OFN_FILEMUSTEXIST;
	if(GetOpenFileName(&ofn))
	{
		*result = SysAllocStringLen(pUri, MAX_PATH);
		return S_OK;
	}
	else
	{
		return E_ABORT;
	}
}

#pragma endregion

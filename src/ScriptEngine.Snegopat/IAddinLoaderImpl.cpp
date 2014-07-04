#include "stdafx.h"
#include "IAddinLoaderImpl.h"
#include "Snegopat_i.c"
#include "MarshalingHelpers.h"
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
	IRuntimeContextInstance^ designerWrapper = gcnew Contexts::COMWrapperContext(managedObject);

	String^ ident = L"Designer";
	env->InjectGlobalProperty((IValue^)designerWrapper, ident, true);

	SnegopatAttachedContext^ importedProperties = gcnew SnegopatAttachedContext(designerWrapper);
	env->InjectObject(importedProperties, true);

	m_engine->Initialize(env);

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

	HRESULT res;

	std::wstring wsUri = uri;
	int pos = wsUri.find_first_of(':', 0);
	if(pos != std::wstring::npos)
	{
		//std::wstring proto = wsUri.substr(0, pos);
		String^ path = gcnew String(wsUri.substr(pos+1).c_str());
		System::IO::StreamReader^ rd = nullptr;
		try
		{
			rd = ScriptEngine::Environment::FileOpener::OpenReader(path);

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

				*displayName = stringToBSTR(strDisplayName);
				*uniqueName = stringToBSTR(strUniqueName);
				*fullPath = SysAllocString(uri);

				String^ code = rd->ReadToEnd();
				ICodeSource^ src = m_engine->Loader->FromString(code);
				CompilerService^ compiler = m_engine->GetCompilerService();
				compiler->DefineVariable(L"ЭтотОбъект", SymbolType::ContextProperty);
				LoadedModuleHandle mh = m_engine->LoadModuleImage(compiler->CreateModule(src));

				ScriptDrivenAddin^ scriptObject = gcnew ScriptDrivenAddin(mh);
				IAddinImpl* snegopatAddin = new IAddinImpl(scriptObject);
				m_engine->InitializeSDO(scriptObject);

				snegopatAddin->SetNames(*uniqueName, *displayName, *fullPath);
				snegopatAddin->QueryInterface(IID_IUnknown, (void**)result);

			}

			res = S_OK;

		}
		catch(Exception^ e)
		{
			WCHAR* msg = stringBuf(e->Message);
			MessageBox(0, msg, L"Load error", MB_OK);
			delete[] msg;

			res = E_FAIL;
		}
		finally
		{
			if(rd != nullptr)
			{
				rd->Close();
				rd = nullptr;
			}
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
	//addin->Release();
	//  Непонятно: Снегопат передает сюда addIn, который уже уничтожен по счетчику ссылок
	//  при создании аддина загрузчик создает ссылку, вызывая AddRef (см. метод load)
	//  при выгрузке Снегопат вызывает AddIn->Release(), хотя ответного AddRef не делал.
	//	В результате он отпускает ссылку, которой не владеет (созданную загрузчиком в методе load)

	*result = VARIANT_TRUE;
	return S_OK;
}

HRESULT __stdcall  IAddinLoaderImpl::loadCommandName( 
	BSTR *result)
{
	*result = SysAllocString(L"Загрузить скрипт 1С|1clang");
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
	ofn.lpstrFilter = L"Скрипты 1С\0*.1scr\0Все файлы\0*.*\0\0";
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

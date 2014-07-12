#include "stdafx.h"
#include "ScriptDrivenAddin.h"


ScriptDrivenAddin::ScriptDrivenAddin(LoadedModuleHandle module) 
	: ScriptDrivenObject(module)
{
	m_marshalledReference = gcnew EventCallableSDO(this, module);
}

ScriptDrivenAddin::~ScriptDrivenAddin()
{

}

Object^ ScriptDrivenAddin::UnderlyingObject::get()
{
	return m_marshalledReference;
}

int ScriptDrivenAddin::FindOwnProperty(String^ name)
{
	if(name->ToLower() == L"ЭтотОбъект")
	{
		return 0;
	}
	else
	{
		return -1;
	}
}
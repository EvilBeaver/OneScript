#include "stdafx.h"
#include "ScriptDrivenAddin.h"


ScriptDrivenAddin::ScriptDrivenAddin(LoadedModuleHandle module) 
	: ScriptDrivenObject(module, true)
{
	m_marshalledReference = gcnew EventCallableSDO(this, module);
	m_valuesByIndex = gcnew List<IValue^>();
	m_names = gcnew Dictionary<String^, int>(StringComparer::InvariantCultureIgnoreCase);
}

ScriptDrivenAddin::~ScriptDrivenAddin()
{
	if(m_valuesByIndex != nullptr)
	{
		for (int i = 0; i < m_valuesByIndex->Count; i++)
		{
			if(Marshal::IsComObject(m_valuesByIndex[i]))
			{
				Marshal::ReleaseComObject(m_valuesByIndex[i]);
			}
		}
		m_valuesByIndex->Clear();
		m_names->Clear();
		m_valuesByIndex = nullptr;
	}
}

Object^ ScriptDrivenAddin::UnderlyingObject::get()
{
	return m_marshalledReference;
}

void ScriptDrivenAddin::AddProperty(String^ name, IValue^ value)
{
	int newIndex = m_valuesByIndex->Count;
	m_valuesByIndex->Add(value);
	m_names->Add(name, newIndex);
}

int ScriptDrivenAddin::FindOwnProperty(String^ name)
{
	int index = -1;
	if(m_names->TryGetValue(name, index))
	{
		return index;
	}
	else
	{
		return -1;
	}
}

IValue^ ScriptDrivenAddin::GetOwnPropValue(int index)
{
	return m_valuesByIndex[index];
}
#include "stdafx.h"
#include "SnegopatAttachedContext.h"


SnegopatAttachedContext::SnegopatAttachedContext(IRuntimeContextInstance^ Designer)
{
	m_DesignerWrapper = Designer;
	m_varList = gcnew List<IVariable^>();
	m_nameList = gcnew List<String^>();
	m_methods = gcnew List<MethodInfo>();
	m_propDispIdMap = gcnew Dictionary<int,int>();
	m_methDispIdMap = gcnew Dictionary<int,int>();

	InsertProperty("addins");
	InsertProperty("cmdTrace");
	InsertProperty("events");
	InsertProperty("profileRoot");
	InsertProperty("snegopat");
	InsertProperty("hotkeys");
	InsertProperty("windows");
	InsertProperty("metadata");
	InsertProperty("v8files");
	InsertProperty("ownerName");
	InsertProperty("v8debug");
	InsertProperty("sVersion");
	InsertProperty("v8Version");

	InsertMethod("v8New");
	InsertMethod("Message");
	InsertMethod("MessageBox");
	InsertMethod("globalContext");
	InsertMethod("getCmdState");
	InsertMethod("saveProfile");
	InsertMethod("createTimer");
	InsertMethod("killTimer");
	InsertMethod("toV8Value");
	InsertMethod("loadResourceString");


}

void SnegopatAttachedContext::OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance)
{
	instance = this;
	methods = m_methods->ToArray();
	variables = m_varList->ToArray();
}

IEnumerable<VariableInfo>^ SnegopatAttachedContext::GetProperties()
{
	array<VariableInfo>^ arr = gcnew array<VariableInfo>(m_varList->Count);
	for (int i = 0; i < m_varList->Count; i++)
	{
		VariableInfo vi;

		vi.Identifier = m_nameList[i];
		vi.Type = SymbolType::ContextProperty;
		arr[i] = vi;
	}

	return arr;

}

IEnumerable<MethodInfo>^ SnegopatAttachedContext::GetMethods()
{
	array<MethodInfo>^ arr = gcnew array<MethodInfo>(m_methods->Count);
	for (int i = 0; i < m_methods->Count; i++)
	{
		arr[i] = m_methods[i];
	}

	return arr;
}

void SnegopatAttachedContext::InsertProperty(String^ name)
{
	int index = m_nameList->Count;
	int propNum = m_DesignerWrapper->FindProperty(name);
	m_propDispIdMap->Add(index, propNum);
	m_varList->Add(Variable::CreateContextPropertyReference(this, index));
	m_nameList->Add(name);
}

void SnegopatAttachedContext::InsertMethod(String^ name)
{
	int index = m_methods->Count;
	int mNum = m_DesignerWrapper->FindMethod(name);
	MethodInfo mi = m_DesignerWrapper->GetMethodInfo(mNum);
	m_methods->Add(mi);
	m_methDispIdMap->Add(index, mNum);
}

int SnegopatAttachedContext::FindProperty(String^ name) 
{
	for (int i = 0; i < m_nameList->Count; i++)
	{
		String^ nameFromList = m_nameList[i];
		if(String::Compare(nameFromList, name, true) == 0)
		{
			return i;
		}
	}

	throw RuntimeException::PropNotFoundException(name);
}

bool SnegopatAttachedContext::IsPropReadable(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->IsPropReadable(dispId);
}

bool SnegopatAttachedContext::IsPropWritable(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->IsPropWritable(dispId);
}

IValue^ SnegopatAttachedContext::GetPropValue(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->GetPropValue(dispId);
}

void SnegopatAttachedContext::SetPropValue(int propNum, IValue^ val) 
{
	int dispId = m_propDispIdMap[propNum];
	m_DesignerWrapper->SetPropValue(dispId, val);
}

int SnegopatAttachedContext::FindMethod(String^ mName) 
{
	for (int i = 0; i < m_methods->Count; i++)
	{
		String^ nameFromList = m_methods[i].Name;
		if(String::Compare(nameFromList, mName, true) == 0)
		{
			return i;
		}
	}

	throw RuntimeException::MethodNotFoundException(mName);
}

MethodInfo SnegopatAttachedContext::GetMethodInfo(int mNum) 
{
	int dispId = m_methDispIdMap[mNum];
	return m_DesignerWrapper->GetMethodInfo(dispId);
}

void SnegopatAttachedContext::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsProcedure(dispId, args);
}

void SnegopatAttachedContext::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsFunction(dispId, args, retVal);
}
#include "stdafx.h"
#include "SnegopatAttachedContext.h"


SnegopatAttachedContext::SnegopatAttachedContext(IRuntimeContextInstance^ Designer)
{
	m_DesignerWrapper = Designer;
	m_varList = gcnew List<IVariable^>();
	m_nameList = gcnew List<String^>();
	m_methods = gcnew List<MethodInfo>();

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
	instance = m_DesignerWrapper;
	methods = gcnew array<MethodInfo,1>(0);
	variables = m_varList->ToArray();
}

IEnumerable<VariableDescriptor>^ SnegopatAttachedContext::GetSymbols()
{
	array<VariableDescriptor>^ arr = gcnew array<VariableDescriptor>(m_varList->Count);
	for (int i = 0; i < m_varList->Count; i++)
	{
		VariableDescriptor d;
		d.Identifier = m_nameList[i];
		d.Type = SymbolType::ContextProperty;
		arr[i] = d;
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
	int propNum = m_DesignerWrapper->FindProperty(name);
	m_varList->Add(Variable::CreateContextPropertyReference(m_DesignerWrapper, propNum));
	m_nameList->Add(name);
}

void SnegopatAttachedContext::InsertMethod(String^ name)
{
	int mNum = m_DesignerWrapper->FindMethod(name);
	MethodInfo mi = m_DesignerWrapper->GetMethodInfo(mNum);
	m_methods->Add(mi);
}
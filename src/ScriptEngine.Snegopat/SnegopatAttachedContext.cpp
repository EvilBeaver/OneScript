#include "stdafx.h"
#include "SnegopatAttachedContext.h"


SnegopatAttachedContext::SnegopatAttachedContext(IRuntimeContextInstance^ Designer)
{
	m_DesignerWrapper = Designer;
	m_varList = gcnew List<IVariable^>();
	m_nameList = gcnew List<String^>();

	InsertProperty("addins");
	InsertProperty("cmdTrace");
	InsertProperty("events");
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
	return gcnew array<MethodInfo,1>(0);
}

void SnegopatAttachedContext::InsertProperty(String^ name)
{
	int propNum = m_DesignerWrapper->FindProperty(name);
	m_varList->Add(Variable::CreateContextPropertyReference(m_DesignerWrapper, propNum));
	m_nameList->Add(name);
}
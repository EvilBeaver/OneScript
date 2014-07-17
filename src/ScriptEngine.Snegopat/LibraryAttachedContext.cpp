#include "stdafx.h"
#include "LibraryAttachedContext.h"


LibraryAttachedContext::LibraryAttachedContext(AttachedScriptsFactory^ factory)
{
}

void LibraryAttachedContext::OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance)
{
	//instance = this;
	//methods = m_methods->ToArray();
	//variables = m_varList->ToArray();
}

IEnumerable<VariableInfo>^ LibraryAttachedContext::GetProperties()
{
	
}

IEnumerable<MethodInfo>^ LibraryAttachedContext::GetMethods()
{
	
}

int LibraryAttachedContext::FindProperty(String^ name) 
{
	
	throw RuntimeException::PropNotFoundException(name);
}

bool LibraryAttachedContext::IsPropReadable(int propNum) 
{
	
}

bool LibraryAttachedContext::IsPropWritable(int propNum) 
{
	
}

IValue^ LibraryAttachedContext::GetPropValue(int propNum) 
{
	
}

void LibraryAttachedContext::SetPropValue(int propNum, IValue^ val) 
{
	
}

int LibraryAttachedContext::FindMethod(String^ mName) 
{
	
}

MethodInfo LibraryAttachedContext::GetMethodInfo(int mNum) 
{
	
}

void LibraryAttachedContext::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	
}

void LibraryAttachedContext::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	
}
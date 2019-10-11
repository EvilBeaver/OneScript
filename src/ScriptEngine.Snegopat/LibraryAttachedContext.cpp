#include "stdafx.h"
#include "LibraryAttachedContext.h"


LibraryAttachedContext::LibraryAttachedContext(ScriptingEngine^ engine)
{
	m_engine = engine;
	m_methodIndexes = gcnew Dictionary<String^, int>();
	m_methods = gcnew array<ScriptEngine::Machine::MethodInfo>(1);

	ScriptEngine::Machine::MethodInfo miDispose;
	miDispose.Name = L"־סגמבמהטעü־בתוךע";
	miDispose.Alias = L"DisposeObject";
	miDispose.Params = gcnew array<ParameterDefinition>(1);
	miDispose.Params[0].IsByValue = true;

	m_methods[0] = miDispose;
}

void LibraryAttachedContext::OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<ScriptEngine::Machine::MethodInfo>^% methods,
			[Out] IRuntimeContextInstance^% instance)
{
	instance = this;
	methods = m_methods;
	variables = gcnew array<IVariable^>(0);
}

IEnumerable<VariableInfo>^ LibraryAttachedContext::GetProperties()
{
	return gcnew array<VariableInfo>(0);
}

IEnumerable<ScriptEngine::Machine::MethodInfo>^ LibraryAttachedContext::GetMethods()
{
	return m_methods;
}

int LibraryAttachedContext::FindProperty(String^ name) 
{
	throw RuntimeException::PropNotFoundException(name);
}

bool LibraryAttachedContext::IsPropReadable(int propNum) 
{
	return false;
}

bool LibraryAttachedContext::IsPropWritable(int propNum) 
{
	return false;
}

IValue^ LibraryAttachedContext::GetPropValue(int propNum) 
{
	throw gcnew NotImplementedException();
}

void LibraryAttachedContext::SetPropValue(int propNum, IValue^ val) 
{
	throw gcnew NotImplementedException();
}

int LibraryAttachedContext::FindMethod(String^ mName) 
{
	int index;
	if (m_methodIndexes->TryGetValue(mName, index))
	{
		return index;
	}

	throw RuntimeException::MethodNotFoundException(mName);
}

ScriptEngine::Machine::MethodInfo LibraryAttachedContext::GetMethodInfo(int mNum)
{
	return m_methods[mNum];
}

void LibraryAttachedContext::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	switch (mNum)
	{
	case 0:
		DisposeObject(args[0]);
		break;
	}
}

void LibraryAttachedContext::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	throw gcnew NotImplementedException();
}

void LibraryAttachedContext::DisposeObject(IValue^ object)
{
	auto ptr = dynamic_cast<IDisposable^>(object);
	if (ptr != nullptr)
	{
		delete ptr;
	}
}

#pragma once
#include <vcclr.h>
#include <comdef.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Compiler;
using namespace ScriptEngine::Environment;
using namespace System::Runtime::InteropServices;

/* Defines global context for snegopat script
*/
ref class SnegopatAttachedContext : public IAttachableContext, public ICompilerSymbolsProvider
{
private:

	IRuntimeContextInstance^ m_DesignerWrapper;
	List<IVariable^>^ m_varList;
	List<String^>^ m_nameList;
	List<MethodInfo>^ m_methods;

	void InsertProperty(String^ name);
	void InsertMethod(String^ name);

public:
	SnegopatAttachedContext(IRuntimeContextInstance^ Designer);

	virtual void OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance);

	virtual IEnumerable<VariableDescriptor>^ GetSymbols();
    virtual IEnumerable<MethodInfo>^ GetMethods();

};


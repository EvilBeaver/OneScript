#pragma once
#include <vcclr.h>

using namespace System;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace System::Runtime::InteropServices;

ref class SnegopatAttachedContext : public IAttachableContext
{
public:
	SnegopatAttachedContext(void);
	virtual void OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance);
};


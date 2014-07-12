#pragma once

#include <comdef.h>
#include "MarshalingHelpers.h"
#include "SnegAPIDefinitions.h"

using namespace System;
using namespace System::Reflection;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Machine::Contexts;

public ref class EventCallableSDO : public ReflectableSDO
{
public:
	EventCallableSDO(ScriptDrivenObject^ instance, LoadedModuleHandle module);

protected:

	virtual Object^ InvokeInternal(String^ name,
		System::Reflection::BindingFlags invokeAttr,
		Binder^ binder,
		Object^ target,
		array<Object^>^ args,
		System::Globalization::CultureInfo^ culture) override;
};


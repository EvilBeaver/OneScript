#pragma once

#include "ScriptDrivenAddin.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Machine::Contexts;
using namespace System::Runtime::InteropServices;

ref class LibraryAttachedContext : public IAttachableContext, public IRuntimeContextInstance
{
private:

	ScriptingEngine^ m_engine;
	array<ScriptEngine::Machine::MethodInfo>^ m_methods;
	Dictionary<String^, int>^ m_methodIndexes;

	void DisposeObject(IValue^ object);

public:
	LibraryAttachedContext(ScriptingEngine^ engine);

	virtual void OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<ScriptEngine::Machine::MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance);

	virtual IEnumerable<VariableInfo>^ GetProperties();
    virtual IEnumerable<ScriptEngine::Machine::MethodInfo>^ GetMethods();

	property bool IsIndexed
	{
		virtual bool get() { return false; }
	};

	property bool DynamicMethodSignatures
	{
		virtual bool get() { return false; }
	};

	virtual IValue^ GetIndexedValue(IValue^ index){ return nullptr; }
	virtual void SetIndexedValue(IValue^ index, IValue^ val){}

	virtual int FindProperty(String^ name);
	virtual bool IsPropReadable(int propNum);
	virtual bool IsPropWritable(int propNum);
	virtual IValue^ GetPropValue(int propNum);
	virtual void SetPropValue(int propNum, IValue^ val);
	virtual int FindMethod(String^ mName);
	virtual ScriptEngine::Machine::MethodInfo GetMethodInfo(int mNum);
	virtual void CallAsProcedure(int mNum, array<IValue^>^ args);
	virtual void CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal);

};


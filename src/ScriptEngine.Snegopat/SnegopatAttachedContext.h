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
ref class SnegopatAttachedContext : public IAttachableContext, public IRuntimeContextInstance
{
private:

	IRuntimeContextInstance^ m_DesignerWrapper;
	List<IVariable^>^ m_varList;
	List<String^>^ m_nameList;
	List<ScriptEngine::Machine::MethodInfo>^ m_methods;
	Dictionary<int,int>^ m_propDispIdMap;
	Dictionary<int,int>^ m_methDispIdMap;
	Dictionary<String^, int>^ m_propIndexes;
	Dictionary<String^, int>^ m_methIndexes;

	void InsertProperty(String^ name);
	void PropertyAlias(String^ name, String^ alias);
	void InsertMethod(String^ name);
	void MethodAlias(String^ name, String^ alias);

	static SnegopatAttachedContext^ s_instance;

public:
	SnegopatAttachedContext(IRuntimeContextInstance^ Designer);

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

	[ScriptEngine::Machine::Contexts::ScriptConstructor(ParametrizeWithClassName = true)]
	static IRuntimeContextInstance^ CreateV8New(String^ className, array<IValue^>^ args);

	static void SetInstance(SnegopatAttachedContext^ instance)
	{
		s_instance = instance;
	}

};


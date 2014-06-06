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
	List<MethodInfo>^ m_methods;
	Dictionary<int,int>^ m_propDispIdMap;
	Dictionary<int,int>^ m_methDispIdMap;

	void InsertProperty(String^ name);
	void InsertMethod(String^ name);

public:
	SnegopatAttachedContext(IRuntimeContextInstance^ Designer);

	virtual void OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance);

	virtual IEnumerable<VariableInfo>^ GetProperties();
    virtual IEnumerable<MethodInfo>^ GetMethods();

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
	virtual MethodInfo GetMethodInfo(int mNum);
	virtual void CallAsProcedure(int mNum, array<IValue^>^ args);
	virtual void CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal);


};


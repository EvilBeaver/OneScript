#pragma once

#include "IAddinImpl.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Machine::Contexts;
using namespace System::Runtime::InteropServices;

ref class TrickyWrapper : public PropertyNameIndexAccessor, public IObjectWrapper
{
private:

	UserScriptContextInstance^ m_script;
	IAddinImpl* m_scriptDispatcher;
	static const int THIS_VARIABLE_INDEX = 0;

public:
	TrickyWrapper(IAddinImpl* dispatched);
	virtual ~TrickyWrapper();

	void OverrideThisObject(MachineInstance^ machine);

	virtual property Object^ UnderlyingObject
	{
		Object^ get();
	}

	virtual int FindProperty(String^ name) override;
	virtual bool IsPropReadable(int propNum) override;
	virtual bool IsPropWritable(int propNum) override;
	virtual IValue^ GetPropValue(int propNum) override;
	virtual void SetPropValue(int propNum, IValue^ val) override;
	virtual int FindMethod(String^ mName) override;
	virtual MethodInfo GetMethodInfo(int mNum) override;
	virtual void CallAsProcedure(int mNum, array<IValue^>^ args) override;
	virtual void CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) override;


};


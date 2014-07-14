#pragma once

#include "EventCallableSDO.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Machine::Contexts;

ref class ScriptDrivenAddin : public ScriptDrivenObject, public IObjectWrapper
{
private:

	EventCallableSDO^ m_marshalledReference;
	List<IValue^>^ m_valuesByIndex;
	Dictionary<String^, int>^ m_names;

public:
	ScriptDrivenAddin(LoadedModuleHandle module);
	virtual ~ScriptDrivenAddin();
	
	virtual property Object^ UnderlyingObject
	{
		Object^ get();
	}

	void AddProperty(String^ name, IValue^ value);
	
protected:

	virtual int GetMethodCount() override
    {
        return 0;
    }

    virtual int GetVariableCount() override
    {
		return m_valuesByIndex->Count;
    }

	virtual int FindOwnProperty(String^ name) override;
	virtual bool IsOwnPropReadable(int index) override
	{
		return true;
	}
	virtual bool IsOwnPropWritable(int index) override
	{
		return false;
	}
	virtual IValue^ GetOwnPropValue(int index) override;

	virtual void UpdateState() override { };

};


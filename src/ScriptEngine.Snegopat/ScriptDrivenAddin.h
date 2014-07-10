#pragma once

#include "SelfScriptIDispatch.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace ScriptEngine;
using namespace ScriptEngine::Machine;
using namespace ScriptEngine::Machine::Contexts;

ref class ScriptDrivenAddin : public ScriptDrivenObject, public IObjectWrapper
{
private:
	
	SelfScriptIDispatch* m_scriptDispatcher;
	//Object^ m_marshalledReference;

	ReflectableSDO^ m_marshalledReference;

public:
	ScriptDrivenAddin(LoadedModuleHandle module);
	virtual ~ScriptDrivenAddin();
	
	virtual property Object^ UnderlyingObject
	{
		Object^ get();
	}

protected:

	virtual int GetMethodCount() override
    {
        return 0;
    }

    virtual int GetVariableCount() override
    {
        return 1;
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
	virtual IValue^ GetOwnPropValue(int index) override
	{
		return this;
	}

	virtual void UpdateState() override { };

};


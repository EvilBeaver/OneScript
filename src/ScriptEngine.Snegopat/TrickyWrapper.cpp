#include "stdafx.h"
#include "TrickyWrapper.h"
#include "MarshalingHelpers.h"


TrickyWrapper::TrickyWrapper(IAddinImpl* dispatched)
{
	m_scriptDispatcher = dispatched;
	m_scriptDispatcher->AddRef();
	m_script = dispatched->GetManagedInstance();
}

TrickyWrapper::~TrickyWrapper()
{
	if(m_scriptDispatcher != nullptr)
	{
		m_script = nullptr;
		m_scriptDispatcher->Release();
		m_scriptDispatcher = NULL;
	}
}

void TrickyWrapper::OverrideThisObject(MachineInstance^ machine)
{
	array<IVariable^>^ vars;
	array<MethodInfo>^ methods;
	IRuntimeContextInstance^ ctx;
	m_script->OnAttach(machine, vars, methods, ctx);
	vars[THIS_VARIABLE_INDEX] = Variable::CreateContextPropertyReference(this, THIS_VARIABLE_INDEX);
}

Object^ TrickyWrapper::UnderlyingObject::get()
{
	IUnknown* pUnk;
	m_scriptDispatcher->QueryInterface(IID_IUnknown, (void**)&pUnk);
	IntPtr pointer = IntPtr(pUnk);
	Object^ obj = Marshal::GetObjectForIUnknown(pointer); // increments refCount
	pUnk->Release();
	
	return obj;
}

int TrickyWrapper::FindProperty(String^ name) 
{
	return m_script->FindProperty(name);
}

bool TrickyWrapper::IsPropReadable(int propNum) 
{
	return m_script->IsPropReadable(propNum);
}

bool TrickyWrapper::IsPropWritable(int propNum) 
{
	return m_script->IsPropWritable(propNum);
}

IValue^ TrickyWrapper::GetPropValue(int propNum) 
{
	if(propNum == THIS_VARIABLE_INDEX)
		return this;
	else
		return m_script->GetPropValue(propNum);
}

void TrickyWrapper::SetPropValue(int propNum, IValue^ val) 
{
	m_script->SetPropValue(propNum, val);
}

int TrickyWrapper::FindMethod(String^ mName) 
{
	return m_script->FindMethod(mName);
}

MethodInfo TrickyWrapper::GetMethodInfo(int mNum) 
{
	return m_script->GetMethodInfo(mNum);
}

void TrickyWrapper::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	m_script->CallAsProcedure(mNum, args);
}

void TrickyWrapper::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	m_script->CallAsFunction(mNum, args, retVal);
}
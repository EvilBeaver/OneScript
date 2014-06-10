#include "stdafx.h"
#include "TrickyWrapper.h"


TrickyWrapper::TrickyWrapper(IAddinImpl* dispatched)
{
	m_scriptDispatcher = dispatched;
	m_script = dispatched->GetManagedInstance();
}

Object^ TrickyWrapper::UnderlyingObject::get()
{
	IntPtr pointer = IntPtr(m_scriptDispatcher);
	return Marshal::GetObjectForIUnknown(pointer); // increments refCount
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
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->IsPropWritable(dispId);
}

IValue^ TrickyWrapper::GetPropValue(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->GetPropValue(dispId);
}

void TrickyWrapper::SetPropValue(int propNum, IValue^ val) 
{
	int dispId = m_propDispIdMap[propNum];
	m_DesignerWrapper->SetPropValue(dispId, val);
}

int TrickyWrapper::FindMethod(String^ mName) 
{
	for (int i = 0; i < m_methods->Count; i++)
	{
		String^ nameFromList = m_methods[i].Name;
		if(String::Compare(nameFromList, mName, true) == 0)
		{
			return i;
		}
	}

	throw RuntimeException::MethodNotFoundException(mName);
}

MethodInfo TrickyWrapper::GetMethodInfo(int mNum) 
{
	int dispId = m_methDispIdMap[mNum];
	return m_DesignerWrapper->GetMethodInfo(dispId);
}

void TrickyWrapper::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsProcedure(dispId, args);
}

void TrickyWrapper::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsFunction(dispId, args, retVal);
}
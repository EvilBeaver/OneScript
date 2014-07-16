#include "stdafx.h"
#include "SnegopatAttachedContext.h"

#define ALIASED_METHOD(name, alias) InsertMethod((name)); MethodAlias((name), (alias))

SnegopatAttachedContext::SnegopatAttachedContext(IRuntimeContextInstance^ Designer)
{
	m_DesignerWrapper = Designer;
	m_varList = gcnew List<IVariable^>();
	//m_nameList = gcnew List<String^>();
	m_methods = gcnew List<MethodInfo>();
	m_propDispIdMap = gcnew Dictionary<int,int>();
	m_methDispIdMap = gcnew Dictionary<int,int>();
	m_propIndexes = gcnew Dictionary<String^, int>(StringComparer::InvariantCultureIgnoreCase);
	m_methIndexes = gcnew Dictionary<String^, int>(StringComparer::InvariantCultureIgnoreCase);

	s_instance = this;

	InsertProperty("addins");
	InsertProperty("cmdTrace");
	InsertProperty("events");
	InsertProperty("profileRoot");
	InsertProperty("snegopat");
	InsertProperty("hotkeys");
	InsertProperty("windows");
	InsertProperty("metadata");
	InsertProperty("v8files");
	InsertProperty("ownerName");
	InsertProperty("v8debug");
	InsertProperty("sVersion");
	InsertProperty("v8Version");

	ALIASED_METHOD("v8New", "СоздатьОбъект");
	ALIASED_METHOD("Message", "Сообщить");
	ALIASED_METHOD("MessageBox", "Предупреждение");
	ALIASED_METHOD("globalContext","ГлобальныйКонтекст");
	ALIASED_METHOD("getCmdState", "ПолучитьСостояниеКоманды");
	ALIASED_METHOD("saveProfile", "СохранитьНастройку");
	ALIASED_METHOD("createTimer", "СоздатьТаймер");
	ALIASED_METHOD("killTimer", "ОстановитьТаймер");
	ALIASED_METHOD("toV8Value", "ВЗначение1С");
	ALIASED_METHOD("loadResourceString", "ЗагрузитьСтрокуРесурсов");
	ALIASED_METHOD("loadScriptForm", "ЗагрузитьФормуИзФайла");
	ALIASED_METHOD("designScriptForm", "РедактироватьФормуИзФайла");
	ALIASED_METHOD("sendCommand", "ПослатьКоманду");

	MethodAlias("MessageBox", "Вопрос");

}

void SnegopatAttachedContext::OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance)
{
	instance = this;
	methods = m_methods->ToArray();
	variables = m_varList->ToArray();
}

IEnumerable<VariableInfo>^ SnegopatAttachedContext::GetProperties()
{
	array<VariableInfo>^ arr = gcnew array<VariableInfo>( m_propIndexes->Count);
	int i = 0;
	for each (KeyValuePair<String^, int> kv in m_propIndexes)
	{
		VariableInfo vi;

		vi.Identifier = kv.Key;
		vi.Type = SymbolType::ContextProperty;
		arr[i++] = vi;
	}

	return arr;

}

IEnumerable<MethodInfo>^ SnegopatAttachedContext::GetMethods()
{
	array<MethodInfo>^ arr = gcnew array<MethodInfo>(m_methods->Count);
	for (int i = 0; i < m_methods->Count; i++)
	{
		arr[i] = m_methods[i];
	}

	return arr;
}

void SnegopatAttachedContext::InsertProperty(String^ name)
{
	int index = m_propIndexes->Count;
	int propNum = m_DesignerWrapper->FindProperty(name);
	m_propDispIdMap->Add(index, propNum);
	m_varList->Add(Variable::CreateContextPropertyReference(this, index));
	m_propIndexes->Add(name, index);
}

void SnegopatAttachedContext::PropertyAlias(String^ name, String^ alias)
{
	int index = m_propIndexes[name];
	m_propIndexes->Add(alias, index);
}

void SnegopatAttachedContext::InsertMethod(String^ name)
{
	int index = m_methods->Count;
	int mNum = m_DesignerWrapper->FindMethod(name);
	MethodInfo mi = m_DesignerWrapper->GetMethodInfo(mNum);
	m_methods->Add(mi);
	m_methDispIdMap->Add(index, mNum);
	m_methIndexes->Add(name, index);
}

void SnegopatAttachedContext::MethodAlias(String^ name, String^ alias)
{
	int index = m_methIndexes[name];
	m_methIndexes->Add(alias, index);
	MethodInfo mi = m_methods[index];
	mi.Alias = alias;
	m_methods[index] = mi;

}

int SnegopatAttachedContext::FindProperty(String^ name) 
{
	int index;
	if(m_propIndexes->TryGetValue(name, index))
	{
		return index;
	}
	
	throw RuntimeException::PropNotFoundException(name);
}

bool SnegopatAttachedContext::IsPropReadable(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->IsPropReadable(dispId);
}

bool SnegopatAttachedContext::IsPropWritable(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->IsPropWritable(dispId);
}

IValue^ SnegopatAttachedContext::GetPropValue(int propNum) 
{
	int dispId = m_propDispIdMap[propNum];
	return m_DesignerWrapper->GetPropValue(dispId);
}

void SnegopatAttachedContext::SetPropValue(int propNum, IValue^ val) 
{
	int dispId = m_propDispIdMap[propNum];
	m_DesignerWrapper->SetPropValue(dispId, val);
}

int SnegopatAttachedContext::FindMethod(String^ mName) 
{
	int index;
	if(m_methIndexes->TryGetValue(mName, index))
	{
		return index;
	}
	
	throw RuntimeException::MethodNotFoundException(mName);
}

MethodInfo SnegopatAttachedContext::GetMethodInfo(int mNum) 
{
	int dispId = m_methDispIdMap[mNum];
	return m_DesignerWrapper->GetMethodInfo(dispId);
}

void SnegopatAttachedContext::CallAsProcedure(int mNum, array<IValue^>^ args) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsProcedure(dispId, args);
}

void SnegopatAttachedContext::CallAsFunction(int mNum, array<IValue^>^ args, [Out] IValue^% retVal) 
{
	int dispId = m_methDispIdMap[mNum];
	m_DesignerWrapper->CallAsFunction(dispId, args, retVal);
}

IRuntimeContextInstance^ SnegopatAttachedContext::CreateV8New(String^ className, array<IValue^>^ args)
{
	int v8new = s_instance->m_DesignerWrapper->FindMethod("v8new");
	IValue^ result;
	array<IValue^>^ realArgs = gcnew array<IValue^>(args->Length + 1);
	realArgs[0] = ValueFactory::Create(className);
	args->CopyTo(realArgs, 1);
	s_instance->m_DesignerWrapper->CallAsFunction(v8new, realArgs, result);

	return safe_cast<IRuntimeContextInstance^>(result);
}
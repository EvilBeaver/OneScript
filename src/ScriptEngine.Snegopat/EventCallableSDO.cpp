#include "stdafx.h"
#include "EventCallableSDO.h"

EventCallableSDO::EventCallableSDO(ScriptDrivenObject^ instance, LoadedModuleHandle module)
	: ReflectableSDO(instance, module)
{
}

Object^ EventCallableSDO::InvokeInternal(String^ name,
		System::Reflection::BindingFlags invokeAttr,
		Binder^ binder,
		Object^ target,
		array<Object^>^ args,
		System::Globalization::CultureInfo^ culture)
{
	array<Object^>^ passedArgs = gcnew array<Object^>(args->Length);
	array<IParamsWrapper^>^ wrappers = gcnew array<IParamsWrapper^>(args->Length);
	
	try
	{
		Object^ result;
	
		for (int i = 0; i < args->Length; i++)
		{
			IParamsWrapper^ wrapper = dynamic_cast<IParamsWrapper^>(args[i]);
			IValue^ argValue;
			if(wrapper != nullptr)
			{
				wrappers[i] = wrapper;
				argValue = COMWrapperContext::CreateIValue(wrapper->val);
			}
			else
			{
				argValue = COMWrapperContext::CreateIValue(args[i]);
			}

			passedArgs[i] = Variable::CreateReference(Variable::Create(argValue));
	
		}
	
		result = ReflectableSDO::InvokeInternal(name, invokeAttr, binder, target, passedArgs, culture);
	
		for (int i = 0; i < args->Length; i++)
		{
			IParamsWrapper^ wrapper = wrappers[i];
			Object^ argValue = COMWrapperContext::MarshalIValue(safe_cast<IValue^>(passedArgs[i]));
			if(wrapper != nullptr)
			{
				wrapper->val = argValue;
			}
			else
			{
				args[i] = passedArgs[i];
			}
		}
	
		return result;
	
	}
	catch(Exception^ exc)
	{
		auto buf = stringBuf(exc->ToString());
		MessageBox(0, buf, L"Error", MB_ICONERROR);
		delete[] buf;
			
		return nullptr;
	}
	finally
	{
		delete[] passedArgs;
		delete[] wrappers;
	}
}
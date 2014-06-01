#include "stdafx.h"
#include "SnegopatAttachedContext.h"


SnegopatAttachedContext::SnegopatAttachedContext(void)
{
}

void SnegopatAttachedContext::OnAttach(MachineInstance^ machine,
			[Out] cli::array<IVariable^,1>^% variables, 
			[Out] cli::array<MethodInfo>^% methods, 
			[Out] IRuntimeContextInstance^% instance)
{
}
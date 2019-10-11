#include "stdafx.h"
#include "RefCountable.h"

RefCountable::RefCountable(void)
{
	m_refCount = 0;
}

ULONG   __stdcall RefCountable::AddRef()
{
	return InterlockedIncrement(&m_refCount) ;
}

ULONG   __stdcall RefCountable::Release()
{
	long nRefCount = 0;
	nRefCount = InterlockedDecrement(&m_refCount) ;
	if (nRefCount == 0)
	{
		OnZeroCount();
		delete this;
	}

	return nRefCount;
}

void RefCountable::OnZeroCount()
{
}

RefCountable::~RefCountable(void)
{
}

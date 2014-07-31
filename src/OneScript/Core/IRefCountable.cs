using System;

namespace OneScript.Core
{
    public interface IRefCountable
    {
        int AddRef();
        int Release();
    }
}

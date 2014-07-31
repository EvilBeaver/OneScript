using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public interface IRuntimeContextInstance
    {
        bool IsIndexed { get; }
        int GetPropCount();
        int FindProperty(string name);
        string GetPropertyName(int index);
        IValue GetIndexedValue(IValue index);
        void SetIndexedValue(IValue index, IValue newValue);
        bool IsPropReadable(int index);
        bool IsPropWriteable(int index);
        IValue GetPropertyValue(int index);
        void SetPropertyValue(int index, IValue newValue);
        bool DynamicMethodSignatures { get; }
        int GetMethodsCount();
        int FindMethod(string name);
        string GetMethodName(int index);
        bool HasReturnValue(int index);
        int GetParametersCount(int index);
        bool GetDefaultValue(int methodIndex, int paramIndex, out IValue defaultValue);
        void CallAsProcedure(int index, IValue[] args);
        IValue CallAsFunction(int index, IValue[] args);
    }
}

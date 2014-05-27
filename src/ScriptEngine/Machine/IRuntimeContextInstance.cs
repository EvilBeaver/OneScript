using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public interface IRuntimeContextInstance
    {
        bool IsIndexed { get; }

        IValue GetIndexedValue(IValue index);
        void SetIndexedValue(IValue index, IValue val);

        int FindProperty(string name);
        bool IsPropReadable(int propNum);
        bool IsPropWritable(int propNum);
        IValue GetPropValue(int propNum);
        void SetPropValue(int propNum, IValue newVal);


        int FindMethod(string name);
        MethodInfo GetMethodInfo(int methodNumber);
        void CallAsProcedure(int methodNumber, IValue[] arguments);
        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue);

    }

}
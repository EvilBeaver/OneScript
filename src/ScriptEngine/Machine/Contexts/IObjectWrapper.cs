using System;
namespace ScriptEngine.Machine.Contexts
{
    public interface IObjectWrapper
    {
        object UnderlyingObject { get; }
    }
}

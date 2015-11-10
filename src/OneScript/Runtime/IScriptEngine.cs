using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    /// <summary>
    /// Хранит контекстную runtime-информацию о запущенном движке:
    ///  - Менеджер типов (TypeManager)
    ///  - возможно, информацию для отладки
    ///  - настройки (если есть)
    /// </summary>
    public interface IScriptEngine
    {
        TypeManager TypeManager { get; }
        IRuntimeContext RuntimeContext { get; }
    }
}

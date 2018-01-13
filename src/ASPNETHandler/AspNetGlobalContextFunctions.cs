/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Web;


namespace OneScript.ASPNETHandler
{
    /// <summary>
    /// Глобальный контекст. Функции, доступные в Web-приложении.
    /// </summary>
    [GlobalContext(Category = "Процедуры и функции доступные в Web-приложении")]
    public sealed class GlobalWebFunctions : GlobalContextBase<GlobalWebFunctions>
    {
        public static IAttachableContext CreateInstance()
        {
            return new GlobalWebFunctions();
        }
        /// <summary>
        /// Возвращает физический путь, соответствующий виртуальному
        /// </summary>
        /// <param name="virtualPath">Виртуальный путь, например: ~/</param>
        /// <returns></returns>
        [ContextMethod("ПолучитьФизическийПутьИзВиртуального")]
        public string MapPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }
    }
}

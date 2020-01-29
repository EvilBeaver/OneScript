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

using ScriptEngine.HostedScript.Library;

/// <summary>
/// 
/// </summary>

namespace ScriptEngine.HostedScript.Library.HTTPService
{
    [ContextClass("СредстваHTTP", "HTTPMeans")]
    public class HttpMeansImpl : AutoContext<HttpMeansImpl>
    {
        [ContextMethod("ПолучитьФизическийПутьИзВиртуального", "MapPath")]
        public string MapPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath); ;
        }

        [ScriptConstructor(Name = "Без параметров")]
        public static HttpMeansImpl Constructor()
        {
            return new HttpMeansImpl();
        }

        public HttpMeansImpl()
        {
        }
    }
}

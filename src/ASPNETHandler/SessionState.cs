﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;

/// <summary>
/// 
/// </summary>

namespace ScriptEngine.HostedScript.Library.HTTPService
{
    [ContextClass("HTTPСервисПараметрыСессии", "HTTPServiceRequestSessionState")]
    public class SessionStateImpl : AutoContext<SessionStateImpl>
    {
        System.Web.HttpContext _context;

        public SessionStateImpl(System.Web.HttpContext context)
        {
            _context = context;
        }

        [ContextProperty("Количество", "Count")]
        public IValue Count
        {
            get
            {
                return ValueFactory.Create(_context.Session.Count);
            }
        }
    }
}

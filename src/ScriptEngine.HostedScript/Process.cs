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
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript
{
    public class Process
    {
        ScriptingEngine _engine;

        readonly IHostApplication _host;
        readonly LoadedModule _module;

        internal Process(IHostApplication host, LoadedModule src, ScriptingEngine runtime)
        {
            _host = host;
            _engine = runtime;
            _module = src;
        }

        public int Start()
        {
            int exitCode = 0;

            try
            {
                _engine.UpdateContexts();
                _engine.NewObject(_module);
                exitCode = 0;
            }
            catch (ScriptInterruptionException e)
            {
                exitCode = e.ExitCode;
            }
            catch (Exception e)
            {
                _host.ShowExceptionInfo(e);
                exitCode = 1;
            }
            finally
            {
                _engine.DebugController?.NotifyProcessExit(exitCode);
                _engine.Dispose();
                _engine = null;
            }

            return exitCode;
        }

    }
}

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

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EnumValueAttribute : Attribute
    {
        readonly string _name;

        readonly string _alias;

        public EnumValueAttribute(string name, string alias = null)
        {
            _name = name;
            _alias = alias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }
    }
}

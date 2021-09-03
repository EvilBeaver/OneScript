﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Contexts.Internal
{
    internal interface IBuildableMember
    {
        void SetDeclaringType(Type type);
        void SetName(string name);
        void SetAlias(string alias);
        void SetExportFlag(bool isExport);
        void SetDataType(Type type);
        void SetAnnotations(IEnumerable<object> annotations);
        void SetDispatchIndex(int index);
    }

    internal interface IBuildableMethod : IBuildableMember
    {
        void SetParameters(IEnumerable<BslParameterInfo> parameters);
    }
    
    internal interface IBuildableProperty : IBuildableMember
    {
        void CanRead(bool canRead);
        void CanWrite(bool canWrite);
    }
}
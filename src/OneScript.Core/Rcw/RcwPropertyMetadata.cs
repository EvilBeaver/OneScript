﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Rcw
{
    public class RcwPropertyMetadata : RcwMemberMetadata
    {
        public bool IsReadable { get; internal set; }

        public bool IsWritable { get; internal set; }

        public RcwPropertyMetadata(string name, int dispId) : base(name, dispId)
        {
            IsReadable = true;
            IsWritable = true;
        }
    }
}
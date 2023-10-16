/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Rcw
{
    public abstract class RcwMemberMetadata
    {
        public int DispatchId { get; }

        public string Name { get; }

        protected RcwMemberMetadata(string name, int dispId)
        {
            Name = name;
            DispatchId = dispId;
        }
    }
}
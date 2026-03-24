// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

namespace VSCode.DebugAdapter
{
    internal class AttachOptions
    {
        public int DebugPort { get; set; } = 2801;
        public WorkspaceMapper PathsMapping { get; set; }
    }
}
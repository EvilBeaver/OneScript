/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    public class AttachedContext
    {
        private AttachedContext(IAttachableContext instance, bool asDynamic)
        {
            Instance = instance;
        }

        public IAttachableContext Instance { get; }

        public static AttachedContext Create(IAttachableContext context) => 
            new AttachedContext(context, false);

        public static AttachedContext CreateDynamic(IAttachableContext context) =>
            new AttachedContext(context, true);
    }
}
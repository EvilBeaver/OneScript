/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Compiler;

namespace ScriptEngine.Machine
{
    public class AttachedContext
    {
        private AttachedContext(IAttachableContext instance, bool asDynamic)
        {
            Symbols = new SymbolScope
            {
                IsDynamicScope = asDynamic
            };
            
            Instance = instance;

            LoadSymbols();
        }

        private void LoadSymbols()
        {
            foreach (var item in Instance.GetProperties())
            {
                Symbols.DefineVariable(item.Identifier, item.Alias);
            }

            foreach (var item in Instance.GetMethods())
            {
                Symbols.DefineMethod(item);
            }
        }

        public SymbolScope Symbols { get; }
        
        public IAttachableContext Instance { get; }

        public static AttachedContext Create(IAttachableContext context) => 
            new AttachedContext(context, false);

        public static AttachedContext CreateDynamic(IAttachableContext context) =>
            new AttachedContext(context, true);
    }
}
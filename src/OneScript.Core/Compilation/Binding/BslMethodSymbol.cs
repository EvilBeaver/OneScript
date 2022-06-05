/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation.Binding;
using OneScript.Contexts;

namespace OneScript.Runtime.Binding
{
    internal class BslMethodSymbol : IMethodSymbol
    {
        public string Name => Method?.Name;
        public string Alias => Method?.Alias;
        public BslMethodInfo Method { get; set; }
    }
}
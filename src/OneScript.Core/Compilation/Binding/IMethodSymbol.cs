/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace OneScript.Compilation.Binding
{
    public interface IMethodSymbol : ISymbol
    {
        BslMethodInfo Method { get; }
    }
}
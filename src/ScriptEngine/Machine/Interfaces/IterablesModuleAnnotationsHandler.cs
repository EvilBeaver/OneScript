/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Localization;

namespace ScriptEngine.Machine.Interfaces
{
    public class IterablesModuleAnnotationsHandler : SingleWordModuleAnnotationHandler
    {
        private static HashSet<BilingualString> SupportedNames = new HashSet<BilingualString>()
        {
            IterableBslInterfaceChecker.IterableAnnotation,
            IteratorBslInterfaceChecker.IterableAnnotation
        };
        
        public IterablesModuleAnnotationsHandler(IErrorSink errorSink) : base(SupportedNames, errorSink)
        {
        }
    }
}
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Localization
{
    public static class LocalizationExtensions
    {
        public static bool ContainsString(this BilingualString bi, string sample)
        {
            return ContainsString(bi, sample, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public static bool ContainsString(this BilingualString bi, string sample, StringComparison comparison)
        {
            return bi.Russian.Equals(sample, comparison) || bi.Russian.Equals(sample, comparison);
        }
    }
}
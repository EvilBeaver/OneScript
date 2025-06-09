/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneScriptDocumenter.Primary
{
    public class ReferenceCollector : IReferencesRegistry
    {
        private readonly Dictionary<string, ReferencableElement> _referenceTargets =
            new Dictionary<string, ReferencableElement>();

        public void Register(string typeKey, Type target)
        {
            _referenceTargets.Add(typeKey, new ReferencableElement
            {
                Owner = target
            });
        }
        
        public void Register(string memberKey, MemberInfo member)
        {
            _referenceTargets.Add(memberKey, new ReferencableElement
            {
                Owner = member.DeclaringType,
                Member = member
            });
        }

        public ReferencableElement? Get(string key)
        {
            if (_referenceTargets.TryGetValue(key, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScriptDocumenter.Primary;

namespace OneScriptDocumenter.Secondary
{
    public class MarkdownReferenceResolver : IReferenceResolver
    {
        private readonly IReferencesRegistry _registry;
        private readonly ReferenceFactory _referenceFactory;

        public MarkdownReferenceResolver(IReferencesRegistry registry, ReferenceFactory referenceFactory)
        {
            _registry = registry;
            _referenceFactory = referenceFactory;
        }

        public string Resolve(string linkTarget, string linkText)
        {
            var referenceOrNull = _registry.Get(linkTarget);
            if (referenceOrNull == null)
            {
                ConsoleLogger.Warning($"Can't resolve reference {linkTarget}");
                return linkText;
            }

            var reference = referenceOrNull.Value;

            var ownerType = reference.Owner;
            var memberInfo = reference.Member;

            string target;
            if (linkText != "")
            {
                target = _referenceFactory.CreateReference(ownerType, memberInfo);
            }
            else
            {
                var data = _referenceFactory.CreateReferenceWithText(ownerType, memberInfo);
                linkText = data.LinkText;
                target = data.LinkTarget;
            }

            return MarkdownLink(target, linkText);
        }

        public static string MarkdownLink(string linkTarget, string linkText)
        {
            return $"[{linkText}]({linkTarget})";
        }
    }
}
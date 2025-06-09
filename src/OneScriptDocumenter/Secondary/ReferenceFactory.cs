/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;
using OneScript.Contexts;

namespace OneScriptDocumenter.Secondary
{
    public class ReferenceFactory
    {
        private string _baseUrl = "/syntax/";

        public struct ReferenceData
        {
            public string LinkTarget { get; set; }
            public string LinkText { get; set; }
        }

        public ReferenceData CreateReferenceWithText(Type ownerType, MemberInfo memberInfo = null)
        {
            var typeNameInLink = GetBslNameForAnnotatedObject(ownerType);
            var memberNameInLink = GetBslNameForAnnotatedObject(memberInfo);

            return new ReferenceData
            {
                LinkTarget = LinkTarget(typeNameInLink, memberNameInLink),
                LinkText = memberNameInLink ?? typeNameInLink
            };
        }
        
        public string CreateReference(Type ownerType, MemberInfo memberInfo = null)
        {
            var typeNameInLink = GetBslNameForAnnotatedObject(ownerType);
            var memberNameInLink = GetBslNameForAnnotatedObject(memberInfo);

            return $"{_baseUrl}{LinkTarget(typeNameInLink, memberNameInLink)}";
        }

        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = AdoptBaseUrl(value);
        }
        
        private string LinkTarget(string typeNameInLink, string memberNameInLink)
        {
            return memberNameInLink == null ? typeNameInLink : typeNameInLink + "#" + memberNameInLink;
        }

        private static string AdoptBaseUrl(string url)
        {
            var finalBaseUrl = url;
            if (finalBaseUrl == "")
                finalBaseUrl = "/";
            else if (!finalBaseUrl.EndsWith("/"))
                finalBaseUrl += "/";

            return finalBaseUrl;
        }
        
        public static string GetBslNameForAnnotatedObject(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return null;
            
            return memberInfo.GetCustomAttributes()
                .Where(a => a is INameAndAliasProvider)
                .Cast<INameAndAliasProvider>()
                .Select(na => !string.IsNullOrEmpty(na.Alias) ? na.Alias : na.Name)
                .FirstOrDefault() ?? memberInfo.Name;
        }
    }
}
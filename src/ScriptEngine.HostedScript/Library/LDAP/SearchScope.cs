using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.LDAP
{
    [EnumerationType("ОбластьПоиска", "SearchScope")]
    public enum SearchScopeImpl
    {
        [EnumItem("БазовыйОбъект", "Base")]
        Base,
        [EnumItem("ОдинУровень", "OneLevel")]
        OneLevel,
        [EnumItem("Дерево", "Subtree")]
        Subtree
    }

    public static class SearchScopeConverter
    {

        public static SearchScope ToSearchScope(SearchScopeImpl searchScope)
        {
            switch (searchScope)
            {
                case SearchScopeImpl.Base:
                    return SearchScope.Base;
                case SearchScopeImpl.OneLevel:
                    return SearchScope.OneLevel;
                case SearchScopeImpl.Subtree:
                    return SearchScope.Subtree;
                default:
                    throw RuntimeException.InvalidArgumentType();

            }
        }

        public static SearchScopeImpl ToSearchScopeImpl(SearchScope searchScope)
        {
            switch (searchScope)
            {
                case SearchScope.Base:
                    return SearchScopeImpl.Base;
                case SearchScope.OneLevel:
                    return SearchScopeImpl.OneLevel;
                case SearchScope.Subtree:
                    return SearchScopeImpl.Subtree;
                default:
                    throw RuntimeException.InvalidArgumentType();

            }
        }
    }
}

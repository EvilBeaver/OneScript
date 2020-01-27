using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.LDAP
{
    [EnumerationType("ОбластьПоискаLDAP", "LDAPSearchScope")]
    public enum LDAPSearchScopeImpl
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

        public static SearchScope ToSearchScope(LDAPSearchScopeImpl searchScope)
        {
            switch (searchScope)
            {
                case LDAPSearchScopeImpl.Base:
                    return SearchScope.Base;
                case LDAPSearchScopeImpl.OneLevel:
                    return SearchScope.OneLevel;
                case LDAPSearchScopeImpl.Subtree:
                    return SearchScope.Subtree;
                default:
                    throw RuntimeException.InvalidArgumentType();

            }
        }

        public static LDAPSearchScopeImpl ToSearchScopeImpl(SearchScope searchScope)
        {
            switch (searchScope)
            {
                case SearchScope.Base:
                    return LDAPSearchScopeImpl.Base;
                case SearchScope.OneLevel:
                    return LDAPSearchScopeImpl.OneLevel;
                case SearchScope.Subtree:
                    return LDAPSearchScopeImpl.Subtree;
                default:
                    throw RuntimeException.InvalidArgumentType();

            }
        }
    }
}

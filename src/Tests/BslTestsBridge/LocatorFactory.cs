/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using BslTestsBridge.Attributes;

namespace BslTestsBridge
{
    internal static class LocatorFactory
    {
        public static IBaseDirectoryLocator Create(BslTestFixtureAttribute attribute)
        {
            if (attribute.LocatorType == null)
            {
                return RepositoryRootLocator.Instance;
            }

            if (attribute.LocatorType.IsAssignableTo(typeof(IBaseDirectoryLocator)))
            {
                return (IBaseDirectoryLocator)Activator.CreateInstance(attribute.LocatorType)!;
            }
            
            throw new ArgumentException($"Locator type {attribute.LocatorType} is not assignable to {typeof(IBaseDirectoryLocator)}");
        }
    }
}
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
using NUnit.Engine.Extensibility;

namespace BslTestsBridge;

[Extension]
public class BslDriverFactory : IDriverFactory
{
    StreamWriter file = new StreamWriter("Extension.log", true);
    
    public bool IsSupportedTestFramework(AssemblyName reference)
    {
        file.WriteLine($"IsSupportedTestFramework: {reference.FullName}");
        file.Flush();
        
        return reference.Name == "nunit.framework";
    }

    public IFrameworkDriver GetDriver(AssemblyName reference)
    {
        file.WriteLine($"GetDriver: {reference.FullName}");
        file.Flush();
        
        return new BslFrameworkDriver();
    }
}
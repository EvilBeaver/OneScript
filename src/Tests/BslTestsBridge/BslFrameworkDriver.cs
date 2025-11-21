/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using NUnit.Engine;
using NUnit.Engine.Extensibility;

namespace BslTestsBridge;

public class BslFrameworkDriver : IFrameworkDriver
{
    public string Load(string testAssemblyPath, IDictionary<string, object> settings)
    {
        throw new NotImplementedException();
    }

    public int CountTestCases(string filter)
    {
        throw new NotImplementedException();
    }

    public string Run(ITestEventListener listener, string filter)
    {
        throw new NotImplementedException();
    }

    public string Explore(string filter)
    {
        throw new NotImplementedException();
    }

    public void StopRun(bool force)
    {
        throw new NotImplementedException();
    }

    public string ID { get; set; }
}
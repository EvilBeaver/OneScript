/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Xunit;

namespace TestRunner;

public class ExternalTestsRunner
{
    [Fact]
    public void TypeDescription()
    {
        TestRunnerHelper.Run(@"typedescription.os");
    }
}
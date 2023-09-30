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
    
    [Fact]
    public void ValueTree()
    {
        TestRunnerHelper.Run(@"valuetree.os");
    }
    
    [Fact]
    public void ValueTable()
    {
        TestRunnerHelper.Run(@"valuetable.os");
    }
    
    [Fact]
    public void ValueList()
    {
        TestRunnerHelper.Run(@"value-list.os");
    }
    
    [Fact]
    public void Math()
    {
        TestRunnerHelper.Run(@"math.os");
    }
}
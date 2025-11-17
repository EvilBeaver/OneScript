using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnitTests.Bsl;

public class BslTestFixtureAttribute : Attribute, IFixtureBuilder
{
    public BslTestFixtureAttribute(string directory)
    {
        Directory = RepositoryRootLocator.ResolvePath(directory);
    }

    public string Directory { get; }

    public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
    {
        var attr = typeInfo.GetCustomAttributes<BslTestFixtureAttribute>(false).FirstOrDefault();
        var testFolder = attr?.Directory ?? throw new Exception("Directory is not set");
        var testFramework = new BslTestsFramework();
        var tests = testFramework.GetTests(testFolder);
        var fixtures = tests.Select(test =>
        {
            try
            {
                return testFramework.LoadTestFixture(test);
            }
            catch(Exception e)
            {
                TestContext.WriteLine(e);
                return null;
            }
        }).Where(f => f != null)
        .ToList();
        
        testFramework.ClearMessages();
        return fixtures;
    }
}
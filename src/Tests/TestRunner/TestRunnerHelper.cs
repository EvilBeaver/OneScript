/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Xunit;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace TestRunner;

[ContextClass("ПроверкиЗначений")]
public class TestRunnerHelper : AutoContext<TestRunnerHelper>
{
    
    public const string GetTestSubName = "ПолучитьСписокТестов";

    private static ScriptingEngine _instance;

    private static ScriptingEngine Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = DefaultEngineBuilder
                    .Create()
                    .SetDefaultOptions()
                    .SetupEnvironment(e =>
                    {
                        e.AddStandardLibrary()
                            .UseTemplateFactory(new DefaultTemplatesFactory());
                    })
                    .Build();
        
                Locale.SystemLanguageISOName = "RU";
            }

            return _instance;
        }
    }

    public static void Run(string filename)
    {
        var engine = Instance;

        var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var filepath = Path.Combine(rootPath, "..", "..", "..", "..", "..", "..", "tests", filename);

        var compiler = engine.GetCompilerService();
        compiler.FillSymbols(typeof(UserScriptContextInstance));
        
        var testModule = compiler.Compile(engine.Loader.FromFile(filepath));
        engine.Initialize();
        var testInstance = (UserScriptContextInstance)engine.NewObject(testModule);
        var testRunnerInstance = new TestRunnerHelper();

        var getTestsIndex = testInstance.GetMethodNumber(GetTestSubName);
        var tmp1 = testRunnerInstance.GetMethodNumber("ПроверитьРавенство");
        Assert.NotEqual(-1, getTestsIndex);

        testInstance.CallAsFunction(getTestsIndex, new []{ testRunnerInstance }, out var result);
        Assert.NotNull(result);

        var arrayOfNames = (ArrayImpl)result;
        Assert.NotNull(arrayOfNames);
        foreach (var testName in arrayOfNames)
        {
            var testSubIndex = testInstance.GetMethodNumber(testName.AsString());
            Assert.NotEqual(-1, testSubIndex);
            
            testInstance.CallAsProcedure(testSubIndex, Array.Empty<IValue>());
        }
    }

    [ContextMethod("ПроверитьРавенство")]
    public void CheckEqual(IValue v1, IValue v2, string extendedInfo = null)
    {
        Assert.Equal(v1, v2);
    }

    [ContextMethod("ПроверитьНеРавенство")]
    public void CheckNotEqual(IValue v1, IValue v2, string extendedInfo = null)
    {
        Assert.NotEqual(v1, v2);
    }

    [ContextMethod("ПроверитьИстину")]
    public void CheckTrue(IValue v1, string extendedInfo = null)
    {
        Assert.True(v1?.AsBoolean() ?? false);
    }

    [ContextMethod("ПроверитьЛожь")]
    public void CheckFalse(IValue v1, string extendedInfo = null)
    {
        Assert.False(v1?.AsBoolean() ?? true);
    }

}
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using Xunit;

namespace OneScript.Core.Tests;

public class CompoundSymbolTableTest
{
    [Fact]
    public void OnlyMaster()
    {
        var master = new SymbolTable();
        master.PushScope(new SymbolScope(), null);

        master.DefineMethod(BslMethodBuilder.Create().Name("Test").Build().ToSymbol());
        master.DefineVariable(BslFieldBuilder.Create().Name("TestField").Build().ToSymbol());
        
        var compound = new CompoundSymbolTable(master);

        compound.ScopeCount.Should().Be(1);

        compound.GetScope(0).Should().BeSameAs(master.GetScope(0));
        compound.TryFindMethod("Test", out _).Should().BeTrue();
        compound.TryFindMethodBinding("Test", out var methBind).Should().BeTrue();
        methBind.ScopeNumber.Should().Be(0);

        compound.FindVariable("TestField", out var varBind).Should().BeTrue();
        varBind.ScopeNumber.Should().Be(0);

        compound.GetMethod(methBind).Name.Should().Be("Test");
        compound.GetVariable(methBind).Name.Should().Be("TestField");

    }
    
    [Fact]
    public void Master_With_Inner()
    {
        var master = new SymbolTable();
        master.PushScope(new SymbolScope(), null);

        master.DefineMethod(BslMethodBuilder.Create().Name("Test").Build().ToSymbol());
        master.DefineVariable(BslFieldBuilder.Create().Name("TestField").Build().ToSymbol());
        
        var compound = new CompoundSymbolTable(master);
        compound.PushScope(new SymbolScope(), null);

        compound.ScopeCount.Should().Be(2);
        
        compound.GetScope(0).Should().BeSameAs(master.GetScope(0));
        compound.TryFindMethod("Test", out _).Should().BeTrue();
        compound.TryFindMethodBinding("Test", out var methBind).Should().BeTrue();
        methBind.ScopeNumber.Should().Be(0);
        
        compound.FindVariable("TestField", out var varBind).Should().BeTrue();
        varBind.ScopeNumber.Should().Be(0);

        compound.GetMethod(methBind).Name.Should().Be("Test");
        compound.GetVariable(methBind).Name.Should().Be("TestField");

        compound.DefineMethod(BslMethodBuilder.Create().Name("InnerTest").Build().ToSymbol());
        compound.DefineVariable(BslFieldBuilder.Create().Name("InnerTestField").Build().ToSymbol());
        
        //
        compound.TryFindMethod("InnerTest", out _).Should().BeTrue();
        compound.TryFindMethodBinding("InnerTest", out methBind).Should().BeTrue();
        methBind.ScopeNumber.Should().Be(1);
        
        compound.FindVariable("InnerTestField", out varBind).Should().BeTrue();
        varBind.ScopeNumber.Should().Be(1);

        compound.GetMethod(methBind).Name.Should().Be("InnerTest");
        compound.GetVariable(methBind).Name.Should().Be("InnerTestField");
        
        
    }
}
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.StandardLibrary.Processes;
using Xunit;

namespace OneScript.Core.Tests;

public class ArgumentParserTests
{
    [Theory]
    [InlineData("-c 'oscript -version'", "-c", "oscript -version")]
    [InlineData("-c oscript -version", "-c", "oscript", "-version")]
    [InlineData("-c '\"oscript\" -version'", "-c", "\"oscript\" -version")]
    [InlineData("-c \"'oscript' -version\"", "-c", "'oscript' -version")]
    [InlineData("'aaa\"", "aaa\"")]
    [InlineData(" ")]
    public void Should_Parse_Arguments(string input, params string[] expected)
    {
        var parser = new ArgumentsParser(input);

        var actual = parser.GetArguments();

        actual.Should().Equal(expected);
    }
}
using FluentAssertions;
using OneScript.StandardLibrary.Processes;
using Xunit;

namespace OneScript.Core.Tests;

public class ArgumentParserTests
{
    [Theory]
    [InlineData("-c 'oscript -version'", "-c", "'oscript -version'")]
    [InlineData("-c oscript -version", "-c", "oscript", "-version")]
    [InlineData("-c '\"oscript\" -version'", "-c", "'\"oscript\" -version'")]
    [InlineData("-c \"'oscript' -version\"", "-c", "\"'oscript' -version\"")]
    [InlineData(" ")]
    [InlineData("'aaa\"", "'aaa\"")]
    public void Should_Parse_Arguments(string input, params string[] expected)
    {
        var parser = new ArgumentsParser(input);

        var actual = parser.GetArguments();

        actual.Should().Equal(expected);
    }
}
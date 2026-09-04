using Dassie.Syntax.Helpers;
using Assert = Xunit.Assert;

namespace Dassie.Tests.Syntax.Helpers;

public class StringHelpersTests
{
    [Fact]
    public static void EscapeString_EscapesDassieSpecialCharacters()
    {
        string input = "\0\a\b\e\f\n\r\t\v\"^";
        string escaped = StringHelpers.EscapeString(input);

        Assert.Equal("^0^a^b^e^f^n^r^t^v^\"^^", escaped);
    }

    [Fact]
    public static void EscapeString_LeavesOrdinaryCharactersUnchanged()
    {
        Assert.Equal("hello, world! λ", StringHelpers.EscapeString("hello, world! λ"));
    }

    [Fact]
    public static void UnescapeString_UnescapesDassieSpecialCharacters()
    {
        string escaped = "^0^a^b^e^f^n^r^t^v^\"^^";
        string unescaped = StringHelpers.UnescapeString(escaped);

        Assert.Equal("\0\a\b\e\f\n\r\t\v\"^", unescaped);
    }

    [Theory]
    [InlineData("^x41", "A")]
    [InlineData("^x41Z", "AZ")]
    [InlineData("^u0041", "A")]
    [InlineData("^U00000041", "A")]
    [InlineData("^U0001F47D", "👽")]
    public static void UnescapeString_UnescapesUnicodeSequences(string escaped, string expected)
    {
        Assert.Equal(expected, StringHelpers.UnescapeString(escaped));
    }

    [Fact]
    public static void EscapeString_AndUnescapeString_RoundTrip()
    {
        const string input = "hello\r\nworld\t^\"λ";
        string roundTripped = StringHelpers.UnescapeString(StringHelpers.EscapeString(input));

        Assert.Equal(input, roundTripped);
    }

}
﻿using System.Collections;
using System.Globalization;
using Xunit.Abstractions;

namespace ClosedXML.Parser.Tests;

public class NameUtilsTests
{
    private readonly ITestOutputHelper _output;

    public NameUtilsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("A", false)] // First char - Letter-like
    [InlineData("Z", false)]
    [InlineData("㝳", false)]
    [InlineData("㏝", false)]
    [InlineData(" ", true)] // First char - non-letter-like
    [InlineData(".", true)]
    [InlineData("^", true)]
    [InlineData("!", true)]
    [InlineData("+", true)]
    [InlineData("0", true)]
    [InlineData("㏞", true)]
    [InlineData("😁", true)] // Symbol, non base multilingual plane
    [InlineData("AZ", false)] // Subsequent char - letter-like
    [InlineData("A0", false)] // Subsequent char - number-like
    [InlineData("A.", false)] // Subsequent char - symbol-like
    [InlineData("A!", true)] // Subsequent char - other
    [InlineData("A+A", true)]
    [InlineData("A😁", true)]
    public void Name_should_be_quoted_when_first_char_is_letter_and_rest_letter_number_or_symbol(string sheetName, bool shouldBeQuoted)
    {
        Assert.Equal(shouldBeQuoted, NameUtils.ShouldQuote(sheetName));
    }

    [Fact]
    public void Empty_name_is_not_valid_sheet_name()
    {
        Assert.False(NameUtils.IsSheetNameValid(string.Empty));
    }

    [Fact]
    public void Sheet_name_can_have_at_most_31_chars()
    {
        Assert.True(NameUtils.IsSheetNameValid(new string('A', 31)));
        Assert.False(NameUtils.IsSheetNameValid(new string('A', 32)));
    }

    [Theory]
    [InlineData("*", false)]
    [InlineData("/", false)]
    [InlineData(":", false)]
    [InlineData("?", false)]
    [InlineData("[", false)]
    [InlineData("\\", false)]
    [InlineData("]", false)]
    [InlineData("name", true)]
    [InlineData(" ", true)]
    public void Sheet_name_without_forbidden_chars(string name, bool isValid)
    {
        Assert.Equal(isValid, NameUtils.IsSheetNameValid(name));
    }

    [Theory(Skip = "Used to generate bitmask for the quotation.")]
    [InlineData(@"ident-sheet-first.txt")]
    [InlineData(@"ident-sheet-next.txt")]
    public void Generate_sheet_quotation_data(string path)
    {
        // Files were generated by AutoHotKey script that changed the name of a sheet and checked formula
        // referencing the sheet. There is no obvious pattern. Codepoints from Unicode 5.2+ are always quoted,
        // at least for BMP. Other than that, MS folk creativity at work.
        // The invalid code points for sheet name are not in the file: * 2A,/ 2F,: 3A,? 3F,[ 5B,\ 5C,] 5D
        var codepointQuoted = new BitArray(0x10000);
        foreach (var line in File.ReadAllLines(path))
        {
            var codepoint = int.Parse(line[..4], NumberStyles.HexNumber);
            var isQuoted = line[5..] switch
            {
                "YES" => true,
                "NO" => false,
                _ => throw new NotSupportedException(line)
            };
            codepointQuoted[codepoint] = isQuoted;
        }

        var intArray = new int[65536 / 32];
        codepointQuoted.CopyTo(intArray, 0);
        _output.WriteLine(string.Join(", ", intArray.Select(x => "0x" + x.ToString("X8"))));
    }
}
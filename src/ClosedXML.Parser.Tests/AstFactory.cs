﻿#nullable disable

using Antlr4.Runtime;
using ClosedXML.Lexer;
using System.Globalization;

namespace ClosedXML.Parser.Tests;

public readonly record struct ScalarValue(string Type, string Value);

internal record AstNode(string Type, string Text);

internal class F : IAstFactory<ScalarValue, AstNode>
{
    public ScalarValue BlankValue(bool value)
    {
        return new ScalarValue("Blank", string.Empty);
    }

    public ScalarValue LogicalValue(bool value)
    {
        return new ScalarValue("Logical", value ? "TRUE" : "FALSE");
    }

    public ScalarValue NumberValue(double value)
    {
        return new ScalarValue("Number", value.ToString(CultureInfo.InvariantCulture));
    }

    public ScalarValue TextValue(ReadOnlySpan<char> input)
    {
        return new ScalarValue("Text", input.ToString());
    }

    public ScalarValue ErrorValue(string input, int firstIndex, int length)
    {
        return new ScalarValue("Error", input.Substring(firstIndex, length));
    }

    public AstNode BlankNode()
    {
        return new AstNode("Blank", string.Empty);
    }

    public AstNode LogicalNode(bool value)
    {
        return new AstNode("Logical", value ? "TRUE" : "FALSE");
    }

    public AstNode ErrorNode(string input, int firstIndex, int length)
    {
        return new AstNode("Error", input.Substring(firstIndex, length));
    }

    public AstNode NumberNode(double value)
    {
        return new AstNode("Number", value.ToString(CultureInfo.InvariantCulture));
    }

    public AstNode TextNode(ReadOnlySpan<char> text)
    {
        return new AstNode("Text", text.ToString());
    }

    public AstNode ArrayNode(int rows, int columns, IList<ScalarValue> array)
    {
        return new AstNode("Array", $"{{{rows}x{columns}}}");
    }

    public AstNode LocalCellReference(string input, int firstIndex, int length)
    {
        return default;
    }

    public AstNode ExternalCellReference(string input, int firstIndex, int length)
    {
        return default;
    }

    public AstNode Function(ReadOnlySpan<char> name, IList<AstNode> args)
    {
        return default;
    }

    public AstNode Function(ReadOnlySpan<char> name, AstNode[] args)
    {
        return default;
    }

    public AstNode StructureReference(ReadOnlySpan<char> intraTableReference)
    {
        return default;
    }

    public AstNode StructureReference(ReadOnlySpan<char> tableName, ReadOnlySpan<char> intraTableReference)
    {
        return default;
    }

    public AstNode StructureReference(ReadOnlySpan<char> bookPrefix, ReadOnlySpan<char> tableName, ReadOnlySpan<char> intraTableReference)
    {
        return default;
    }

    public AstNode LocalNameReference(ReadOnlySpan<char> name)
    {
        return default;
    }

    public AstNode LocalNameReference(ReadOnlySpan<char> sheet, ReadOnlySpan<char> name)
    {
        return default;
    }

    public AstNode ExternalNameReference(ReadOnlySpan<char> bookPrefix, ReadOnlySpan<char> name)
    {
        return default;
    }

    public AstNode BinaryNode(char operation, AstNode leftNode, AstNode rightNode)
    {
        return default;
    }

    public AstNode Unary(char operation, AstNode node)
    {
        return default;
    }
}
﻿namespace ClosedXML.Parser;

public record ExternalReference3DNode(int WorkbookIndex, string FirstSheet, string LastSheet, ReferenceSymbol Reference) : AstNode
{
    public override string GetDisplayString(ReferenceStyle style)
    {
        return $"[{WorkbookIndex}]{FirstSheet}:{LastSheet}!{Reference.GetDisplayString(style)}";
    }
}
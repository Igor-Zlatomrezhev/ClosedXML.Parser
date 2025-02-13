﻿using System.Collections.Generic;
using System.IO;
using System;

namespace ClosedXML.Parser.Rolex;

internal class RolexLexer
{
    /// <summary>
    /// Get all tokens for a formula. Use A1 semantic. If there is an error, add token with an error symbol at the end or EOF token at the end.
    /// </summary>
    /// <param name="formula">Formula to parse.</param>
    public static List<Token> GetTokensA1(ReadOnlySpan<char> formula)
    {
        return GetTokens(formula, RolexA1Dfa.DfaTable);
    }

    /// <summary>
    /// Get all tokens for a formula. Use R1C1 semantic. If there is an error, add token with an error symbol at the end or EOF token at the end.
    /// </summary>
    /// <param name="formula">Formula to parse.</param>
    public static List<Token> GetTokensR1C1(ReadOnlySpan<char> formula)
    {
        return GetTokens(formula, RolexR1C1Dfa.DfaTable);
    }

    private static List<Token> GetTokens(ReadOnlySpan<char> formula, DfaEntry[] lexerDfa)
    {
        var tokens = new List<Token>();
        for (var i = 0; i < formula.Length;)
        {
            var (symbolId, length) = GetToken(formula, i, lexerDfa);
            tokens.Add(new Token(symbolId, i, length));
            if (symbolId < 0)
            {
                return tokens;
            }

            i += length;
        }

        tokens.Add(Token.EofSymbol(formula.Length));
        return tokens;
    }

    private static int Next(ReadOnlySpan<char> input, ref int index)
    {
        var c = input[index];
        if (char.IsHighSurrogate(c))
        {
            if (index >= input.Length)
                throw new IOException("Unexpected end of input while looking for Unicode low surrogate.");

            var lowSurrogate = input[index + 1];
            index += 2;
            return char.ConvertToUtf32(c, lowSurrogate);
        }

        ++index;
        return c;
    }

    private static (int SymbolId, int Length) GetToken(ReadOnlySpan<char> input, int startIdx, DfaEntry[] dfaTable)
    {
        int dfaState = 0;

        // Best token and its length found so far
        var symbolId = Token.ErrorSymbolId;
        var symbolEndIndex = 0;

        for (var idx = startIdx; idx < input.Length;)
        {
            var ch = Next(input, ref idx);

            // We are at some state and are looking for another state
            // That is indicated by a `found` flag.
            int nextDfaState = -1;
            for (var i = 0; i < dfaTable[dfaState].Transitions.Length; ++i)
            {
                DfaTransitionEntry entry = dfaTable[dfaState].Transitions[i];
                bool found = false;
                for (var j = 0; j < entry.PackedRanges.Length; ++j)
                {
                    int first = entry.PackedRanges[j];
                    j++;
                    int last = entry.PackedRanges[j];
                    if (ch <= last)
                    {
                        if (first <= ch)
                        {
                            found = true;
                        }
                        j = int.MaxValue - 1; // Equivalent of a break.
                    }
                }
                if (found)
                {
                    nextDfaState = entry.Destination;
                    i = int.MaxValue - 1; // Equivalent of a break.
                }
            }

            // No valid transition was found from dfaState
            if (nextDfaState == -1)
            {
                // Return best symbolId. If nothing was yet found, it returns -2 as an error.
                if (symbolId < 0)
                    return (Token.ErrorSymbolId, 0);

                // Adjust symbol because ANTLR and Rolex are 1 off
                return (symbolId + 1, symbolEndIndex - startIdx);
            }

            // There is a valid transition.
            dfaState = nextDfaState;
            var currentSymbolId = dfaTable[dfaState].AcceptSymbolId;

            // The substring from startIdx to the current idx are a valid token
            if (currentSymbolId != -1)
            {
                symbolId = currentSymbolId;
                symbolEndIndex = idx;
            }
        }

        // We are at the end of the input. If there is a valid symbol, return it.
        if (symbolId >= 0)
        {
            // Adjust symbol because ANTLR and Rolex are 1 off
            return (symbolId + 1, symbolEndIndex - startIdx);
        }

        // Otherwise, end of stream, but the only way we could got here is if there has been a half-inserted
        // symbol, e.g. `"Text ` without the ending. This method is not called when lexer is at the end of
        // a stream, thus it has to be an error.
        return (Token.ErrorSymbolId, 0);
    }
}
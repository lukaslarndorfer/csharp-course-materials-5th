using System;
using System.Linq;

namespace Calculator;

public static class CalculationLogic
{
    private const string ClearKey = "Clear";
    private const char DecimalSeparator = '.';
    private static readonly char[] Operators = ['+', '*', '-', '/'];

    public static string HandleCalculationInput(string current, string input)
    {
        if (input == ClearKey)
        {
            return "";
        }

        // Clear is already filtered out
        if (input.Length != 1)
        {
            return current; // invalid input
        }

        var c = input[0];

        if (current.Length == 0)
        {
            return char.IsDigit(c) ? input : current; // only digits allowed in first place
        }

        var last = current[^1];

        if (c == DecimalSeparator)
        {
            return CurrentNumberHasDecimal(current) || IsOperator(last) // cant have a decimal separator after an operator nor a digit with more than one separator
                ? current 
                : current + c; 
        }

        if (!IsOperator(c))
        {
            return current + c; // number is always allowed
        }
        
        return IsOperator(last) || last == DecimalSeparator 
            ? current[..^1]  + c // substitute operator or drop dangling separator
            : current   ;
    }

    private static bool IsOperator(char c)
        => Enumerable.Contains(Operators, c);

    private static bool CurrentNumberHasDecimal(string current)
    {
        var start = current.LastIndexOfAny(Operators) + 1;

        return current[start..].Contains(DecimalSeparator);
    }
}
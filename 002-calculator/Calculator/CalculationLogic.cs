using System;
using System.Globalization;
using System.Linq;
using NCalc;

namespace Calculator;

public static class CalculationLogic
{
    private const string ClearKey = "Clear";
    private const char DecimalSeparator = '.';

    private static readonly char[] Operators = ['+', '*', '-', '/'];

    public static string HandleInput(string current, string input)
    {
        if (input == ClearKey)
        {
            return "";
        }

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

        if (IsOperator(last))
        {
            return current; // ignore operator after operator
        }

        return last == DecimalSeparator
            ? current[..^1] + c // drop dangling separator
            : current + c;
    }
    

    private static bool IsOperator(char c)
        => Enumerable.Contains(Operators, c);

    private static bool CurrentNumberHasDecimal(string current)
    {
        var start = current.LastIndexOfAny(Operators) + 1;

        return current[start..].Contains(DecimalSeparator);
    }
    
    public static (double? result, string? error) Evaluate(string calculation)
    {
        try
        {
            var cleaned = calculation.Trim().TrimEnd(Operators); // NCalc throws if there are trailing operators
            if (cleaned.Length == 0)
            {
                return (null, null); // nothing to calculate, nothing to show
            }
            var expression = new Expression(cleaned);
            var evaluation = expression.Evaluate();
            var result = Convert.ToDouble(evaluation, CultureInfo.InvariantCulture);
            return (result, null);
        }
        
        catch (Exception ex)
        {
            var error = ex.Message;
            return (null, error);
        }
        

    }

}
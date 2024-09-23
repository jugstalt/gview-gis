using System;
using System.Globalization;

namespace gView.Blazor.Core.Extensions;
static public class LabelStringExtensions
{
    public static string ApplyLabelCommand(this string input, LabelCommand command)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        switch (command)
        {
            case LabelCommand.ToUpperCase:
                return input.ToUpper();

            case LabelCommand.ToLowerCase:
                return input.ToLower();

            case LabelCommand.ToCapitalizedCase:
                return ToCapitalizedCase(input);

            case LabelCommand.ToSentenseCase:
                return ToSentenceCase(input);

            case LabelCommand.ToTitleCase:
                return ToTitleCase(input);

            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    private static string ToCapitalizedCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }

        return string.Join(" ", words);
    }

    private static string ToSentenceCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string lowerInput = input.ToLower();
        return char.ToUpper(lowerInput[0]) + lowerInput.Substring(1);
    }

    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }
}

public enum LabelCommand
{
    ToUpperCase,
    ToLowerCase,
    ToCapitalizedCase,
    ToSentenseCase,
    ToTitleCase
}
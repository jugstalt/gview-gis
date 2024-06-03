namespace gView.Cmd;

internal class CommandLinePro
{
    static List<string> suggestions = new List<string> { "--help", "--version", "--update" };
    static int suggestionIndex = 0;

    static public string ReadLineWithAdvancedAutoComplete()
    {
        string input = "";
        ConsoleKeyInfo key;
        bool showSuggestions = false;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Tab && showSuggestions)
            {
                input += suggestions[suggestionIndex].Substring(input.Length);
                Console.Write(suggestions[suggestionIndex].Substring(input.Length));
                showSuggestions = false;
            }
            else if ((key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.UpArrow) && showSuggestions)
            {
                suggestionIndex += key.Key == ConsoleKey.DownArrow ? 1 : -1;
                if (suggestionIndex >= suggestions.Count)
                {
                    suggestionIndex = 0;
                }

                if (suggestionIndex < 0)
                {
                    suggestionIndex = suggestions.Count - 1;
                }

                ShowPartialSuggestion(input);
            }
            else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input = input.Substring(0, input.Length - 1);
                Console.Write("\b \b");
                showSuggestions = input.StartsWith("-");
                if (showSuggestions)
                {
                    ShowPartialSuggestion(input);
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                Console.Write(key.KeyChar);
                input += key.KeyChar;
                showSuggestions = input.StartsWith("-");
                if (showSuggestions)
                {
                    ShowPartialSuggestion(input);
                }
            }
        } while (key.Key != ConsoleKey.Enter);

        return input;
    }

    static void ShowPartialSuggestion(string input)
    {
        suggestionIndex = 0;  // Reset suggestion index
        var match = suggestions.FirstOrDefault(s => s.StartsWith(input));
        if (match != null)
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));  // Clear line
            Console.CursorLeft = 0;
            Console.Write(input);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(match.Substring(input.Length));  // Show suggestion in gray
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorLeft = input.Length;  // Reset cursor position after input
        }
    }
}

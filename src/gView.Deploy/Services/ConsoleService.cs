using gView.Deploy.Extensions;
using gView.Deploy.Reflection;
using gView.Security.Extensions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace gView.Deploy.Services;

class ConsoleService
{
    public void WriteUsageMessage()
    {
        Console.WriteLine(Properties.Resources.usage);
    }

    public string ChooseFrom(IEnumerable<string> items, string label, bool allowNewValues = false, string examples = "")
    {
        var itemArray = items.ToArray();

        Console.Write($"Choose a {label}");
        if (allowNewValues)
        {
            Console.Write(" or create a new by enter an unique name");
        }
        if (!string.IsNullOrEmpty(examples))
        {
            Console.Write($", eg. {examples}");
        }
        Console.WriteLine();

        for (int i = 0; i < Math.Min(itemArray.Length, 5); i++)
        {
            Console.WriteLine($"{i} ... {itemArray[i]}");
        }

        while (true)
        {
            if (allowNewValues)
            {
                Console.Write($"Enter a new {label} name");
                if (itemArray.Length > 0) Console.Write(" or ");
            }
            if (itemArray.Length > 0)
            {
                Console.Write($"input {label} index [0]");
            }

            Console.Write(": ");

            var indexString = Console.ReadLine();

            if (string.IsNullOrEmpty(indexString))
            {
                indexString = "0";
            }

            if (int.TryParse(indexString, out int versionIndex))
            {
                if (versionIndex < 0 || versionIndex >= itemArray.Length)
                {
                    Console.WriteLine("Invalid input...");
                }
                else
                {
                    Console.WriteLine();

                    return itemArray[versionIndex];
                }
            }
            else
            {
                if (allowNewValues && !string.IsNullOrWhiteSpace(indexString))
                {
                    Console.WriteLine();

                    return indexString;
                }

                Console.WriteLine("Invalid input...");
            }
        }
    }

    public bool DoYouWantToContinue() => DoYouWant("to continue");

    public bool DoYouWant(string prompt, char defaultInput = 'Y')
    {
        Console.Write($"Do you want {prompt}? Y/N [{defaultInput.ToString().ToUpper()}]");

        while (true)
        {
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                input = defaultInput.ToString();

            if (input.Trim().ToLower() == "y")
            {
                return true;
            }
            else if (input.Trim().ToLower() == "n")
            {
                return false;
            }
            else
            {
                Console.Write("Invalid Input... Y/N [Y]");
            }
        }
    }

    public bool InputRequiredModelProperties(object model)
    {
        bool hasChanged = false;
        var modelType = model.GetType();

        foreach (var property in modelType.GetProperties())
        {
            var modelPropertyAttr = property.GetCustomAttribute<ModelPropertyAttribute>();
            if (modelPropertyAttr != null)
            {
                var val = property.GetValue(model, null)?.ToString();

                if (String.IsNullOrEmpty(val))
                {
                    var defaultValue = modelPropertyAttr.GetDefaultValue(model);

                    while (true)
                    {
                        Console.Write($"{modelPropertyAttr.Prompt} [{defaultValue}]: ");
                        val = modelPropertyAttr.IsPassword
                            ? InputPassword()?.Trim()
                            : Console.ReadLine()?.Trim();

                        if (String.IsNullOrEmpty(val))
                        {
                            val = defaultValue;
                        }

                        if (!String.IsNullOrEmpty(val))
                        {
                            if (!String.IsNullOrEmpty(modelPropertyAttr.RegexPattern))
                            {
                                if (!Regex.IsMatch(val, modelPropertyAttr.RegexPattern))
                                {
                                    Console.WriteLine($"Value don't match pattern: {modelPropertyAttr.RegexNotMatchMessage}");
                                    continue;
                                }
                            }

                            val = modelPropertyAttr.PropertyFormat switch
                            {
                                PropertyFormat.Hash256 => val.ToSha256Hash(),
                                PropertyFormat.Hash512 => val.ToSha512Hash(),   
                                _ => val
                            };

                            property.SetValue(model, val, null);
                            hasChanged = true;

                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{modelPropertyAttr.Prompt ?? property.Name}: {val}");
                }
            }
        }

        return hasChanged;
    }

    private string InputPassword()
    {
        string passwort = "";
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);

            if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
            {
                passwort += keyInfo.KeyChar;
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && passwort.Length > 0)
            {
                passwort = passwort.Substring(0, passwort.Length - 1);
                Console.Write("\b \b"); // backshift, space, backshift
            }
        } while (keyInfo.Key != ConsoleKey.Enter);
        Console.WriteLine();

        return passwort;
    }

    public void WriteCharLine(char character)
    {
        Console.Write(new string(character, Console.WindowWidth));
    }

    public void WriteBlock(string message, char blockChar = '*')
    {
        Console.WriteLine();
        Console.WriteLine();
        WriteCharLine(blockChar);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
        WriteCharLine(blockChar);
        Console.WriteLine();
        Console.WriteLine();
    }
}

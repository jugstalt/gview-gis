using gView.Deploy.Extensions;
using gView.Deploy.Reflection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace gView.Deploy.Services;

class ConsoleService
{
    public void WriteUsageMessage()
    {
        Console.WriteLine(Properties.Resources.usage);
    }

    public string ChooseFrom(IEnumerable<string> items, string label, bool allowNewVales = false, string examples = "")
    {
        var itemArray = items.ToArray();

        Console.Write($"Choose a {label}");
        if (allowNewVales)
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
            Console.Write($"Input {label} index [0]: ");
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
                if (allowNewVales && !string.IsNullOrWhiteSpace(indexString))
                {
                    Console.WriteLine();

                    return indexString;
                }

                Console.WriteLine("Invalid input...");
            }
        }
    }

    public bool DoYouWantToContinue()
    {
        Console.Write("Do you want to continue Y/N [Y]");

        while (true)
        {
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.Trim().ToLower() == "y")
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
                        val = Console.ReadLine()?.Trim();

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

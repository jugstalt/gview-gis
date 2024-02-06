using gView.Cmd.Core.Abstraction;
using System.Reflection;

namespace gView.Cmd.Services;
internal class CommandCollectionService
{
    private readonly IEnumerable<Type> _commandTypes;

    public CommandCollectionService()
    {
        _commandTypes = GetCommandTypes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
    }

    public IEnumerable<ICommand> Instances
        => _commandTypes
                .Select(t => (ICommand)Activator.CreateInstance(t))
                .Where(t => t != null)!;

    public ICommand? CommandByName(string name)
        => Instances
            .Where(i => name.Equals(i.Name, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

    #region Helper

    private IEnumerable<Type> GetCommandTypes(string directoryPath)
    {
        var commandTypes = new List<Type>();
        var dllFiles = Directory.GetFiles(directoryPath, "gView.Cmd.*.Lib.dll");

        foreach (var file in dllFiles)
        {
            var assembly = Assembly.LoadFrom(file);
            var types = assembly.GetTypes()
                                .Where(t => t.IsClass
                                        && !t.IsAbstract
                                        && typeof(ICommand).IsAssignableFrom(t)
                                        && t.GetConstructor(Type.EmptyTypes) != null);

            commandTypes.AddRange(types);
        }

        return commandTypes;
    }

    #endregion
}

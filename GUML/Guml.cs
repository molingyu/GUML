using System.Reflection;
using Godot;

namespace GUML;

public class TypeNotFoundException(string msg) : Exception(msg);

public class TypeErrorException(string msg) : Exception(msg);

public static class Guml
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly Dictionary<string, GuiController> TopControllers = new ();
    /// <summary>
    /// 
    /// </summary>
    public static Func<string, object> ResourceLoader;
    
    public static readonly List<Assembly> Assemblies = [];
    public static readonly Dictionary<string, object> GlobalRefs = new ();
    
    public static readonly GumlParser Parser = new ();

    /// <summary>
    /// 
    /// </summary>
    public static void Init()
    {
        Parser.WithConverter(new KeyConverter());
        Assemblies.Add(typeof(Guml).Assembly);
        Assemblies.Add(typeof(Node).Assembly);
    }

    public static object GetResource(string path)
    {
        return ResourceLoader.Invoke(path);
    }

    public static GuiController LoadGuml(Node root, string path)
    {
        var importPath = Path.GetDirectoryName(Path.GetFullPath(path)) ?? throw new InvalidOperationException();
        var controllerName = $"{KeyConverter.ToPascalCase(Path.GetFileNameWithoutExtension(path))}Controller";
        var controller = Activator.CreateInstance(FindType(controllerName)) as GuiController ?? throw new InvalidOperationException();
        GumlRenderer.Render(Parser.Parse(File.ReadAllText(path)), controller, root, importPath);
        return controller;
    }

    public static Type FindType(string name)
    {
        foreach (var type in Assemblies.Select(assembly => assembly.GetType(name)).OfType<Type>())
        {
            return type;
        }

        throw new TypeNotFoundException(name);
    }

}

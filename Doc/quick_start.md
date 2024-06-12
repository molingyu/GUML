# Quick Start

## Install

use nuget:
```
dotnet add package GUML --version 0.0.2
```

## initialization
- Calling the GUML initialization function
```c#
Guml.Init();
```
- Add Controller Assembly
```c#
Guml.Assemblies.Add(typeof(CONTROLLER_CLASS).Assembly);
```
If the Controller is scattered in multiple Assemblies, all need to be added.

For example, if multiple Mods define new Controllers, you need to add the Assembly corresponding to each Mod.

- Add Controller Namespace
```c#
Guml.ControllerNamespaces.Add("CONTROLLER_NAMESPACE");
```

- Implement resource loader
```c#
Guml.ResourceLoader += resPath => {
    // ResourceLoader Implement
}
```
**GUML** does not provide a resource loader by default. This is to facilitate collaboration with the game's own runtime resource loading cache. You can implement your own loader and design the relevant resource caching algorithm according to your needs.

- Load `.guml` file
```c#
Guml.LoadGuml(Root,"gui/main.guml");
```
The first parameter of the `Guml.LoadGuml` method is the root node of the GUI. The second parameter is the address of the guml file.
### Example
```c#
using Godot;
using GUML;

public partial class Main : Node
{
    [Export]
	public Node Root;

	private GuiController _controller;
    
    public override void _Ready()
	{
		Guml.Init();
		Guml.Assemblies.Add(typeof(MainController).Assembly);
		Guml.ControllerNamespaces.Add("GUI");
		Guml.ResourceLoader += resPath =>
		{
			var type = Path.GetExtension(resPath);
			switch (type)
			{
				case "png":
					return ImageTexture.CreateFromImage(Image.LoadFromFile(resPath));
				case "ogg":
					return AudioStreamOggVorbis.LoadFromFile(resPath);
				case "ttf":
				case "woff":
				case "woff2":
				case "pfb":
				case "pfm":
				case "otf":
					var font = new FontFile();
					font.LoadDynamicFont(resPath);
					return font;
				default:
					throw new Exception($"Error resource type('{type}').");
			}
		};
        _controller = Guml.LoadGuml(Root,"gui/main.guml");
	}
   
   	public override void _Process(double delta)
	{
		_controller?.Update();

	}

	public override void _ExitTree()
	{
		_controller?.Dispose();
	} 
}
```

## `XxxController` class and `xxx.guml` file
Every guml file has a corresponding `Controller` class. **GUML** will automatically create its corresponding 
controller class instance and bind the two when loading the `guml` file.

Of course, you need to add the controller corresponding to Assembly and namespace to the `Guml.Assemblies` and 
`Guml.ControllerNamespaces` lists. This will allow you to find the correct controller type when creating it.

## Controller object
Controller object all inherit from `GuiController` and have the `INotifyPropertyChanged` interface. 

### Controller lifecycle overview:

| Method   | When                                                            | Description    |
|----------|-----------------------------------------------------------------|----------------|
| Created  | After object initialization and binding width guml is completed | Automatic call |
| Update   | Executed every frame, same as `Node._Process`                   | Manual call    |
| Dispose* | When the Controller is destroyed                                | Manual call    |

*`Dispose` will automatically remove the bound GUI node from the Godot node tree*

### NameNode and GumlRootNode
In guml, you can reference the required components at any time by using component aliases. These alias components 
can be accessed through `NameNode` in the controller.

### `import` Syntax
The guml file supports importing another defined guml file through the `import` keyword. 

If you use `import`, you need to define a property with the same name and type as the controller corresponding to 
the imported guml file in the controller class corresponding to the current file, so that the controller corresponding to the imported guml file can be bound to the current controller when it is imported.

### Example:

`main.guml`
```
import "setting"

Panel {
    @hello_label: Label {
        text: "hello"
    }
}
```
`MainController.cs`
```c#
class MainController : RguiController 
{
    public SettingController SettingController;
    
    public override void Created()
    {
        GD.Print(NameNode["hello_label"].); //print hello
    }
}
```
The root node of guml will be assigned to the GumlRootNode property of Controller after binding.

## Data binding

**GUML** supports one-way binding from data to UI components.In guml files, data binding is done using the `:=` 
syntax. Of course, it not only supports property binding, but also supports expression binding, so that when the properties in the expression change, the properties of the UI component will also change accordingly.

It should be noted that all properties involved in the binding expression need to write setters so that `OnPropertyChanged` is called when the value changes.

The datasource specified by the each syntax must be a collection type that implements the `INotifyListChanged`
interface. **GUML** provides the `NotifyList<T>` type, which is similar to `List<T>`, but implements the
`INotifyListChanged` interface
to trigger the `ListChanged` event when collection elements are added or deleted.

Example:

`main.guml`
```
Panel {
    Label {
        position: vec2(10, 10),
        size: vec2(200, 30),
        text:= "hello " + $controller.SayHello
    }
    
    each $controller.Names { |index, name|
        Control {
            size: vec2(220, 100),
            custom_minimum_size: vec2(220, 100),
            Label {
                position: vec2(10, 10),
                size: vec2(200, 30),
                text: name
            }
        }
    }         
}
```
`MainController.cs`
```c#
public class MainController : GuiController
{
    public NotifyList<string> Names {
        get => _names;
        set
        {
            _names = value;
            OnPropertyChanged();
        }
        
    }
    
    public string SayHello {
        get => _sayHello;
        set
        {
            _sayHello = value;
            OnPropertyChanged();
        }
    }

    private string _sayHello = "world!";
    public NotifyList<string> _names = ["bob", "james"];
}
```
When we update `MainController.SeyHello`, the text on the Label will also change.

## guml file
Regarding guml files, a more detailed description can be found in the [guml syntax](guml_syntax.md) section.

## Theme overrides
In the godot editor, you can use `ThemeOverrides` to set theme overrides for UI components. But there is no direct 
interface in the code. **GUML** provides a fake `ThemeOverrides` property. Its value is an object object. You can use the 
same properties to set the UI component's theme overrides as in the editor. 

The `ThemeOverrides` static property of the Guml class provides all optional theme override entries for a component and specifies their data types.

The `DefaultTheme` static property of Guml can set the default theme for all UI components.
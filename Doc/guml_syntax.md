# GUML Syntax

## Variable
GUML supports the following variables:
- Alias variable
- Global variable
- Local variable

### Alias variable
Alias for ui node. So that the corresponding ui node can be referenced in guml file or corresponding controller.

### Global variable
Global variables bound externally via Guml.GlobalRefs . The scope is the entire guml file. Among them, `$controller` always points to the instance of the controller class corresponding to the current guml file.

### Local variable
Used in `each` node to represent `each` parameter. Its scope is only within the corresponding `each` node.

## Import Statements
A GUML document may have one or more imports at the top of the file.

Syntax:
```
import <guml path>
import_top <guml path>
```
- `guml path` is a string, path to the guml file to load. Can be an absolute path or a relative path to the current 
  file.

`import` will mount the loaded component under the current node, while `import_top` will mount it on the root node.

## Redirect Statement
Used to redirect the controller corresponding to the current guml file.

Syntax:
```
redirect <redirect controller>
```
- `redirect controller` is a string, the type name of the redirected controller.


## Component Declarations
Syntax:
``` 
<alias name> <component type> {
    <property name>: <property value>,
    <signal name>: <method name>
}
```
- `alias name`
- `component type` must start with capital letter. Its value can be the component name of any UI component supported 
  by Godot.
- `property name`
- `property value`
- `signal name` is the signal name supported by the current component. It must start with `#` and be named using the underscore notation.
- `method name` is a string whose value corresponds to the method name.

### Value Type
- `string`:  Similar to C#'s `string`.
- `int`: Similar to C#'s `int`.
- `float`: Similar to C#'s `float`. Does not support scientific notation.
- `boolean`: `true` or `false`.
- `object`: `{ <property name>: <property value> }`
- `ref`: All variable ref
- `null`
- `vector2`: `vec2(x: int, y: int)` used to represent Godot's Vector2.
- `color`: `color(r: float, g: float, b: float, a: float)` used to represent Godot's Color.
- `styleBox`: `style_box_empty()` `style_box_flat(properties: object)` `style_box_line(properties: object)` 
  `style_box_texture(properties: object)` used to represent Godot's StyleBox.
- `resource`: `resource(path: string)` used to represent fonts, images, or audio.

## Child Components
Any component declaration can define child components through nested component declarations. In this way, any component 
declaration implicitly declares an component tree that may contain any number of child components.

Example:
```
Panel {
    Label {
        text: "hello"
    }
    
    Panel {
        Button {}
        Button {}
    }
}
```

## List Rendering
GUML use the `each` syntax to render a list of items based on an `DataSource`.

Syntax:
```
each <data source> { | <index variable define>, <value variable define> |
    <each body>
}
```
- `data source` can be any variable.
- `index variable define` && `value variable define` Declare local variables of index and value, whose scope is valid 
  throughout each.
- `each body` is multiple component declarations. Note that each cannot be nested directly.

**❌Error:**
```
each $controller.DataA { |index_a, value_a|
    each $controller.DataB { |index_b, value_b|
        // some components
    }
}
```
**✔️Right:**
```
each $controller.DataA { |index_a, value_a|
    Control {
        each $controller.DataB { |index_b, value_b|
            // some components
        }
    }
}
```
## Comments 
GUML only has single line comments. Single line comments start with // and finish at the end of the line.

Syntax:
```
// This is a single line comment

import "setting" // This is also allowed
```

## Keywords
The component's property key cannot conflict with keywords.

`import` `import_top` `redirect` `each` `resource` `vec2` `color` `style_box_empty` `style_box_flat` `style_box_line` 
`style_box_texture`
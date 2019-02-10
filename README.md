# MessagePackGridView

this Editor Plugin depends on [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp)

## Usage

```csharp
var msgPackObj = MessagePackSerializer.Deserialize<ExampleType>(bytes);
var window = EditorWindow.GetWindow<MessagePackGridViewWindow>();
window.SetData(msgPackObj);
```

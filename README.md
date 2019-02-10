# MessagePackGridView

## Usage

```csharp
var msgPackObj = MessagePackSerializer.Deserialize<ExampleType>(bytes);
var window = EditorWindow.GetWindow<MessagePackGridViewWindow>();
window.SetData(msgPackObj);
```

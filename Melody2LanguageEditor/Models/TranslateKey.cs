using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Melody2LanguageEditor.Models;

public class TranslateKey : ReactiveObject
{
    [Reactive] public TranslateCategory Category { get; set; }
    [Reactive] public string Key { get; set; }
    [Reactive] public string Value { get; set; }
    
    public TranslateKey(TranslateCategory category, string key, string value)
    {
        Category = category;
        Key = key;
        Value = value;
    }
    
    public void ClearKeyValue()
    {
        Value = string.Empty;
    }

    public override string ToString()
    {
        return Key;
    }
}
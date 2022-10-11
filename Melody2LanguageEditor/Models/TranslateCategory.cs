using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Melody2LanguageEditor.Models;

public class TranslateCategory : ReactiveObject
{
    [Reactive] public string Title { get; set; }
    
    [Reactive] public ObservableCollection<TranslateKey> TranslateKeys { get; set; }
    
    public TranslateCategory(string title)
    {
        Title = title;
        TranslateKeys = new();
    }

    public override string ToString()
    {
        return $"{Title} ({TranslateKeys.Count})";
    }
    
    public void ClearAllKeyValues()
    {
        foreach (var translateKey in TranslateKeys)
        {
            translateKey.Value = string.Empty;
        }
    }
}
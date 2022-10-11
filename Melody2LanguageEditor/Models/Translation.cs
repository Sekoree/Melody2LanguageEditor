using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Melody2LanguageEditor.Models;

public class Translation : ReactiveObject
{
    [Reactive] public string LanguageTitle { get; set; }
    [Reactive] public ObservableCollection<TranslateCategory> Categories { get; set; }
    
    public Translation(string languageTitle)
    {
        LanguageTitle = languageTitle;
        Categories = new ObservableCollection<TranslateCategory>();
    }

    public override string ToString()
    {
        return LanguageTitle;
    }

    public void ClearAllCategoryKeyValues()
    {
        foreach (var category in Categories)
            category.ClearAllKeyValues();
    }
}
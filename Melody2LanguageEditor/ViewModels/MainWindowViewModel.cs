using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Melody2LanguageEditor.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Melody2LanguageEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private TranslateKey? _selectedKeyAsObject;
        private Translation? _customTranslation;

        [Reactive] public Translation? DefaultTranslation { get; set; }
        [Reactive] public TranslateKey? SelectedCustomKey { get; set; }

        [Reactive] public string? CurrentDefaultLocation { get; set; }
        [Reactive] public string? CurrentSaveLocation { get; set; }

        public Translation? CustomTranslation
        {
            get => _customTranslation;
            set
            {
                this.RaiseAndSetIfChanged(ref _customTranslation, value);
                this.RaisePropertyChanged(nameof(CanEdit));
            }
        }

        //Pain setter
        public TranslateKey? SelectedKey
        {
            get => _selectedKeyAsObject;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedKeyAsObject, value);
                if (CustomTranslation is null)
                    return;
                var category = CustomTranslation?.Categories.FirstOrDefault(c => c.Title == value.Category.Title);
                if (category is null)
                {
                    category = new TranslateCategory(value.Category.Title);
                    CustomTranslation?.Categories.Add(category);
                }

                var keyInCategory = category.TranslateKeys.FirstOrDefault(k => k.Key == value.Key);
                if (keyInCategory is null)
                {
                    keyInCategory = new TranslateKey(category, value.Key, string.Empty);
                    category.TranslateKeys.Add(keyInCategory);
                }

                SelectedCustomKey = keyInCategory;
            }
        }

        public bool CanEdit => CustomTranslation is not null;
        public bool DefaultTranslationLoaded => DefaultTranslation is not null;


        public MainWindowViewModel()
        {
            //_ = Dispatcher.UIThread.InvokeAsync(LoadDefaultTranslationAsync);
        }

        public async Task SetAndLoadDefaultTranslationAsync(Window window)
        {
            var filePickerOptions = new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text file")
                    {
                        Patterns = new[] { "*.txt" },
                        MimeTypes = new[] { "text/plain" }
                    }
                }
            };
            var file = await window.StorageProvider.OpenFilePickerAsync(filePickerOptions).ConfigureAwait(true);
            if (file.Count == 0)
                return;
            var couldPathUri = file.First().TryGetUri(out var pathUri);
            if (!couldPathUri)
                return;
            var path = pathUri!.LocalPath;
            //Dispatcher.UIThread.Post(() =>
            //{
            CurrentDefaultLocation = path;
            //});
            await LoadDefaultTranslationAsync().ConfigureAwait(true);
        }

        public async Task LoadDefaultTranslationAsync()
        {
            var defaultTranslation = await LoadTranslationFromFileAsync(CurrentDefaultLocation!).ConfigureAwait(true);
            if (defaultTranslation == null)
                throw new ArgumentNullException(nameof(defaultTranslation));
            Dispatcher.UIThread.Post(() =>
            {
                DefaultTranslation = defaultTranslation;
                SelectedKey = DefaultTranslation?.Categories.FirstOrDefault()?.TranslateKeys.FirstOrDefault();
                this.RaisePropertyChanged(nameof(DefaultTranslationLoaded));
            });
        }

        public async Task LoadCustomTranslationAsync(Window window)
        {
            var filePickerOptions = new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text files")
                    {
                        Patterns = new[] { "*.txt" },
                        MimeTypes = new[] { "text/plain" }
                    }
                }
            };
            var file = await window.StorageProvider.OpenFilePickerAsync(filePickerOptions).ConfigureAwait(true);
            if (file.Count == 0)
                return;
            var couldPathUri = file.First().TryGetUri(out var pathUri);
            if (!couldPathUri)
                return;
            var path = pathUri.LocalPath;
            var translation = await LoadTranslationFromFileAsync(path).ConfigureAwait(true);
            Dispatcher.UIThread.Post(() =>
            {
                CustomTranslation = translation;
                CurrentSaveLocation = path;
                SelectedKey = SelectedKey;
            });
        }

        public async Task LoadCustomTranslationAsync(string path)
        {
            CustomTranslation = await LoadTranslationFromFileAsync(path).ConfigureAwait(true);
            if (SelectedKey is not null)
            {
                SelectedKey = SelectedKey;
            }
        }

        public async Task NewTranslationAsync()
        {
            var defaultTranslation = await LoadTranslationFromFileAsync(CurrentDefaultLocation!).ConfigureAwait(true);
            defaultTranslation.ClearAllCategoryKeyValues();
            defaultTranslation.LanguageTitle = "Change Me";
            CustomTranslation = defaultTranslation;
        }

        private async Task<Translation> LoadTranslationFromFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            var lines = await File.ReadAllLinesAsync(path);
            //Language and LanguageLocalized on line 0 separated by ;
            var languageTitle = lines[0].Split(';')[1];
            var translation = new Translation(languageTitle);

            //translations start at line 2
            var lastCategory = default(TranslateCategory);
            for (var i = 2; i < lines.Length; i++)
            {
                // categories begin with a #
                // one translation key per line, the translation value is after the key seperated by a ;
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    i++;
                }

                if (lines[i].StartsWith("#"))
                {
                    var category = new TranslateCategory(lines[i].Substring(2));
                    translation.Categories.Add(category);
                    lastCategory = category;
                }
                else
                {
                    var split = lines[i].Split(';');
                    var key = split[0];
                    var value = split[1];
                    translation.Categories[^1].TranslateKeys.Add(new TranslateKey(lastCategory!, key, value));
                }
            }

            return translation;
        }

        public async Task SaveTranslationAsync(Window window)
        {
            if (CustomTranslation is null)
                return;
            if (CurrentSaveLocation is null)
            {
                await SaveTranslationAsAsync(window).ConfigureAwait(true);
                return;
            }

            await SaveCustomTranslationAsync(CurrentSaveLocation).ConfigureAwait(true);
        }

        public async Task SaveTranslationAsAsync(Window window)
        {
            if (CustomTranslation is null)
                return;
            var filePickerOptions = new FilePickerSaveOptions()
            {
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Text files")
                    {
                        Patterns = new[] { "*.txt" },
                        MimeTypes = new[] { "text/plain" }
                    }
                },
                DefaultExtension = "txt"
            };
            var file = await window.StorageProvider.SaveFilePickerAsync(filePickerOptions).ConfigureAwait(true);
            if (file is null)
                return;
            var couldPathUri = file.TryGetUri(out var pathUri);
            if (!couldPathUri)
                return;
            var path = pathUri.LocalPath;
            await SaveCustomTranslationAsync(path).ConfigureAwait(true);
        }

        private async Task<bool> SaveCustomTranslationAsync(string path)
        {
            if (CustomTranslation is null)
                return false;
            List<string> lines = new();
            lines.Add($"English;{CustomTranslation.LanguageTitle}");
            foreach (var category in CustomTranslation.Categories)
            {
                lines.Add(string.Empty);
                lines.Add($"# {category.Title}");
                foreach (var key in category.TranslateKeys)
                {
                    lines.Add($"{key.Key};{key.Value}");
                }
            }

            await File.WriteAllLinesAsync(path, lines);
            return true;
        }


        #region Constants

        public static string OpenDefault => "Please open the default language file from the game!\n" +
                                            "You can usually find it at:\n" +
                                            "\"<ME2 Install Directory>\\Melody's Escape 2_Data\\StreamingAssets\\Translations\"";

        public static string OpenNewFile => "Please open or create a new translation from the 'File' menu";

        #endregion
    }
}
using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Melody2LanguageEditor.Models;
using Melody2LanguageEditor.ViewModels;

namespace Melody2LanguageEditor.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel 
            => (MainWindowViewModel)this.DataContext!;
        
        private IClassicDesktopStyleApplicationLifetime AppLifetime 
            => (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void MenuItem_Exit_OnClick(object? sender, RoutedEventArgs e)
            => AppLifetime.Shutdown();

        private void MenuItem_Save_OnClick(object? sender, RoutedEventArgs e)
        {
            if (!File.Exists(ViewModel.CurrentSaveLocation))
            {
                Dispatcher.UIThread.InvokeAsync(() => ViewModel.SaveTranslationAsAsync(this));
                return;
            }
            Dispatcher.UIThread.InvokeAsync(() => ViewModel.SaveTranslationAsync(this));
        }

        private void MenuItem_SaveAs_OnClick(object? sender, RoutedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => ViewModel.SaveTranslationAsync(this));
        }

        private void MenuItem_Open_OnClick(object? sender, RoutedEventArgs e)
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => ViewModel.LoadCustomTranslationAsync(this));
        }

        private void MenuItem_OpenDefault_OnClick(object? sender, RoutedEventArgs e)
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => ViewModel.SetAndLoadDefaultTranslationAsync(this));
        }
    }
}
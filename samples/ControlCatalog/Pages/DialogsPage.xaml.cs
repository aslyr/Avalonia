using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Platform.Dialogs;
using Avalonia.Controls.Presenters;
using Avalonia.Dialogs;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
#pragma warning disable 4014

namespace ControlCatalog.Pages
{
    public class DialogsPage : UserControl
    {
        public DialogsPage()
        {
            this.InitializeComponent();

            var results = this.FindControl<ItemsPresenter>("PickerLastResults");
            var resultsVisible = this.FindControl<TextBlock>("PickerLastResultsVisible");

            string lastSelectedDirectory = null;

            List<FileDialogFilter> GetFilters()
            {
                if (this.FindControl<CheckBox>("UseFilters").IsChecked != true)
                    return null;
                return  new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "Text files (.txt)", Extensions = new List<string> {"txt"}
                    },
                    new FileDialogFilter
                    {
                        Name = "All files",
                        Extensions = new List<string> {"*"}
                    }
                };
            }

            List<FilePickerFileType> GetFileTypes()
            {
                if (this.FindControl<CheckBox>("UseFilters").IsChecked != true)
                    return null;
                return new List<FilePickerFileType>
                {
                    FilePickerFileTypes.All,
                    FilePickerFileTypes.TextPlain
                };
            }

            this.FindControl<Button>("OpenFile").Click += async delegate
            {
                var result = await new OpenFileDialog()
                {
                    Title = "Open file",
                    Filters = GetFilters(),
                    Directory = lastSelectedDirectory,
                    // Almost guaranteed to exist
                    InitialFileName = Assembly.GetEntryAssembly()?.GetModules().FirstOrDefault()?.FullyQualifiedName
                }.ShowAsync(GetWindow());
                results.Items = result;
                resultsVisible.IsVisible = result?.Any() == true;
            };
            this.FindControl<Button>("OpenMultipleFiles").Click += async delegate
            {
                var result = await new OpenFileDialog()
                {
                    Title = "Open multiple files",
                    Filters = GetFilters(),
                    Directory = lastSelectedDirectory,
                    AllowMultiple = true
                }.ShowAsync(GetWindow());
                results.Items = result;
                resultsVisible.IsVisible = result?.Any() == true;
            };
            this.FindControl<Button>("SaveFile").Click += async delegate
            {
                var result = await new SaveFileDialog()
                {
                    Title = "Save file",
                    Filters = GetFilters(),
                    Directory = lastSelectedDirectory,
                    InitialFileName = "test.txt"
                }.ShowAsync(GetWindow());
                results.Items = new[] { result };
                resultsVisible.IsVisible = result != null;
            };
            this.FindControl<Button>("SelectFolder").Click += async delegate
            {
                var result = await new OpenFolderDialog()
                {
                    Title = "Select folder",
                    Directory = lastSelectedDirectory,
                }.ShowAsync(GetWindow());
                lastSelectedDirectory = result;
                results.Items = new [] { result };
                resultsVisible.IsVisible = result != null;
            };
            this.FindControl<Button>("OpenBoth").Click += async delegate
            {
                var result = await new OpenFileDialog()
                {
                    Title = "Select both",
                    Directory = lastSelectedDirectory,
                    AllowMultiple = true
                }.ShowManagedAsync(GetWindow(), new ManagedFileDialogOptions
                {
                    AllowDirectorySelection = true
                });
                results.Items = result;
                resultsVisible.IsVisible = result?.Any() == true;
            };
            this.FindControl<Button>("DecoratedWindow").Click += delegate
            {
                new DecoratedWindow().Show();
            };
            this.FindControl<Button>("DecoratedWindowDialog").Click += delegate
            {
                new DecoratedWindow().ShowDialog(GetWindow());
            };
            this.FindControl<Button>("Dialog").Click += delegate
            {
                var window = CreateSampleWindow();
                window.Height = 200;
                window.ShowDialog(GetWindow());
            };
            this.FindControl<Button>("DialogNoTaskbar").Click += delegate
            {
                var window = CreateSampleWindow();
                window.Height = 200;
                window.ShowInTaskbar = false;
                window.ShowDialog(GetWindow());
            };
            this.FindControl<Button>("OwnedWindow").Click += delegate
            {
                var window = CreateSampleWindow();

                window.Show(GetWindow());
            };

            this.FindControl<Button>("OwnedWindowNoTaskbar").Click += delegate
            {
                var window = CreateSampleWindow();

                window.ShowInTaskbar = false;

                window.Show(GetWindow());
            };

            this.FindControl<Button>("OpenFilePicker").Click += async delegate
            {
                var result = await GetTopLevel().FilePicker.OpenAsync(new FilePickerOpenOptions()
                {
                    Title = "Open file",
                    FileTypes = GetFileTypes()
                });
                results.Items = result.Select(f => f.TryGetFullPath(out var fullPath) ? fullPath : f.FileName).ToArray();
                resultsVisible.IsVisible = result?.Any() == true;

                if (result.FirstOrDefault() is { } file)
                {
                    using var stream = file.Stream;
                    using var reader = new System.IO.StreamReader(stream);
                    this.FindControl<TextBox>("OpenedFileContent").Text = reader.ReadToEnd();
                }
            };
            this.FindControl<Button>("OpenMultipleFilesPicker").Click += async delegate
            {
                var result = await GetTopLevel().FilePicker.OpenAsync(new FilePickerOpenOptions()
                {
                    Title = "Open multiple file",
                    FileTypes = GetFileTypes(),
                    AllowMultiple = true
                });
                results.Items = result.Select(f => f.TryGetFullPath(out var fullPath) ? fullPath : f.FileName).ToArray();
                resultsVisible.IsVisible = result?.Any() == true;

                if (result.FirstOrDefault() is { } file)
                {
                    using var stream = file.Stream;
                    using var reader = new System.IO.StreamReader(stream);
                    this.FindControl<TextBox>("OpenedFileContent").Text = reader.ReadToEnd();
                }
            };
            this.FindControl<Button>("SaveFilePicker").Click += async delegate
            {
                var file = await GetTopLevel().FilePicker.SaveAsync(new FilePickerSaveOptions()
                {
                    Title = "Save file",
                    FileTypes = GetFileTypes()
                });
                results.Items = new[] { file }.Select(f => f.TryGetFullPath(out var fullPath) ? fullPath : f.FileName).ToArray();
                resultsVisible.IsVisible = file is not null;

                if (file is not null)
                {
                    using var stream = file.Stream;
                    using var reader = new System.IO.StreamWriter(stream);
                    reader.WriteLine(this.FindControl<TextBox>("OpenedFileContent").Text);
                }
            };

        }

        private Window CreateSampleWindow()
        {
            Button button;
            
            var window = new Window
            {
                Height = 200,
                Width = 200,
                Content = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = "Hello world!" },
                        (button = new Button
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Content = "Click to close"
                        })
                    }
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            button.Click += (_, __) => window.Close();

            return window;
        }

        Window GetWindow() => (Window)this.VisualRoot;
        TopLevel GetTopLevel() => (TopLevel)this.VisualRoot;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

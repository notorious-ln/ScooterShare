using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace WpfMaterialControls
{
    public partial class PromptDialog : Window
    {
        private readonly PromptDialogViewModel viewModel;
        private readonly Func<string, string> validate;
        private readonly bool lettersOnly;

        public PromptDialog(string title, string label, string hint, string defaultValue, Func<string, string> validate, bool lettersOnly)
        {
            InitializeComponent();

            this.validate = validate;
            this.lettersOnly = lettersOnly;
            Title = title ?? "Ввод";
            LabelText.Text = label ?? "Введите значение";
            HintText.Text = hint ?? string.Empty;

            viewModel = new PromptDialogViewModel(defaultValue ?? string.Empty);
            DataContext = viewModel;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            if (lettersOnly)
            {
                InputBox.PreviewTextInput += InputBox_PreviewTextInput;
                DataObject.AddPastingHandler(InputBox, InputBox_Pasting);
            }

            Loaded += (_, __) =>
            {
                InputBox.Focus();
                InputBox.SelectAll();
                ValidateAndUpdateUi();
            };
        }

        public string ResultText => viewModel.InputText;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndUpdateUi())
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        private bool ValidateAndUpdateUi()
        {
            string input = viewModel.InputText ?? string.Empty;
            string error = null;

            if (lettersOnly && Regex.IsMatch(input, @"\d"))
            {
                error = "Здесь нужно писать буквы (цифры нельзя).";
            }
            else
            {
                error = validate == null ? null : validate(input);
            }

            viewModel.ErrorText = string.IsNullOrWhiteSpace(error) ? string.Empty : error;
            viewModel.CanAccept = string.IsNullOrWhiteSpace(viewModel.ErrorText);

            // Visual border feedback on the textbox itself.
            InputBox.BorderBrush = viewModel.CanAccept
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD7, 0xDA, 0xE5))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xD1, 0x5B, 0x75));
            InputBox.BorderThickness = viewModel.CanAccept ? new Thickness(1) : new Thickness(2);

            return viewModel.CanAccept;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputText")
            {
                ValidateAndUpdateUi();
            }
        }

        public static string ShowDialog(
            Window owner,
            string title,
            string label,
            string hint,
            string defaultValue,
            Func<string, string> validate,
            bool lettersOnly = false,
            bool trimResult = true)
        {
            var dlg = new PromptDialog(title, label, hint, defaultValue, validate, lettersOnly)
            {
                Owner = owner
            };

            bool? ok = dlg.ShowDialog();
            if (ok != true)
            {
                return null;
            }

            string result = dlg.ResultText ?? string.Empty;
            return trimResult ? result.Trim() : result;
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!lettersOnly)
            {
                return;
            }

            if (Regex.IsMatch(e.Text ?? string.Empty, @"\d"))
            {
                e.Handled = true;
                // Keep the UI feedback consistent with ValidateAndUpdateUi().
                ValidateAndUpdateUi();
            }
        }

        private void InputBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!lettersOnly)
            {
                return;
            }

            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                return;
            }

            string pasted = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
            if (Regex.IsMatch(pasted, @"\d"))
            {
                e.CancelCommand();
                viewModel.InputText = viewModel.InputText ?? string.Empty;
                viewModel.ErrorText = "Нельзя вставлять цифры — здесь только буквы.";
                viewModel.CanAccept = false;
            }
        }

        private sealed class PromptDialogViewModel : ObservableObject
        {
            private string inputText;
            private string errorText;
            private bool canAccept;

            public PromptDialogViewModel(string defaultValue)
            {
                inputText = defaultValue ?? string.Empty;
                errorText = string.Empty;
                canAccept = true;
            }

            public string InputText
            {
                get => inputText;
                set
                {
                    if (SetProperty(ref inputText, value))
                    {
                        // instant feedback while typing
                        // dialog will call ValidateAndUpdateUi via binding in code-behind below
                    }
                }
            }

            public string ErrorText
            {
                get => errorText;
                set
                {
                    if (SetProperty(ref errorText, value))
                    {
                        OnPropertyChanged(nameof(ErrorVisibility));
                    }
                }
            }

            public Visibility ErrorVisibility =>
                string.IsNullOrWhiteSpace(ErrorText) ? Visibility.Collapsed : Visibility.Visible;

            public bool CanAccept
            {
                get => canAccept;
                set => SetProperty(ref canAccept, value);
            }
        }
    }
}


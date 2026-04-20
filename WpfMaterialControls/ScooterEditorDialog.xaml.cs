using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using WpfMaterialControls.ViewModels;

namespace WpfMaterialControls
{
    public partial class ScooterEditorDialog : Window
    {
        private readonly ScooterEditorDialogViewModel viewModel;

        public ScooterEditorDialog(
            string dialogTitle,
            IReadOnlyCollection<ConditionOption> conditions,
            ScooterDialogResult initialData)
        {
            InitializeComponent();

            viewModel = new ScooterEditorDialogViewModel(dialogTitle, conditions, initialData);
            DataContext = viewModel;

            Loaded += (_, __) => txtModel.Focus();
        }

        public ScooterDialogResult Result => viewModel.BuildResult();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.Validate())
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    internal sealed class ScooterEditorDialogViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private string model;
        private int selectedConditionId;
        private string selectedStatus;
        private string batteryPercentText;
        private string currentLocation;
        private DateTime? yearOfRelease;
        private string modelDigitsOnlyHintText;
        private string locationDigitsOnlyHintText;
        private readonly Dictionary<string, List<string>> errorsByPropertyName =
            new Dictionary<string, List<string>>(StringComparer.Ordinal);

        public ScooterEditorDialogViewModel(
            string dialogTitle,
            IReadOnlyCollection<ConditionOption> conditions,
            ScooterDialogResult initialData)
        {
            DialogTitle = dialogTitle;
            Conditions = new ObservableCollection<ConditionOption>(conditions ?? Array.Empty<ConditionOption>());
            AvailableStatuses = new ObservableCollection<string>(new[]
            {
                "Доступен",
                "В использовании",
                "Низкий заряд",
                "На обслуживании"
            });

            Model = initialData?.Model ?? string.Empty;
            SelectedConditionId = initialData?.ConditionId ?? (Conditions.FirstOrDefault()?.ConditionId ?? 0);
            SelectedStatus = !string.IsNullOrWhiteSpace(initialData?.OperationalStatus)
                ? initialData.OperationalStatus
                : AvailableStatuses.First();
            BatteryPercentText = (initialData?.BatteryPercent ?? 100).ToString(CultureInfo.InvariantCulture);
            CurrentLocation = initialData?.CurrentLocation ?? string.Empty;
            YearOfRelease = initialData?.YearOfRelease ?? DateTime.Today;
            ScooterId = initialData?.ScooterId ?? 0;

            ValidateAll();
        }

        public int ScooterId { get; }

        public bool IsYearOfReleaseEditable => ScooterId <= 0;

        public string DialogTitle { get; }

        public ObservableCollection<ConditionOption> Conditions { get; }

        public ObservableCollection<string> AvailableStatuses { get; }

        public string Model
        {
            get => model;
            set
            {
                if (SetProperty(ref model, value))
                {
                    ValidateModel();
                }
            }
        }

        public string ModelDigitsOnlyHintText
        {
            get => modelDigitsOnlyHintText;
            private set
            {
                if (SetProperty(ref modelDigitsOnlyHintText, value))
                {
                    OnPropertyChanged(nameof(ModelDigitsOnlyHintVisibility));
                }
            }
        }

        public Visibility ModelDigitsOnlyHintVisibility =>
            string.IsNullOrWhiteSpace(ModelDigitsOnlyHintText) ? Visibility.Collapsed : Visibility.Visible;

        public string LocationDigitsOnlyHintText
        {
            get => locationDigitsOnlyHintText;
            private set
            {
                if (SetProperty(ref locationDigitsOnlyHintText, value))
                {
                    OnPropertyChanged(nameof(LocationDigitsOnlyHintVisibility));
                }
            }
        }

        public Visibility LocationDigitsOnlyHintVisibility =>
            string.IsNullOrWhiteSpace(LocationDigitsOnlyHintText) ? Visibility.Collapsed : Visibility.Visible;

        public int SelectedConditionId
        {
            get => selectedConditionId;
            set
            {
                if (SetProperty(ref selectedConditionId, value))
                {
                    ValidateSelectedConditionId();
                }
            }
        }

        public string SelectedStatus
        {
            get => selectedStatus;
            set
            {
                if (SetProperty(ref selectedStatus, value))
                {
                    ValidateSelectedStatus();
                }
            }
        }

        public string BatteryPercentText
        {
            get => batteryPercentText;
            set
            {
                if (SetProperty(ref batteryPercentText, value))
                {
                    ValidateBatteryPercentText();
                }
            }
        }

        public string CurrentLocation
        {
            get => currentLocation;
            set
            {
                if (SetProperty(ref currentLocation, value))
                {
                    ValidateCurrentLocation();
                }
            }
        }

        public DateTime? YearOfRelease
        {
            get => yearOfRelease;
            set
            {
                if (SetProperty(ref yearOfRelease, value))
                {
                    ValidateYearOfRelease();
                }
            }
        }

        public bool CanSave => !HasErrors;

        public bool HasErrors => errorsByPropertyName.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return errorsByPropertyName.SelectMany(kvp => kvp.Value).ToList();
            }

            List<string> errors;
            if (errorsByPropertyName.TryGetValue(propertyName, out errors))
            {
                return errors;
            }

            return new string[0];
        }

        public bool Validate()
        {
            ValidateAll();
            return !HasErrors;
        }

        public ScooterDialogResult BuildResult()
        {
            int batteryPercent = 100;
            int.TryParse(BatteryPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out batteryPercent);

            return new ScooterDialogResult
            {
                ScooterId = ScooterId,
                Model = (Model ?? string.Empty).Trim(),
                ConditionId = SelectedConditionId,
                ConditionName = Conditions.FirstOrDefault(x => x.ConditionId == SelectedConditionId)?.ConditionName ?? string.Empty,
                OperationalStatus = (SelectedStatus ?? string.Empty).Trim(),
                BatteryPercent = batteryPercent,
                CurrentLocation = (CurrentLocation ?? string.Empty).Trim(),
                YearOfRelease = YearOfRelease ?? DateTime.Today
            };
        }

        private void ValidateAll()
        {
            ValidateModel();
            ValidateSelectedConditionId();
            ValidateSelectedStatus();
            ValidateBatteryPercentText();
            ValidateCurrentLocation();
            ValidateYearOfRelease();
            OnPropertyChanged(nameof(CanSave));
        }

        private void ValidateModel()
        {
            ClearErrors(nameof(Model));
            ModelDigitsOnlyHintText = string.Empty;
            string v = (Model ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v))
            {
                AddError(nameof(Model), "Укажи модель самоката. Можно буквы и цифры (например: Xiaomi M365).");
                return;
            }

            // Allow letters + digits, but disallow "digits only"
            if (!Regex.IsMatch(v, @"\p{L}"))
            {
                const string msg = "Нельзя указывать модель только цифрами — добавь хотя бы одну букву (например: M365).";
                AddError(nameof(Model), msg);
                ModelDigitsOnlyHintText = msg;
                return;
            }
        }

        private void ValidateSelectedConditionId()
        {
            ClearErrors(nameof(SelectedConditionId));
            if (SelectedConditionId <= 0)
            {
                AddError(nameof(SelectedConditionId), "Выбери техническое состояние самоката.");
            }
        }

        private void ValidateSelectedStatus()
        {
            ClearErrors(nameof(SelectedStatus));
            if (string.IsNullOrWhiteSpace(SelectedStatus))
            {
                AddError(nameof(SelectedStatus), "Выбери оперативный статус.");
            }
        }

        private void ValidateBatteryPercentText()
        {
            ClearErrors(nameof(BatteryPercentText));
            if (string.IsNullOrWhiteSpace(BatteryPercentText))
            {
                AddError(nameof(BatteryPercentText), "Укажи заряд (0–100).");
                return;
            }

            if (!int.TryParse(BatteryPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int batteryPercent))
            {
                AddError(nameof(BatteryPercentText), "Заряд должен быть целым числом от 0 до 100.");
                return;
            }

            if (batteryPercent < 0 || batteryPercent > 100)
            {
                AddError(nameof(BatteryPercentText), "Заряд должен быть в диапазоне от 0 до 100.");
            }
        }

        private void ValidateCurrentLocation()
        {
            ClearErrors(nameof(CurrentLocation));
            LocationDigitsOnlyHintText = string.Empty;
            string v = (CurrentLocation ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v))
            {
                AddError(nameof(CurrentLocation), "Укажи текущее местоположение самоката.");
                return;
            }

            // Disallow values that are "digits only" (same UX as Model).
            if (!Regex.IsMatch(v, @"\p{L}"))
            {
                const string msg = "Местоположение не может состоять только из цифр — добавь хотя бы одну букву (например: Центр, Park 12).";
                AddError(nameof(CurrentLocation), msg);
                LocationDigitsOnlyHintText = msg;
            }
        }

        private void ValidateYearOfRelease()
        {
            ClearErrors(nameof(YearOfRelease));
            if (IsYearOfReleaseEditable && !YearOfRelease.HasValue)
            {
                AddError(nameof(YearOfRelease), "Укажи год выпуска.");
                return;
            }

            if (YearOfRelease.HasValue && YearOfRelease.Value.Date > DateTime.Today)
            {
                AddError(nameof(YearOfRelease), "Год выпуска не может быть в будущем.");
            }
        }

        private void AddError(string propertyName, string error)
        {
            if (!errorsByPropertyName.TryGetValue(propertyName, out var errors))
            {
                errors = new List<string>();
                errorsByPropertyName[propertyName] = errors;
            }

            if (!errors.Contains(error, StringComparer.Ordinal))
            {
                errors.Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (errorsByPropertyName.Remove(propertyName))
            {
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public sealed class ConditionOption
    {
        public int ConditionId { get; set; }

        public string ConditionName { get; set; }
    }
}

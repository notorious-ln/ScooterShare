using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
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

    internal sealed class ScooterEditorDialogViewModel : ObservableObject
    {
        private string model;
        private int selectedConditionId;
        private string selectedStatus;
        private string batteryPercentText;
        private string currentLocation;
        private DateTime? yearOfRelease;
        private string validationMessage;

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
        }

        public int ScooterId { get; }

        public bool IsYearOfReleaseEditable => ScooterId <= 0;

        public string DialogTitle { get; }

        public ObservableCollection<ConditionOption> Conditions { get; }

        public ObservableCollection<string> AvailableStatuses { get; }

        public string Model
        {
            get => model;
            set => SetProperty(ref model, value);
        }

        public int SelectedConditionId
        {
            get => selectedConditionId;
            set => SetProperty(ref selectedConditionId, value);
        }

        public string SelectedStatus
        {
            get => selectedStatus;
            set => SetProperty(ref selectedStatus, value);
        }

        public string BatteryPercentText
        {
            get => batteryPercentText;
            set => SetProperty(ref batteryPercentText, value);
        }

        public string CurrentLocation
        {
            get => currentLocation;
            set => SetProperty(ref currentLocation, value);
        }

        public DateTime? YearOfRelease
        {
            get => yearOfRelease;
            set => SetProperty(ref yearOfRelease, value);
        }

        public string ValidationMessage
        {
            get => validationMessage;
            set
            {
                if (SetProperty(ref validationMessage, value))
                {
                    OnPropertyChanged(nameof(ValidationMessageVisibility));
                }
            }
        }

        public Visibility ValidationMessageVisibility =>
            string.IsNullOrWhiteSpace(ValidationMessage) ? Visibility.Collapsed : Visibility.Visible;

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Model))
            {
                ValidationMessage = "Укажи модель самоката.";
                return false;
            }

            if (SelectedConditionId <= 0)
            {
                ValidationMessage = "Выбери техническое состояние самоката.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedStatus))
            {
                ValidationMessage = "Выбери оперативный статус.";
                return false;
            }

            if (!int.TryParse(BatteryPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int batteryPercent))
            {
                ValidationMessage = "Заряд должен быть целым числом от 0 до 100.";
                return false;
            }

            if (batteryPercent < 0 || batteryPercent > 100)
            {
                ValidationMessage = "Заряд должен быть в диапазоне от 0 до 100.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentLocation))
            {
                ValidationMessage = "Укажи текущее местоположение самоката.";
                return false;
            }

            if (IsYearOfReleaseEditable && !YearOfRelease.HasValue)
            {
                ValidationMessage = "Укажи год выпуска.";
                return false;
            }

            ValidationMessage = string.Empty;
            return true;
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
    }

    public sealed class ConditionOption
    {
        public int ConditionId { get; set; }

        public string ConditionName { get; set; }
    }
}

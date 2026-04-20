using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace WpfMaterialControls.ViewModels
{
    public class ScootersViewModel : ObservableObject
    {
        private int totalCount;
        private int availableCount;
        private int attentionCount;
        private string warningText = "Нет самокатов для отображения.";

        public ScootersViewModel()
        {
            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand<object>(OnEdit);
            DeleteCommand = new RelayCommand<object>(OnDelete);
            Load();
        }

        public ObservableCollection<ScooterItemViewModel> Items { get; } = new ObservableCollection<ScooterItemViewModel>();

        public IRelayCommand AddCommand { get; }

        public IRelayCommand<object> EditCommand { get; }

        public IRelayCommand<object> DeleteCommand { get; }

        public int TotalCount
        {
            get => totalCount;
            set => SetProperty(ref totalCount, value);
        }

        public int AvailableCount
        {
            get => availableCount;
            set => SetProperty(ref availableCount, value);
        }

        public int AttentionCount
        {
            get => attentionCount;
            set => SetProperty(ref attentionCount, value);
        }

        public string WarningText
        {
            get => warningText;
            set => SetProperty(ref warningText, value);
        }

        private void Load()
        {
            Items.Clear();

            try
            {
                DatabaseHelper.EnsureScooterSchema();
                DatabaseHelper.EnsureActivityTableExists();

                LoadStats();
                LoadItems();
                UpdateWarningText();
            }
            catch (Exception ex)
            {
                TotalCount = 0;
                AvailableCount = 0;
                AttentionCount = 0;
                WarningText = "Не удалось загрузить данные о самокатах.";
                MessageBox.Show(
                    $"Не удалось загрузить список самокатов.\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadStats()
        {
            DataTable statsTable = DatabaseHelper.ExecuteQuery(@"
SELECT
    COUNT(*) AS total_count,
    SUM(CASE WHEN ISNULL(s.operational_status, N'') = N'Доступен' THEN 1 ELSE 0 END) AS available_count,
    SUM(
        CASE
            WHEN ISNULL(s.battery_percent, 0) <= 30
                 OR ISNULL(s.operational_status, N'') = N'На обслуживании'
                 OR LOWER(ISNULL(c.condition_name, N'')) LIKE N'%неисправ%'
                 OR LOWER(ISNULL(c.condition_name, N'')) LIKE N'%плох%'
                 OR LOWER(ISNULL(c.condition_name, N'')) LIKE N'%крит%'
            THEN 1
            ELSE 0
        END
    ) AS attention_count
FROM Scooters s
LEFT JOIN Conditions c ON c.condition_id = s.condition_id;");

            if (statsTable.Rows.Count == 0)
            {
                TotalCount = 0;
                AvailableCount = 0;
                AttentionCount = 0;
                return;
            }

            DataRow row = statsTable.Rows[0];
            TotalCount = ConvertToInt(row["total_count"]);
            AvailableCount = ConvertToInt(row["available_count"]);
            AttentionCount = ConvertToInt(row["attention_count"]);
        }

        private void LoadItems()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(@"
SELECT
    s.scooter_id,
    ISNULL(s.model, N'') AS model,
    s.condition_id,
    ISNULL(c.condition_name, N'Не указано') AS condition_name,
    ISNULL(s.operational_status, N'Доступен') AS operational_status,
    ISNULL(s.battery_percent, 100) AS battery_percent,
    ISNULL(s.current_location, N'Не указано') AS current_location,
    s.yearOfRelease,
    MAX(ar.start_time) AS last_ride
FROM Scooters s
LEFT JOIN Conditions c ON c.condition_id = s.condition_id
LEFT JOIN Rental_scooters rs ON rs.scooter_id = s.scooter_id
LEFT JOIN Active_rentals ar ON ar.activeRental_id = rs.activeRental_id
GROUP BY
    s.scooter_id,
    s.model,
    s.condition_id,
    c.condition_name,
    s.operational_status,
    s.battery_percent,
    s.current_location,
    s.yearOfRelease
ORDER BY s.scooter_id DESC;");

            foreach (DataRow row in dt.Rows)
            {
                DateTime? yearOfRelease = row["yearOfRelease"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(row["yearOfRelease"], CultureInfo.InvariantCulture);

                string lastRide = row["last_ride"] == DBNull.Value
                    ? "Нет поездок"
                    : GetRelativeTime(Convert.ToDateTime(row["last_ride"], CultureInfo.InvariantCulture));

                Items.Add(new ScooterItemViewModel
                {
                    Id = ConvertToInt(row["scooter_id"]),
                    Model = Convert.ToString(row["model"], CultureInfo.InvariantCulture) ?? string.Empty,
                    ConditionId = ConvertToInt(row["condition_id"]),
                    ConditionName = Convert.ToString(row["condition_name"], CultureInfo.InvariantCulture) ?? string.Empty,
                    OperationalStatus = Convert.ToString(row["operational_status"], CultureInfo.InvariantCulture) ?? "Доступен",
                    BatteryPercent = ConvertToInt(row["battery_percent"]),
                    CurrentLocation = Convert.ToString(row["current_location"], CultureInfo.InvariantCulture) ?? "Не указано",
                    LastRideRaw = lastRide,
                    YearOfRelease = yearOfRelease
                });
            }
        }

        private void UpdateWarningText()
        {
            int lowChargeCount = Items.Count(item => item.BatteryPercent <= 30);
            int maintenanceCount = Items.Count(item =>
                string.Equals(item.OperationalStatus, "На обслуживании", StringComparison.OrdinalIgnoreCase) ||
                IsPoorCondition(item.ConditionName));

            if (Items.Count == 0)
            {
                WarningText = "В базе пока нет самокатов.";
                return;
            }

            if (lowChargeCount == 0 && maintenanceCount == 0)
            {
                WarningText = "Все самокаты в рабочем состоянии и готовы к аренде.";
                return;
            }

            WarningText = $"{lowChargeCount} самокат(ов) требуют зарядки, {maintenanceCount} самокат(ов) требуют технического внимания.";
        }

        private void OnAdd()
        {
            try
            {
                IReadOnlyCollection<ConditionOption> conditions = LoadConditions();
                if (conditions.Count == 0)
                {
                    MessageBox.Show(
                        "Сначала добавь записи в таблицу Conditions, чтобы можно было выбрать техническое состояние.",
                        "Нет состояний",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var dialog = new ScooterEditorDialog(
                    "Добавить самокат",
                    conditions,
                    new ScooterDialogResult
                    {
                        OperationalStatus = "Доступен",
                        BatteryPercent = 100,
                        CurrentLocation = "Не указано",
                        YearOfRelease = DateTime.Today,
                        ConditionId = conditions.First().ConditionId
                    });

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                ScooterDialogResult result = dialog.Result;
                DatabaseHelper.ReseedIdentityToMax("Scooters", "scooter_id");
                object idObj = DatabaseHelper.ExecuteScalar(
                    @"INSERT INTO Scooters (model, condition_id, yearOfRelease, operational_status, battery_percent, current_location)
                      VALUES (@model, @conditionId, @yearOfRelease, @operationalStatus, @batteryPercent, @currentLocation);
                      SELECT SCOPE_IDENTITY();",
                    new[]
                    {
                        new SqlParameter("@model", result.Model),
                        new SqlParameter("@conditionId", result.ConditionId),
                        new SqlParameter("@yearOfRelease", result.YearOfRelease),
                        new SqlParameter("@operationalStatus", result.OperationalStatus),
                        new SqlParameter("@batteryPercent", result.BatteryPercent),
                        new SqlParameter("@currentLocation", result.CurrentLocation)
                    });

                int newId = ConvertIdentity(idObj);
                DatabaseHelper.LogActivity($"Добавлен самокат #{newId}: {result.Model}");
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось добавить самокат.\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnEdit(object idParam)
        {
            if (!TryGetId(idParam, out int id))
            {
                return;
            }

            try
            {
                IReadOnlyCollection<ConditionOption> conditions = LoadConditions();
                if (conditions.Count == 0)
                {
                    MessageBox.Show(
                        "Сначала добавь записи в таблицу Conditions, чтобы можно было выбрать техническое состояние.",
                        "Нет состояний",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                DataTable row = DatabaseHelper.ExecuteQuery(
                    @"SELECT TOP 1
                        scooter_id,
                        model,
                        condition_id,
                        ISNULL(operational_status, N'Доступен') AS operational_status,
                        ISNULL(battery_percent, 100) AS battery_percent,
                        ISNULL(current_location, N'Не указано') AS current_location,
                        yearOfRelease
                      FROM Scooters
                      WHERE scooter_id = @id",
                    new[] { new SqlParameter("@id", id) });

                if (row.Rows.Count == 0)
                {
                    MessageBox.Show("Самокат не найден.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                DataRow data = row.Rows[0];
                var dialog = new ScooterEditorDialog(
                    $"Изменить самокат #{id}",
                    conditions,
                    new ScooterDialogResult
                    {
                        ScooterId = id,
                        Model = Convert.ToString(data["model"], CultureInfo.InvariantCulture) ?? string.Empty,
                        ConditionId = ConvertToInt(data["condition_id"]),
                        OperationalStatus = Convert.ToString(data["operational_status"], CultureInfo.InvariantCulture) ?? "Доступен",
                        BatteryPercent = ConvertToInt(data["battery_percent"]),
                        CurrentLocation = Convert.ToString(data["current_location"], CultureInfo.InvariantCulture) ?? "Не указано",
                        YearOfRelease = data["yearOfRelease"] == DBNull.Value
                            ? DateTime.Today
                            : Convert.ToDateTime(data["yearOfRelease"], CultureInfo.InvariantCulture)
                    });

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                ScooterDialogResult result = dialog.Result;
                DatabaseHelper.ExecuteNonQuery(
                    @"UPDATE Scooters
                      SET model = @model,
                          condition_id = @conditionId,
                          operational_status = @operationalStatus,
                          battery_percent = @batteryPercent,
                          current_location = @currentLocation
                      WHERE scooter_id = @id",
                    new[]
                    {
                        new SqlParameter("@model", result.Model),
                        new SqlParameter("@conditionId", result.ConditionId),
                        new SqlParameter("@operationalStatus", result.OperationalStatus),
                        new SqlParameter("@batteryPercent", result.BatteryPercent),
                        new SqlParameter("@currentLocation", result.CurrentLocation),
                        new SqlParameter("@id", id)
                    });

                DatabaseHelper.LogActivity($"Изменён самокат #{id}: {result.Model}");
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить изменения.\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnDelete(object idParam)
        {
            if (!TryGetId(idParam, out int id))
            {
                return;
            }

            if (MessageBox.Show(
                    $"Удалить самокат #{id}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Scooters WHERE scooter_id = @id",
                    new[] { new SqlParameter("@id", id) });

                DatabaseHelper.ReseedIdentityToMax("Scooters", "scooter_id");
                DatabaseHelper.LogActivity($"Удалён самокат #{id}");
                Load();
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Нельзя удалить самокат, потому что он связан с арендой, жалобами или другими записями.",
                    "Удаление невозможно",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить самокат.\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static IReadOnlyCollection<ConditionOption> LoadConditions()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT condition_id, condition_name FROM Conditions ORDER BY condition_name");

            return dt.Rows
                .Cast<DataRow>()
                .Select(row => new ConditionOption
                {
                    ConditionId = ConvertToInt(row["condition_id"]),
                    ConditionName = Convert.ToString(row["condition_name"], CultureInfo.InvariantCulture) ?? string.Empty
                })
                .ToList();
        }

        private static bool IsPoorCondition(string conditionName)
        {
            string normalized = (conditionName ?? string.Empty).ToLowerInvariant();
            return normalized.Contains("неисправ")
                   || normalized.Contains("плох")
                   || normalized.Contains("крит")
                   || normalized.Contains("обслуж");
        }

        private static string GetRelativeTime(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return "только что";
            if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} мин назад";
            if (span.TotalDays < 1) return $"{(int)span.TotalHours} ч назад";
            if (span.TotalDays < 2) return "1 день назад";
            return $"{(int)span.TotalDays} дней назад";
        }

        private static int ConvertIdentity(object identity)
        {
            if (identity == null || identity == DBNull.Value)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(Convert.ToDecimal(identity, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        private static int ConvertToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static bool TryGetId(object value, out int id)
        {
            id = 0;
            if (value == null)
            {
                return false;
            }

            if (value is int typedId)
            {
                id = typedId;
                return true;
            }

            return int.TryParse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out id);
        }
    }
}
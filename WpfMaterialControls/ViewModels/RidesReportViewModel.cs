using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace WpfMaterialControls.ViewModels
{
    public sealed class RidesReportViewModel : ObservableObject
    {
        private string selectedStatus = "Все статусы";
        private string selectedPeriod = "Все время";

        public RidesReportViewModel()
        {
            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = FilterRide;
            ExportExcelCommand = new RelayCommand(ExportToExcel);
            LoadRideItems();
        }

        public ObservableCollection<RideItemViewModel> Items { get; } = new ObservableCollection<RideItemViewModel>();

        public ICollectionView FilteredItems { get; }

        public ObservableCollection<string> StatusOptions { get; } = new ObservableCollection<string>
        {
            "Все статусы",
            "Завершена",
            "В процессе"
        };

        public ObservableCollection<string> PeriodOptions { get; } = new ObservableCollection<string>
        {
            "Сегодня",
            "Неделя",
            "Все время"
        };

        public string SelectedStatus
        {
            get => selectedStatus;
            set
            {
                if (SetProperty(ref selectedStatus, value))
                {
                    FilteredItems.Refresh();
                }
            }
        }

        public string SelectedPeriod
        {
            get => selectedPeriod;
            set
            {
                if (SetProperty(ref selectedPeriod, value))
                {
                    FilteredItems.Refresh();
                }
            }
        }

        public IRelayCommand ExportExcelCommand { get; }

        private void LoadRideItems()
        {
            Items.Clear();

            DataTable dt = DatabaseHelper.ExecuteQuery(@"
SELECT
    ar.activeRental_id,
    ISNULL(c.firstName, N'') AS firstName,
    ISNULL(c.lastName, N'') AS lastName,
    s.scooter_id,
    ar.start_time,
    ar.plannedfFinishTime,
    ar.start_odometer_km,
    ar.end_odometer_km,
    ISNULL(r.costPerMinute, 0) AS costPerMinute,
    ISNULL(st.status_name, N'Завершена') AS rental_status
FROM Active_rentals ar
LEFT JOIN Clients_Rentals cr ON cr.activeRental_id = ar.activeRental_id
LEFT JOIN Clients c ON c.client_id = cr.client_id
LEFT JOIN Rental_scooters rs ON rs.activeRental_id = ar.activeRental_id
LEFT JOIN Scooters s ON s.scooter_id = rs.scooter_id
LEFT JOIN Rates r ON r.rate_id = ar.rate_id
LEFT JOIN StatusesOfRental st ON st.status_id = ar.status_id
ORDER BY ar.start_time DESC;");

            foreach (DataRow row in dt.Rows)
            {
                DateTime startTime = row["start_time"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(row["start_time"], CultureInfo.InvariantCulture);
                DateTime endTime = row["plannedfFinishTime"] == DBNull.Value
                    ? DateTime.Now
                    : Convert.ToDateTime(row["plannedfFinishTime"], CultureInfo.InvariantCulture);

                int durationMinutes = startTime == DateTime.MinValue
                    ? 0
                    : Math.Max(0, (int)Math.Round((endTime - startTime).TotalMinutes));

                int startOdometer = ToInt(row["start_odometer_km"]);
                int endOdometer = ToInt(row["end_odometer_km"]);
                double distance = Math.Max(0, endOdometer - startOdometer);

                decimal costPerMinute = ToDecimal(row["costPerMinute"]);
                decimal cost = durationMinutes * costPerMinute;

                string status = NormalizeStatus(ToString(row["rental_status"]));
                string userName = BuildUserName(ToString(row["firstName"]), ToString(row["lastName"]));
                string scooterText = $"#{ToInt(row["scooter_id"])}";
                string route = BuildRouteText(ToInt(row["scooter_id"]), distance);

                Items.Add(new RideItemViewModel
                {
                    Id = ToInt(row["activeRental_id"]),
                    UserName = userName,
                    ScooterText = scooterText,
                    StartTime = startTime,
                    DurationMinutes = durationMinutes,
                    DistanceKm = distance,
                    RouteText = route,
                    Cost = cost,
                    StatusText = status
                });
            }

            FilteredItems.Refresh();
        }

        private bool FilterRide(object obj)
        {
            var item = obj as RideItemViewModel;
            if (item == null)
            {
                return false;
            }

            bool statusMatch = SelectedStatus == "Все статусы" ||
                               string.Equals(item.StatusText, SelectedStatus, StringComparison.OrdinalIgnoreCase);
            if (!statusMatch)
            {
                return false;
            }

            DateTime today = DateTime.Today;
            if (SelectedPeriod == "Сегодня")
            {
                return item.StartTime.Date == today;
            }

            if (SelectedPeriod == "Неделя")
            {
                // "Неделя" = последние 7 дней включая сегодня; исключаем будущие даты.
                DateTime from = today.AddDays(-6);
                DateTime toExclusive = today.AddDays(1);
                return item.StartTime >= from && item.StartTime < toExclusive;
            }

            return true;
        }

        private void ExportToExcel()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Экспорт отчёта в Excel",
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"rides-report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx",
                    AddExtension = true,
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var rows = FilteredItems.Cast<RideItemViewModel>().ToList();

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Поездки");

                    string[] headers =
                    {
                        "ID","Пользователь","Самокат","Дата","Время","Длительность","Расстояние","Маршрут","Стоимость","Статус"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(1, i + 1).Value = headers[i];
                    }

                    var headerRange = ws.Range(1, 1, 1, headers.Length);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F7");
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    for (int r = 0; r < rows.Count; r++)
                    {
                        var rowItem = rows[r];
                        int rowIndex = r + 2;

                        ws.Cell(rowIndex, 1).Value = rowItem.RideIdText;
                        ws.Cell(rowIndex, 2).Value = rowItem.UserName;
                        ws.Cell(rowIndex, 3).Value = rowItem.ScooterText;
                        ws.Cell(rowIndex, 4).Value = rowItem.StartTime == DateTime.MinValue ? "" : rowItem.StartTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                        ws.Cell(rowIndex, 5).Value = rowItem.StartTime == DateTime.MinValue ? "" : rowItem.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture);
                        ws.Cell(rowIndex, 6).Value = rowItem.DurationText;
                        ws.Cell(rowIndex, 7).Value = rowItem.DistanceText;
                        ws.Cell(rowIndex, 8).Value = rowItem.RouteText;
                        ws.Cell(rowIndex, 9).Value = rowItem.CostText;
                        ws.Cell(rowIndex, 10).Value = rowItem.StatusText;
                    }

                    ws.SheetView.FreezeRows(1);
                    ws.Columns().AdjustToContents();

                    wb.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Файл успешно сохранён.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось выполнить экспорт.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string BuildUserName(string firstName, string lastName)
        {
            string fullName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? "Неизвестный пользователь" : fullName;
        }

        private static string BuildRouteText(int scooterId, double distanceKm)
        {
            string[] points =
            {
                "Главная улица → Центральный парк",
                "Торговый центр → Университетский кампус",
                "Кофейня → Офисное здание",
                "Ж/Д вокзал → Торговый квартал",
                "Центр города → Жилой район"
            };

            if (distanceKm <= 0)
            {
                return "Маршрут не указан";
            }

            int index = scooterId <= 0 ? 0 : scooterId % points.Length;
            return points[index];
        }

        private static string NormalizeStatus(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            if (s.Contains("проц") || s.Contains("active"))
            {
                return "В процессе";
            }

            return "Завершена";
        }

        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static string ToString(object value)
        {
            return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}


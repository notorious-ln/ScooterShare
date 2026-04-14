using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfMaterialControls.ViewModels
{
    public class RidesViewModel : ObservableObject
    {
        private string ridesToday = "0";
        private string ridesThisWeek = "0";
        private string revenueToday = "₽0";
        private string averageDuration = "0 мин";
        private string selectedStatus = "Все статусы";
        private string selectedPeriod = "Все время";

        public RidesViewModel()
        {
            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = FilterRide;
            Load();
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

        public string RidesToday
        {
            get => ridesToday;
            set => SetProperty(ref ridesToday, value);
        }

        public string RidesThisWeek
        {
            get => ridesThisWeek;
            set => SetProperty(ref ridesThisWeek, value);
        }

        public string RevenueToday
        {
            get => revenueToday;
            set => SetProperty(ref revenueToday, value);
        }

        public string AverageDuration
        {
            get => averageDuration;
            set => SetProperty(ref averageDuration, value);
        }

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

        private void Load()
        {
            try
            {
                Items.Clear();

                object ridesTodayObj = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Active_rentals WHERE CAST(start_time AS DATE) = CAST(GETDATE() AS DATE)");
                int ridesTodayCount = ToInt(ridesTodayObj);
                RidesToday = ridesTodayCount.ToString(CultureInfo.InvariantCulture);

                object ridesWeekObj = DatabaseHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Active_rentals WHERE start_time >= DATEADD(day, -7, GETDATE())");
                RidesThisWeek = ToInt(ridesWeekObj).ToString(CultureInfo.InvariantCulture);

                object revenueTodayObj = DatabaseHelper.ExecuteScalar(@"
SELECT ISNULL(SUM(
    CASE
        WHEN ar.start_time IS NULL THEN 0
        ELSE DATEDIFF(MINUTE, ar.start_time, ISNULL(ar.plannedfFinishTime, GETDATE())) * ISNULL(r.costPerMinute, 0)
    END
), 0)
FROM Active_rentals ar
LEFT JOIN Rates r ON r.rate_id = ar.rate_id
WHERE CAST(ar.start_time AS DATE) = CAST(GETDATE() AS DATE);");
                decimal revenue = ToDecimal(revenueTodayObj);
                RevenueToday = $"₽{Math.Round(revenue, 0):N0}".Replace(",", " ");

                object avgDurationObj = DatabaseHelper.ExecuteScalar(@"
SELECT ISNULL(AVG(CAST(DATEDIFF(MINUTE, start_time, ISNULL(plannedfFinishTime, GETDATE())) AS FLOAT)), 0)
FROM Active_rentals
WHERE start_time IS NOT NULL;");
                int avgMinutes = (int)Math.Round(ToDouble(avgDurationObj), MidpointRounding.AwayFromZero);
                AverageDuration = $"{avgMinutes} мин";

                LoadRideItems();
                FilteredItems.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить поездки.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRideItems()
        {
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
        }

        private bool FilterRide(object obj)
        {
            if (!(obj is RideItemViewModel item))
            {
                return false;
            }

            bool statusMatch = SelectedStatus == "Все статусы" || string.Equals(item.StatusText, SelectedStatus, StringComparison.OrdinalIgnoreCase);
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
                return item.StartTime >= today.AddDays(-7);
            }

            return true;
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

        private static double ToDouble(object value)
        {
            if (value == null || value == DBNull.Value) return 0d;
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        private static string ToString(object value)
        {
            return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}

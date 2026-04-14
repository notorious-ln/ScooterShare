using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WpfMaterialControls.ViewModels
{
    public class RideItemViewModel : ObservableObject
    {
        public int Id { get; set; }

        public string RideIdText => $"#{Id}";

        public string UserName { get; set; }

        public string ScooterText { get; set; }

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; }

        public double DistanceKm { get; set; }

        public string RouteText { get; set; }

        public decimal Cost { get; set; }

        public string StatusText { get; set; }

        public string UserPrimaryLine
        {
            get
            {
                string[] parts = (UserName ?? string.Empty)
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return parts.Length > 0 ? parts[0] : "Неизвестный";
            }
        }

        public string UserSecondaryLine
        {
            get
            {
                string[] parts = (UserName ?? string.Empty)
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length <= 1)
                {
                    return string.Empty;
                }

                return string.Join(" ", parts, 1, parts.Length - 1);
            }
        }

        public string TimeText => StartTime == DateTime.MinValue ? "—" : $"{StartTime:dd.MM.yyyy}\n{StartTime:HH:mm}";

        public string DurationText => $"{DurationMinutes} мин";

        public string DistanceText => $"{DistanceKm:0.#} км";

        public string CostText => $"{Math.Round(Cost, 0):N0}".Replace(",", " ");

        public string StatusBackground => GetStatusBackground(StatusText);

        public string StatusForeground => GetStatusForeground(StatusText);

        private static string GetStatusBackground(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            if (s.Contains("проц")) return "#E7F0FF";
            return "#E8FAEE";
        }

        private static string GetStatusForeground(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            if (s.Contains("проц")) return "#4F74D2";
            return "#2EA35F";
        }
    }
}

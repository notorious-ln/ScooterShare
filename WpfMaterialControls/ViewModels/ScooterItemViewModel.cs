using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfMaterialControls.ViewModels
{
    public class ScooterItemViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public int ConditionId { get; set; }
        public string ConditionName { get; set; }
        public string OperationalStatus { get; set; }
        public int BatteryPercent { get; set; }
        public string CurrentLocation { get; set; }
        public string LastRideRaw { get; set; }
        public System.DateTime? YearOfRelease { get; set; }

        public string DisplayId => $"↔  #{Id}";
        public string StatusText => string.IsNullOrWhiteSpace(OperationalStatus) ? "Неизвестно" : OperationalStatus;
        public string StatusBackground => GetStatusBackground(StatusText);
        public string StatusForeground => GetStatusForeground(StatusText);
        public string ChargeText => $"{BatteryPercent}%";
        public string ChargeIcon => "▭";
        public string ChargeColor => GetChargeColor(BatteryPercent);
        public string Location => $"◉ {CurrentLocation}";
        public string LastRide => string.IsNullOrWhiteSpace(LastRideRaw) ? "Нет поездок" : LastRideRaw;

        public bool NeedsCharging => BatteryPercent <= 30;
        public bool NeedsAttention => NeedsCharging || IsMaintenanceStatus(StatusText) || IsPoorCondition(ConditionName);

        private static string GetStatusBackground(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            if (s.Contains("низк")) return "#FFF5CC";
            if (s.Contains("обслуж")) return "#FDECEE";
            if (s.Contains("использ")) return "#E9F0FF";
            return "#EAF9EE";
        }

        private static string GetStatusForeground(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            if (s.Contains("низк")) return "#C39917";
            if (s.Contains("обслуж")) return "#D15B75";
            if (s.Contains("использ")) return "#4F74D2";
            return "#2EA35F";
        }

        private static string GetChargeColor(int charge)
        {
            if (charge >= 75) return "#2EA35F";
            if (charge >= 40) return "#C39917";
            return "#D15B75";
        }

        private static bool IsMaintenanceStatus(string status)
        {
            string s = (status ?? string.Empty).ToLowerInvariant();
            return s.Contains("обслуж");
        }

        private static bool IsPoorCondition(string conditionName)
        {
            string s = (conditionName ?? string.Empty).ToLowerInvariant();
            return s.Contains("неисправ") || s.Contains("плох") || s.Contains("крит") || s.Contains("обслуж");
        }
    }
}
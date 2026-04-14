using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfMaterialControls.ViewModels
{
    public class UserItemViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Initials { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string JoinedText { get; set; }
        public string RidesRevenueText { get; set; }
        public bool IsBlocked { get; set; }
        public string StatusText => IsBlocked ? "Заблокирован" : "Активен";
        public string StatusBackground => IsBlocked ? "#FDECEF" : "#EAF8EC";
        public string StatusForeground => IsBlocked ? "#CC4B62" : "#2EA35F";
    }
}

using System.Windows.Controls;
using WpfMaterialControls.ViewModels;

namespace WpfMaterialControls
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            DataContext = new UsersViewModel();
        }
    }
}

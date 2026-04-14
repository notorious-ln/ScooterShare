using System.Windows.Controls;
using WpfMaterialControls.ViewModels;

namespace WpfMaterialControls
{
    public partial class ScootersView : UserControl
    {
        public ScootersView()
        {
            InitializeComponent();
            DataContext = new ScootersViewModel();
        }
    }
}
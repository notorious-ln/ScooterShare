using System.Windows.Controls;
using WpfMaterialControls.ViewModels;

namespace WpfMaterialControls
{
    public partial class RidesView : UserControl
    {
        public RidesView()
        {
            InitializeComponent();
            DataContext = new RidesViewModel();
        }
    }
}

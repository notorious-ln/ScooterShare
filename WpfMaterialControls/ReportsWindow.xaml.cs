using System.Windows;
using WpfMaterialControls.ViewModels;

namespace WpfMaterialControls
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
            DataContext = new RidesReportViewModel();
        }
    }
}


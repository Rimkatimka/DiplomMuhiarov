using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Admin;

namespace EnergyMeteringSystem.App.Views.Admin
{
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();
            DataContext = new AuditLogViewModel();
        }
    }
}
using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Admin;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для AuditLogView.xaml
    /// </summary>
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();
            DataContext = new AuditLogViewModel();
        }
    }
}

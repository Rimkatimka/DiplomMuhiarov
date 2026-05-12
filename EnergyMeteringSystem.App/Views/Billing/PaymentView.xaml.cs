using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Billing;
using EnergyMeteringSystem.Core.Models.DTO;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Billing
{
    /// <summary>
    /// Логика взаимодействия для PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new PaymentViewModel(currentUser);
        }
        private void TextBox_PreviewTextInput_Decimal(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }

        private void TextBox_PreviewKeyDown_BlockSpace(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }
    }
}

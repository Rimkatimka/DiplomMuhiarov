using EnergyMeteringSystem.App.Helpers;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Tariffs
{
    /// <summary>
    /// Логика взаимодействия для TariffEditView.xaml
    /// </summary>
    public partial class TariffEditView : UserControl
    {
        public TariffEditView()
        {
            InitializeComponent();
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

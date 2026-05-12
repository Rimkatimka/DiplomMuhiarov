using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Admin;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserEditView.xaml
    /// </summary>
    public partial class UserEditView : UserControl
    {
        public UserEditView(UserEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;  // ← устанавливаем DataContext
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictEmailCharacters(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputValidator.ValidateEmailOnLostFocus(sender, e);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputValidator.ResetEmailBorderOnFocus(sender, e);
        }
    }
}


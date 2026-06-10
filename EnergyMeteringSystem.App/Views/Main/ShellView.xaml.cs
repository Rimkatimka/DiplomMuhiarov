using System.Windows;
using System.Windows.Controls;
using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Main;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Main
{
    public partial class ShellView : Window
    {
        private bool _isMenuCollapsed = false;
        private double _expandedWidth = 250;
        private double _collapsedWidth = 0;  // ← не 0, а 50px для иконок

        private Thickness _expandedMargin = new Thickness(60, 10, 10, 10);
        private Thickness _collapsedMargin = new Thickness(60, 10, 10, 10);

        public ShellView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new ShellViewModel(currentUser);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is MenuItemViewModel menuItem && menuItem.Command != null)
            {
                menuItem.Command.Execute(null);
            }
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var viewModel = DataContext as ShellViewModel;
            viewModel?.EditProfileCommand.Execute(null);
        }

        private async void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            _isMenuCollapsed = !_isMenuCollapsed;

            if (_isMenuCollapsed)
            {
                // Сворачиваем
                PanelAnimator.AnimatePanel(MenuPanel, _expandedWidth, _collapsedWidth, 250);

                // Анимация кнопки - поворот на 180 градусов
                PanelAnimator.AnimateButtonRotation(ToggleButtonText, 0, 243, 250);

                await System.Threading.Tasks.Task.Delay(100);
                ToggleButtonText.Text = "▶";
                ToggleMenuButton.ToolTip = "Развернуть меню";
            }
            else
            {
                // Разворачиваем
                PanelAnimator.AnimatePanel(MenuPanel, _collapsedWidth, _expandedWidth, 250);

                // Анимация кнопки - поворот обратно
                PanelAnimator.AnimateButtonRotation(ToggleButtonText, 180, 0, 250);

                await System.Threading.Tasks.Task.Delay(50);
                ToggleButtonText.Text = "◀";
                ToggleMenuButton.ToolTip = "Свернуть меню";
            }
        }

        private void SearchMenuTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var viewModel = DataContext as ShellViewModel;
            if (viewModel != null)
            {
                viewModel.SearchText = textBox.Text;
            }
        }
    }
}
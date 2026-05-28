using System.Windows;
using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Main;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Main
{
    public partial class ShellView : Window
    {
        private bool _isMenuCollapsed = false;
        private double _expandedWidth = 250;
        private double _collapsedWidth = 0;

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

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            _isMenuCollapsed = !_isMenuCollapsed;

            var menuPanel = MenuPanel;
            var toggleButtonText = ToggleButtonText;
            var mainContent = MainContent;

            if (_isMenuCollapsed)
            {
                menuPanel.Width = _collapsedWidth;
                menuPanel.Visibility = Visibility.Collapsed;

                if (toggleButtonText != null)
                    toggleButtonText.Text = "▶";
                ToggleMenuButton.ToolTip = "Развернуть меню";

                if (mainContent != null)
                    mainContent.Margin = new Thickness(10, 10, 10, 10);
            }
            else
            {
                menuPanel.Width = _expandedWidth;
                menuPanel.Visibility = Visibility.Visible;

                if (toggleButtonText != null)
                    toggleButtonText.Text = "◀";
                ToggleMenuButton.ToolTip = "Свернуть меню";

                if (mainContent != null)
                    mainContent.Margin = new Thickness(50, 10, 10, 10);
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
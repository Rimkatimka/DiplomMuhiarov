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
        private void SearchMenuTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            var viewModel = DataContext as ShellViewModel;
            if (viewModel != null)
            {
                viewModel.SearchText = textBox.Text;
            }
        }

        private void ToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            _isMenuCollapsed = !_isMenuCollapsed;

            var button = sender as Button;
            var menuPanel = MenuPanel;
            var userInfoPanel = UserInfoPanel;
            var searchPanel = SearchPanel;  // ← добавить
            var mainMenuTree = MainMenuTree;
            var logoutButton = LogoutButton;

            if (_isMenuCollapsed)
            {
                // Сворачиваем меню - ширина 0
                menuPanel.Width = _collapsedWidth;
                button.Content = "☷";
                button.ToolTip = "Развернуть меню (показать боковую панель)";

                // Скрываем все внутренние элементы
                userInfoPanel.Visibility = Visibility.Collapsed;
                if (searchPanel != null) searchPanel.Visibility = Visibility.Collapsed;
                mainMenuTree.Visibility = Visibility.Collapsed;

                // Меняем кнопку выхода
                logoutButton.Content = "⏻";
                logoutButton.Padding = new Thickness(10);
                logoutButton.Margin = new Thickness(5);
                logoutButton.Width = 40;
                logoutButton.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                // Разворачиваем меню
                menuPanel.Width = _expandedWidth;
                button.Content = "☰";
                button.ToolTip = "Свернуть меню (скрыть боковую панель)";

                // Показываем внутренние элементы
                userInfoPanel.Visibility = Visibility.Visible;
                if (searchPanel != null) searchPanel.Visibility = Visibility.Visible;
                mainMenuTree.Visibility = Visibility.Visible;

                // Возвращаем кнопку выхода
                logoutButton.Content = "Выйти";
                logoutButton.Padding = new Thickness(10);
                logoutButton.Margin = new Thickness(10);
                logoutButton.Width = double.NaN;
                logoutButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
    }
}
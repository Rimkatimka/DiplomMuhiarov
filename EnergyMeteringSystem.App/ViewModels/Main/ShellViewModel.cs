using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Services.Auth;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.Commands;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private UserDto _currentUser;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ShellViewModel()
        {
            _authService = new AuthService();
            _currentUser = _authService.GetCurrentUser();
            if (_currentUser.IsInspector || _currentUser.IsAdmin)
            {
                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Верификация",
                    Command = new RelayCommand(_ => OpenVerification())
                });
            }

            if (_currentUser.IsAccountant || _currentUser.IsAdmin)
            {
                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Начисления",
                    Command = new RelayCommand(_ => OpenAccrual())
                });
            }

            MenuItems.Add(new MenuItemViewModel
            {
                Title = "История показаний",
                Command = new RelayCommand(_ => OpenReadingHistory())
            });

            MenuItems = new ObservableCollection<MenuItemViewModel>();
            BuildMenu();
            
        }
        

        private void BuildMenu()
        {
            // Главная — всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Главная",
                Command = new RelayCommand(_ => OpenDashboard())
            });

            // Объекты — всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Объекты",
                Command = new RelayCommand(_ => OpenObjects())
            });

            // Показания
            var readingsMenu = new MenuItemViewModel { Title = "Показания" };
            readingsMenu.Command = new RelayCommand(_ => { }); // заглушка

            // Ввод показаний — всем
            readingsMenu.Command = new RelayCommand(_ => OpenReadingInput());

            // Верификация — только инспектору и админу
            if (_currentUser.IsInspector || _currentUser.IsAdmin)
            {
                readingsMenu.Command = new RelayCommand(_ => OpenVerification());
            }

            MenuItems.Add(readingsMenu);

            // Начисления и платежи — только бухгалтеру и админу
            if (_currentUser.IsAccountant || _currentUser.IsAdmin)
            {
                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Начисления",
                    Command = new RelayCommand(_ => OpenAccrual())
                });

                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Платежи",
                    Command = new RelayCommand(_ => OpenPayment())
                });
            }

            // Справочники — только админу
            if (_currentUser.IsAdmin)
            {
                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Справочники",
                    Command = new RelayCommand(_ => OpenDirectories())
                });

                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Пользователи",
                    Command = new RelayCommand(_ => OpenUsers())
                });
            }

            // Выход — всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Выход",
                Command = new RelayCommand(_ => Logout())
            });
        }

        private void OpenDashboard() { }
        private void OpenObjects()
        {
            var view = new Views.Objects.ConsumptionObjectListView();
        }
        private void OpenReadingInput() { }
        private void OpenVerification()
        {
            var view = new Views.Readings.MeterReadingVerificationView();
            var win = new Window
            {
                Title = "Верификация показаний",
                Content = view,
                Width = 1000,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
        }
        private void OpenAccrual()
        {
            var view = new Views.Billing.AccrualView();
            var win = new Window
            {
                Title = "Начисления за потребление",
                Content = view,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
        }
        private void OpenPayment() { }
        private void OpenDirectories() { }
        private void OpenUsers() { }
        private void OpenReadingHistory()
        {
            var view = new Views.Readings.MeterReadingHistoryView();
            var win = new Window
            {
                Title = "История показаний",
                Content = view,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
        }

        private void Logout()
        {
            _authService.Logout();
            System.Windows.Application.Current.Windows[0]?.Close();
            var login = new Views.Auth.LoginView();
            login.Show();
        }
    }
}

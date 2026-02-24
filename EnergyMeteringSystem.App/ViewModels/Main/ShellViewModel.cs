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
using EnergyMeteringSystem.App.ViewModels.Directories;

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

            if (_currentUser.IsAdmin)
            {
                var dirMenu = new MenuItemViewModel { Title = "Справочники" };

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы счетчиков",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateMeterTypeViewModel(), "Типы счетчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы показаний",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateReadingStatusViewModel(), "Статусы показаний"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Способы оплаты",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreatePaymentMethodViewModel(), "Способы оплаты"))
                });

                MenuItems.Add(dirMenu);
            }
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "История показаний",
                Command = new RelayCommand(_ => OpenReadingHistory())
            });

            if (_currentUser.IsAdmin)
            {
                var dirMenu = new MenuItemViewModel { Title = "Справочники" };

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы счетчиков",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateMeterTypeViewModel(), "Типы счетчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы показаний",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateReadingStatusViewModel(), "Статусы показаний"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Способы оплаты",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreatePaymentMethodViewModel(), "Способы оплаты"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы объектов",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateObjectTypeViewModel(), "Типы объектов"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Причины отклонения",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateRejectionReasonViewModel(), "Причины отклонения"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы счетчиков",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateMeterStatusViewModel(), "Статусы счетчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы договоров",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateContractStatusViewModel(), "Статусы договоров"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы тарифов",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateTariffTypeViewModel(), "Типы тарифов"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Единицы измерения",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateUnitOfMeasureViewModel(), "Единицы измерения"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Источники энергии",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateEnergySourceViewModel(), "Источники энергии"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Интервалы поверки",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateVerificationIntervalViewModel(), "Интервалы поверки"))
                });

                MenuItems.Add(dirMenu);
            }

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

            // Ввод показаний — всем
            readingsMenu.Children.Add(new MenuItemViewModel
            {
                Title = "Ввод показаний",
                Command = new RelayCommand(_ => OpenReadingInput())
            });

            // Верификация — только инспектору и админу
            if (_currentUser.IsInspector || _currentUser.IsAdmin)
            {
                readingsMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Верификация",
                    Command = new RelayCommand(_ => OpenVerification())
                });
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
                var dirMenu = new MenuItemViewModel { Title = "Справочники" };

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы счетчиков",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateMeterTypeViewModel(),
                        "Типы счетчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы показаний",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateReadingStatusViewModel(),
                        "Статусы показаний"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Способы оплаты",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreatePaymentMethodViewModel(),
                        "Способы оплаты"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы объектов",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateObjectTypeViewModel(),
                        "Типы объектов"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Причины отклонения",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateRejectionReasonViewModel(),
                        "Причины отклонения"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы счетчиков",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateMeterStatusViewModel(),
                        "Статусы счетчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы договоров",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateContractStatusViewModel(),
                        "Статусы договоров"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы тарифов",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateTariffTypeViewModel(),
                        "Типы тарифов"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Единицы измерения",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateUnitOfMeasureViewModel(),
                        "Единицы измерения"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Источники энергии",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateEnergySourceViewModel(),
                        "Источники энергии"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Интервалы поверки",
                    Command = new RelayCommand(_ => OpenDirectory(
                        DirectoryFactory.CreateVerificationIntervalViewModel(),
                        "Интервалы поверки"))
                });

                MenuItems.Add(dirMenu);

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
        private void OpenPayment()
        {
            var view = new Views.Billing.PaymentView();
            var win = new Window
            {
                Title = "Платежи",
                Content = view,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
        }
        private void OpenDirectory(DirectoryListViewModel viewModel, string title)
        {
            var view = new Views.Directories.DirectoryListView();
            view.DataContext = viewModel;

            var win = new Window
            {
                Title = title,
                Content = view,
                Width = 800,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
        }
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

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
        public UserDto CurrentUser => AuthService.CurrentUser;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
        public RelayCommand LogoutCommand { get; }
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        private MenuItemViewModel _selectedMenuItem;
        public MenuItemViewModel SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value) && value?.Command != null)
                {
                    value.Command.Execute(null);
                }
            }
        }

        public ShellViewModel()
        {
            if (CurrentUser == null)
            {
                // Если пользователь не найден — закрываем окно
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Ошибка авторизации");
                    Application.Current.Windows.OfType<Views.Main.ShellView>().FirstOrDefault()?.Close();
                    new Views.Auth.LoginView().Show();
                });
                return;
            }

            LogoutCommand = new RelayCommand(_ => Logout());
            MenuItems = new ObservableCollection<MenuItemViewModel>();
            BuildMenu();
        }

        private void BuildMenu()
        {
            // Главная - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Главная",
                Command = new RelayCommand(_ => OpenDashboard())
            });

            // Объекты - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Объекты",
                Command = new RelayCommand(_ => OpenObjects())
            });

            // Показания (главное меню)
            var readingsMenu = new MenuItemViewModel { Title = "Показания" };

            readingsMenu.Children.Add(new MenuItemViewModel
            {
                Title = "Ввод показаний",
                Command = new RelayCommand(_ => OpenReadingInput())
            });

            readingsMenu.Children.Add(new MenuItemViewModel
            {
                Title = "История показаний",
                Command = new RelayCommand(_ => OpenReadingHistory())
            });

            // Верификация - только инспектор и админ
            if (CurrentUser.IsInspector || CurrentUser.IsAdmin)
            {
                readingsMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Верификация",
                    Command = new RelayCommand(_ => OpenVerification())
                });
            }

            MenuItems.Add(readingsMenu);

            // Начисления и платежи - бухгалтер и админ
            if (CurrentUser.IsAccountant || CurrentUser.IsAdmin)
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

                MenuItems.Add(new MenuItemViewModel
                {
                    Title = "Задолженность",
                    Command = new RelayCommand(_ => OpenDebt())
                });
            }

            // Отчёты - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Отчёты",
                Command = new RelayCommand(_ => OpenReports())
            });

            // Договоры - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Договоры",
                Command = new RelayCommand(_ => OpenContracts())
            });

            // Справочники - только админ
            if (CurrentUser.IsAdmin)
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

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Тарифы",
                    Command = new RelayCommand(_ => OpenTariffs())
                });

                MenuItems.Add(dirMenu);

                // Администрирование
                var adminMenu = new MenuItemViewModel { Title = "Администрирование" };

                adminMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Пользователи",
                    Command = new RelayCommand(_ => OpenUserManagement())
                });

                adminMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Журнал аудита",
                    Command = new RelayCommand(_ => OpenAuditLog())
                });

                adminMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Резервное копирование",
                    Command = new RelayCommand(_ => OpenBackup())
                });

                MenuItems.Add(adminMenu);
            }

            // Выход - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Выход",
                Command = new RelayCommand(_ => Logout())
            });
        }

        // Методы открытия окон
        private void OpenDashboard()
        {
            CurrentView = new Views.Main.DashboardView();
        }

        private void OpenObjects()
        {
            CurrentView = new Views.Objects.ConsumptionObjectListView();
        }

        private void OpenReadingInput()
        {
            CurrentView = new Views.Readings.MeterReadingInputView();
        }

        private void OpenReadingHistory()
        {
            CurrentView = new Views.Readings.MeterReadingHistoryView();
        }

        private void OpenVerification()
        {
            CurrentView = new Views.Readings.MeterReadingVerificationView();
        }

        private void OpenAccrual()
        {
            CurrentView = new Views.Billing.AccrualView();
        }

        private void OpenPayment()
        {
            CurrentView = new Views.Billing.PaymentView();
        }

        private void OpenDebt()
        {
            CurrentView = new Views.Billing.DebtView();
        }

        private void OpenReports()
        {
            CurrentView = new Views.Reports.ReportView();
        }

        private void OpenContracts()
        {
            CurrentView = new Views.Contracts.ContractListView();
        }

        private void OpenTariffs()
        {
            CurrentView = new Views.Tariffs.TariffListView();
        }

        private void OpenUserManagement()
        {
            CurrentView = new Views.Admin.UserManagementView();
        }

        private void OpenAuditLog()
        {
            CurrentView = new Views.Admin.AuditLogView();
        }

        private void OpenBackup()
        {
            CurrentView = new Views.Admin.BackupView();
        }

        private void OpenDirectory(DirectoryListViewModel viewModel, string title)
        {
            CurrentView = new Views.Directories.DirectoryListView();        
        }

        private void Logout()
        {
            new Views.Auth.LoginView().Show();
            AuthService.Logout();
            Application.Current.Windows[0]?.Close();               
        }
    }
}

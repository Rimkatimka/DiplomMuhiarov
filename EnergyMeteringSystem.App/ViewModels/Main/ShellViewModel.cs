using System.Collections.ObjectModel;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Services.Auth;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class ShellViewModel : ViewModelBase
    {

        public UserDto CurrentUser { get; }
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

        public ShellViewModel(UserDto currentUser)
        {
            System.Diagnostics.Debug.WriteLine("ShellViewModel: конструктор начат");

            CurrentUser = currentUser;  // ← получаем пользователя из параметра

            System.Diagnostics.Debug.WriteLine($"ShellViewModel: _currentUser = {CurrentUser?.Username ?? "null"}");

            if (CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("ShellViewModel: _currentUser == null, выход");
                return;
            }

            LogoutCommand = new RelayCommand(_ => Logout());
            MenuItems = [];
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

            //MenuItems.Add(new MenuItemViewModel
            //{
            //    Title = "Счетчики",
            //    Command = new RelayCommand(_ => OpenMeterList())  // ← вызов здесь
            //});

            // Объекты - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Объекты",
                Command = new RelayCommand(_ => OpenObjects())
            });

            // Показания (главное меню)
            MenuItemViewModel readingsMenu = new() { Title = "Показания" };

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
                MenuItemViewModel dirMenu = new() { Title = "Справочники" };

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
                MenuItemViewModel adminMenu = new() { Title = "Администрирование" };

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
        private void OpenMeterList()
        {
            CurrentView = new Views.Meters.MeterListView();  // ← открытие окна
        }

        private void OpenObjects()
        {
            CurrentView = new Views.Objects.ConsumptionObjectListView();
        }

        private void OpenReadingInput()
        {
            if (CurrentUser == null)
            {
                _ = System.Windows.MessageBox.Show("Ошибка: пользователь не авторизован", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            // Передаем текущего пользователя в View
            Views.Readings.MeterReadingInputView view = new(CurrentUser);
            CurrentView = view;
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
            if (CurrentUser == null)
            {
                _ = MessageBox.Show("Ошибка: пользователь не авторизован", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Передаем текущего пользователя в View
            Views.Billing.PaymentView view = new(CurrentUser);
            CurrentView = view;
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
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: viewModel = null для {title}");
                return;
            }

            CurrentView = new Views.Directories.DirectoryListView { DataContext = viewModel };
        }

        private void Logout()
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                new Views.Auth.LoginView().Show();
                Application.Current.Windows[0]?.Close();
            }

        }
    }
}

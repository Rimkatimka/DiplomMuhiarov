using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Admin;
using EnergyMeteringSystem.App.ViewModels.Analytics;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Auth;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class ShellViewModel : ViewModelBase
    {
        private UserDto _currentUser;
        private object _currentView;
        private MenuItemViewModel _selectedMenuItem;
        private string _searchText;
        private ObservableCollection<MenuItemViewModel> _filteredMenuItems;

        public UserDto CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public AsyncRelayCommand EditProfileCommand { get; }
        public AsyncRelayCommand LogoutCommand { get; }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterMenu();
                }
            }
        }

        public ObservableCollection<MenuItemViewModel> FilteredMenuItems
        {
            get => _filteredMenuItems;
            set => SetProperty(ref _filteredMenuItems, value);
        }

        public ShellViewModel(UserDto currentUser)
        {
            System.Diagnostics.Debug.WriteLine("ShellViewModel: конструктор начат");

            CurrentUser = currentUser;

            System.Diagnostics.Debug.WriteLine($"ShellViewModel: CurrentUser = {CurrentUser?.Username ?? "null"}");

            if (CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("ShellViewModel: CurrentUser == null, выход");
                return;
            }

            LogoutCommand = new AsyncRelayCommand(async () => await LogoutAsync());
            EditProfileCommand = new AsyncRelayCommand(async () => await EditProfileAsync());
            MenuItems = new ObservableCollection<MenuItemViewModel>();
            FilteredMenuItems = new ObservableCollection<MenuItemViewModel>();

            BuildMenu();
            FilterMenu();
            CurrentView = new Views.Main.DashboardView();
        }

        private void BuildMenu()
        {
            MenuItems.Clear();

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

            if (CurrentUser.IsInspector || CurrentUser.IsAdmin)
            {
                readingsMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Верификация",
                    Command = new RelayCommand(_ => OpenVerification())
                });
            }

            MenuItems.Add(readingsMenu);

            // Отчёты - всем
            MenuItems.Add(new MenuItemViewModel
            {
                Title = "Отчёты",
                Command = new RelayCommand(_ => OpenReports())
            });

            // Аналитика - подменю
            MenuItemViewModel analyticsMenu = new() { Title = "Аналитика" };

            analyticsMenu.Children.Add(new MenuItemViewModel
            {
                Title = "По объектам",
                Command = new RelayCommand(_ => OpenAnalytics())
            });

            analyticsMenu.Children.Add(new MenuItemViewModel
            {
                Title = "По регионам (иерархия)",
                Command = new RelayCommand(_ => OpenHierarchyAnalytics())
            });

            MenuItems.Add(analyticsMenu);

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
                    Title = "Типы объектов",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateObjectTypeViewModel(), "Типы объектов"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Причины отклонения показаний",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateRejectionReasonViewModel(), "Причины отклонения"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Статусы счётчиков",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateMeterStatusViewModel(), "Статусы счётчиков"))
                });

                dirMenu.Children.Add(new MenuItemViewModel
                {
                    Title = "Типы счётчиков",
                    Command = new RelayCommand(_ => OpenDirectory(DirectoryFactory.CreateMeterTypeViewModel(), "Типы счётчиков"))
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

            System.Diagnostics.Debug.WriteLine($"BuildMenu: добавлено {MenuItems.Count} пунктов меню");
        }

        private void FilterMenu()
        {
            System.Diagnostics.Debug.WriteLine($"FilterMenu: SearchText='{SearchText}', MenuItems.Count={MenuItems?.Count ?? 0}");

            if (MenuItems == null || MenuItems.Count == 0)
            {
                FilteredMenuItems = new ObservableCollection<MenuItemViewModel>();
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredMenuItems = new ObservableCollection<MenuItemViewModel>(MenuItems);
                System.Diagnostics.Debug.WriteLine($"FilterMenu: показано {FilteredMenuItems.Count} пунктов");
                return;
            }

            var filtered = new ObservableCollection<MenuItemViewModel>();
            var lowerSearch = SearchText.ToLower();

            foreach (var item in MenuItems)
            {
                bool itemMatches = item.Title.ToLower().Contains(lowerSearch);
                var matchingChildren = item.Children.Where(c => c.Title.ToLower().Contains(lowerSearch)).ToList();

                if (itemMatches || matchingChildren.Any())
                {
                    var newItem = new MenuItemViewModel { Title = item.Title, Command = item.Command };

                    if (matchingChildren.Any())
                    {
                        foreach (var child in matchingChildren)
                        {
                            newItem.Children.Add(child);
                        }
                    }
                    else if (itemMatches)
                    {
                        foreach (var child in item.Children)
                        {
                            newItem.Children.Add(child);
                        }
                    }

                    filtered.Add(newItem);
                }
            }

            FilteredMenuItems = filtered;
            System.Diagnostics.Debug.WriteLine($"FilterMenu: отфильтровано {filtered.Count} пунктов");
        }

        private async Task EditProfileAsync()
        {
            var userRepository = new UserRepository();
            var roles = new ObservableCollection<UserRoleDto>(await userRepository.GetAllRolesAsync());

            var editViewModel = new UserEditViewModel(roles, CurrentUser, CurrentUser);
            var editView = new Views.Admin.UserEditView(editViewModel);

            var window = new Window
            {
                Title = "Редактирование профиля",
                Content = editView,
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow
            };

            editViewModel.OnSaved += async (s, e) =>
            {
                var updatedUser = await userRepository.GetByIdAsync(CurrentUser.Id);
                if (updatedUser != null)
                {
                    CurrentUser = updatedUser;
                }
                window.Close();
            };

            window.ShowDialog();
        }

        private async Task LogoutAsync()
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (CurrentUser != null)
                {
                    Core.Helpers.AuditLogger.Log("LOGOUT", "User", CurrentUser.Id, null,
                        new { CurrentUser.Username }, CurrentUser.Id);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    new Views.Auth.LoginView().Show();
                    Application.Current.Windows[0]?.Close();
                });
            }
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

        private void OpenAnalytics()
        {
            CurrentView = new Views.Analytics.AnalyticsView();
        }

        private void OpenHierarchyAnalytics()
        {
            // ✅ ПРАВИЛЬНО - создаем ViewModel и передаем ее во View
            var viewModel = new HierarchyAnalyticsViewModel();
            var view = new Views.Analytics.HierarchyAnalyticsView();
            view.DataContext = viewModel;  // ← ЭТО ВАЖНО!
            CurrentView = view;
        }

        private void OpenReadingInput()
        {
            if (CurrentUser == null)
            {
                MessageBox.Show("Ошибка: пользователь не авторизован", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private void OpenReports()
        {
            CurrentView = new Views.Reports.ReportView();
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
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Main;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class UserManagementViewModel : ListViewModelBase<UserDto, UserRepository>
    {
        private UserDto _currentUser;
        public ObservableCollection<UserRoleDto> Roles { get; set; } = new();

        // ✅ ИСПРАВЛЕНО: используем ItemsList вместо Items
        public ObservableCollection<UserDto> FilteredUsers => new ObservableCollection<UserDto>(FilteredItemsList);

        public UserDto SelectedUser
        {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        // ✅ ИСПРАВЛЕНО: используем ItemsList вместо Items
        public ObservableCollection<UserDto> Users => new ObservableCollection<UserDto>(ItemsList);

        public UserDto CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public AsyncRelayCommand BlockCommand { get; }
        public AsyncRelayCommand ResetPasswordCommand { get; }

        public UserManagementViewModel() : base(new UserRepository())
        {
            BlockCommand = new AsyncRelayCommand(async () => await BlockUserAsync(), CanExecuteSelectionCommand);
            ResetPasswordCommand = new AsyncRelayCommand(async () => await ResetPasswordAsync(), CanExecuteSelectionCommand);

            EditCommand = new AsyncRelayCommand(async () => await EditAsync(), CanExecuteSelectionCommand);

            _ = LoadRolesAsync();
            LoadCurrentUser();
        }

        private void LoadCurrentUser()
        {
            if (Application.Current.MainWindow?.DataContext is ShellViewModel shell)
            {
                CurrentUser = shell.CurrentUser;
            }
        }

        private bool CanExecuteSelectionCommand() => !IsLoading && SelectedItem != null;

        protected override void OnSelectedItemChanged()
        {
            base.OnSelectedItemChanged();
            (BlockCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (ResetPasswordCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        protected override void RefreshAllCommandsCanExecute()
        {
            base.RefreshAllCommandsCanExecute();
            (BlockCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (ResetPasswordCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task LoadRolesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var list = await _repository.GetAllRolesAsync();
                Roles.Clear();
                foreach (var role in list)
                    Roles.Add(role);
            }, "Ошибка загрузки ролей");
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                if (Roles.Count == 0)
                {
                    var roles = await _repository.GetAllRolesAsync();
                    Roles.Clear();
                    foreach (var role in roles)
                        Roles.Add(role);
                }

                var list = await _repository.GetAllAsync();

                // ✅ ИСПРАВЛЕНО: используем ItemsList
                ItemsList.Clear();
                foreach (var user in list)
                    ItemsList.Add(user);

                ApplyFilter();
            }, "Ошибка загрузки пользователей");
        }

        protected override async Task AddAsync()
        {
            if (Roles.Count == 0)
            {
                var roles = await _repository.GetAllRolesAsync();
                Roles.Clear();
                foreach (var role in roles)
                    Roles.Add(role);
            }

            var addViewModel = new UserEditViewModel(Roles, CurrentUser);
            var addView = new Views.Admin.UserEditView(addViewModel);

            var window = new Window
            {
                Title = "Новый пользователь",
                Content = addView,
                Width = 500,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow
            };

            addViewModel.OnSaved += async (s, e) =>
            {
                await LoadDataAsync();
                window.Close();
            };

            window.ShowDialog();
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null) return;

            var editViewModel = new UserEditViewModel(Roles, SelectedItem, CurrentUser);
            var editView = new Views.Admin.UserEditView(editViewModel);

            var window = new Window
            {
                Title = editViewModel.IsSelfEdit ? "Редактирование профиля" : "Редактирование пользователя",
                Content = editView,
                Width = 500,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow
            };

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadDataAsync();
                window.Close();
            };

            window.ShowDialog();
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            if (!CanDeleteUser())
            {
                await ShowMessageAsync("Нельзя удалить этого пользователя", "Предупреждение");
                return;
            }

            var result = await ShowConfirmationAsync(
                $"Удалить пользователя {SelectedItem.FullName}?\n\nВНИМАНИЕ: Это действие нельзя отменить!",
                "Подтверждение удаления");

            if (result)
            {
                await ExecuteAsync(async () =>
                {
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await LoadDataAsync();
                    await ShowMessageAsync("Пользователь удален", "Успех");
                }, "Ошибка при удалении");
            }
        }

        private async Task BlockUserAsync()
        {
            if (SelectedItem == null) return;

            if (CurrentUser != null && CurrentUser.Id == SelectedItem.Id)
            {
                await ShowMessageAsync("Нельзя заблокировать свою учетную запись", "Предупреждение");
                return;
            }

            bool newStatus = !SelectedItem.IsActive;
            string action = newStatus ? "разблокировать" : "заблокировать";
            string successText = newStatus ? "разблокирован" : "заблокирован";

            var result = await ShowConfirmationAsync(
                $"{char.ToUpper(action[0])}{action.Substring(1)} пользователя {SelectedItem.FullName}?",
                "Подтверждение");

            if (result)
            {
                var userId = SelectedItem.Id;
                await ExecuteAsync(async () =>
                {
                    await _repository.SetActiveStatusAsync(userId, newStatus);
                    await LoadDataAsync();
                    await ShowMessageAsync($"Пользователь {successText}", "Успех");
                }, $"Ошибка при попытке {action} пользователя");

                if (!string.IsNullOrEmpty(ErrorMessage))
                    await ShowMessageAsync(ErrorMessage, "Ошибка");
            }
        }

        private async Task ResetPasswordAsync()
        {
            if (SelectedItem == null) return;

            var result = await ShowConfirmationAsync(
                $"Сбросить пароль для {SelectedItem.FullName}?\nНовый пароль: 12345",
                "Подтверждение");

            if (result)
            {
                var userId = SelectedItem.Id;
                await ExecuteAsync(async () =>
                {
                    string newHash = PasswordHelper.HashPassword("12345");
                    await _repository.ResetPasswordAsync(userId, newHash);
                    await ShowMessageAsync("Пароль сброшен на 12345", "Успех");
                }, "Ошибка при сбросе пароля");

                if (!string.IsNullOrEmpty(ErrorMessage))
                    await ShowMessageAsync(ErrorMessage, "Ошибка");
            }
        }

        private bool CanDeleteUser()
        {
            if (SelectedItem == null) return false;
            if (CurrentUser == null) return false;
            if (!CurrentUser.IsAdmin) return false;
            if (SelectedItem.IsAdmin) return false;
            if (CurrentUser.Id == SelectedItem.Id) return false;
            return true;
        }

        protected override bool ItemMatchesSearch(UserDto item, string searchText)
        {
            return item.FullName.Contains(searchText) ||
                   item.Username.Contains(searchText) ||
                   item.Email.Contains(searchText);
        }

        private async Task ShowMessageAsync(string message, string title)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private async Task<bool> ShowConfirmationAsync(string message, string title)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            });
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class AuditLogViewModel : ListViewModelBase<AuditLogDto, AuditRepository>
    {
        private DateTime _fromDate;
        private DateTime _toDate;

        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public AuditLogViewModel() : base(new AuditRepository())
        {
            // 30 дней назад по сегодня
            _fromDate = DateTime.Today.AddDays(-30);
            _toDate = DateTime.Today;

            // Команды переопределяем, так как у AuditLog нет Add/Edit/Delete
            AddCommand = null;
            EditCommand = null;
            DeleteCommand = null;
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: LoadDataAsync, FromDate={FromDate:dd.MM.yyyy}, ToDate={ToDate:dd.MM.yyyy}");

                var list = await _repository.GetByDateAsync(FromDate, ToDate);

                Items.Clear();
                foreach (var log in list)
                {
                    Items.Add(log);
                }

                ApplyFilter();

                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: loaded {Items.Count} logs, filtered {FilteredItems.Count}");
            }, "Ошибка загрузки журнала аудита");
        }

        // Не используются в AuditLog, но требуются интерфейсом
        protected override Task AddAsync() => Task.CompletedTask;
        protected override Task EditAsync() => Task.CompletedTask;
        protected override Task DeleteAsync() => Task.CompletedTask;

        protected override void ApplyFilter()
        {
            FilteredItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Items
                : new ObservableCollection<AuditLogDto>(
                    Items.Where(l =>
                        (l.UserName?.Contains(SearchText) ?? false) ||
                        (l.ActionType?.Contains(SearchText) ?? false) ||
                        (l.TableName?.Contains(SearchText) ?? false) ||
                        (l.DisplayDetails?.Contains(SearchText) ?? false)));

            foreach (var log in filtered)
            {
                FilteredItems.Add(log);
            }

            OnPropertyChanged(nameof(HasFilteredItems));
        }

        protected override bool ItemMatchesSearch(AuditLogDto item, string searchText)
        {
            return (item.UserName?.Contains(searchText) ?? false) ||
                   (item.ActionType?.Contains(searchText) ?? false) ||
                   (item.TableName?.Contains(searchText) ?? false) ||
                   (item.DisplayDetails?.Contains(searchText) ?? false);
        }
    }
}
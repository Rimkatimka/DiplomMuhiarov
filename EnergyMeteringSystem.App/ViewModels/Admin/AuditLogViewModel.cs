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

        public ObservableCollection<AuditLogDto> FilteredLogs => new ObservableCollection<AuditLogDto>(FilteredItemsList);

        public AuditLogDto SelectedLog
        {
            get => SelectedItem;
            set => SelectedItem = value;
        }

        public AuditLogViewModel() : base(new AuditRepository())
        {
            _fromDate = DateTime.Today.AddDays(-30);
            _toDate = DateTime.Today;

            AddCommand = null;
            EditCommand = null;
            DeleteCommand = null;

            // ✅ ПРИНУДИТЕЛЬНАЯ ЗАГРУЗКА
            _ = LoadDataAsync();
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: LoadDataAsync, FromDate={FromDate:dd.MM.yyyy}, ToDate={ToDate:dd.MM.yyyy}");

                var list = await _repository.GetByDateAsync(FromDate, ToDate);

                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: загружено {list.Count} записей");

                ItemsList.Clear();
                foreach (var log in list)
                {
                    ItemsList.Add(log);
                }

                ApplyFilter();

                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: после фильтрации {FilteredItemsList.Count} записей");
            }, "Ошибка загрузки журнала аудита");
        }

        protected override Task AddAsync() => Task.CompletedTask;
        protected override Task EditAsync() => Task.CompletedTask;
        protected override Task DeleteAsync() => Task.CompletedTask;

        protected override void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? ItemsList
                : ItemsList.Where(l =>
                    (l.UserName?.Contains(SearchText) ?? false) ||
                    (l.ActionType?.Contains(SearchText) ?? false) ||
                    (l.TableName?.Contains(SearchText) ?? false) ||
                    (l.DisplayDetails?.Contains(SearchText) ?? false)).ToList();

            FilteredItemsList = filtered;
            OnPropertyChanged(nameof(FilteredLogs));
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
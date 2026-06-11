using System;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class RegionEditViewModel : EditViewModelBase<RegionDto, RegionRepository>
    {
        private string _name;
        private string _code;

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public RegionEditViewModel() : base(new RegionRepository(), null)
        {
            Title = "Добавление региона";
        }

        public RegionEditViewModel(RegionDto existingRegion) : base(new RegionRepository(), existingRegion)
        {
            Title = "Редактирование региона";
        }

        protected override void LoadItem(RegionDto item)
        {
            Name = item.Name;
            Code = item.Code;
        }

        protected override RegionDto GetDto()
        {
            return new RegionDto
            {
                Id = _originalItem?.Id ?? 0,
                Name = Name?.Trim(),
                Code = Code?.Trim()
            };
        }

        protected override async Task SaveToRepositoryAsync(RegionDto dto)
        {
            if (IsEditMode)
                await _repository.UpdateAsync(dto);
            else
                await _repository.AddAsync(dto);
        }

        protected override bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}
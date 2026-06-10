using System;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class DirectoryEditViewModel : ViewModelBase
    {
        private string _name;
        private string _description;
        private readonly DirectoryDto _directory;

        public event EventHandler OnDirectorySaved;

        public string Name
        {
            get => _name;
            set
            {
                _ = SetProperty(ref _name, value);
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DirectoryEditViewModel(DirectoryDto existingItem = null)
        {
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (existingItem != null)
            {
                IsEditMode = true;
                _directory = existingItem;
                Name = existingItem.Name;
                Description = existingItem.Description;
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void Save()
        {
            OnDirectorySaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnDirectorySaved?.Invoke(this, EventArgs.Empty);
        }
    }
}

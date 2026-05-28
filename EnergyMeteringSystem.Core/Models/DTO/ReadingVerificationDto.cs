using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnergyMeteringSystem.App.DTO
{
    public class ReadingVerificationDto : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Id { get; set; }
        public string Address { get; set; }
        public string SerialNumber { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public decimal? PreviousValue { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
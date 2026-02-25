using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Auth;

namespace EnergyMeteringSystem.App.ViewModels.Billing
{
    public class PaymentViewModel : ViewModelBase
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly PaymentMethodRepository _methodRepository;

        private int _selectedYear;
        private int _selectedMonth;
        private ConsumptionObjectDto _selectedObject;
        private PaymentMethodDto _selectedMethod;
        private decimal _amount;
        private string _receiptNumber;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<PaymentMethodDto> PaymentMethods { get; set; }
        public ObservableCollection<PaymentDto> Payments { get; set; }

        private string _selectedMonthName;
        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                SetProperty(ref _selectedMonthName, value);
                SelectedMonth = Array.IndexOf(Months.ToArray(), value) + 1;
            }
        }
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                SetProperty(ref _selectedYear, value);
                LoadPayments();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                SetProperty(ref _selectedMonth, value);
                LoadPayments();
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }

        public PaymentMethodDto SelectedMethod
        {
            get => _selectedMethod;
            set => SetProperty(ref _selectedMethod, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public string ReceiptNumber
        {
            get => _receiptNumber;
            set => SetProperty(ref _receiptNumber, value);
        }

        public decimal TotalPayments => Payments?.Sum(p => p.Amount) ?? 0;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand PrintReceiptCommand { get; }

        public PaymentViewModel()
        {
            _paymentRepository = new PaymentRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _methodRepository = new PaymentMethodRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            PaymentMethods = new ObservableCollection<PaymentMethodDto>();
            Payments = new ObservableCollection<PaymentDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            SaveCommand = new RelayCommand(_ => SavePayment(), _ => CanSave());
            PrintReceiptCommand = new RelayCommand(_ => PrintReceipt(), _ => SelectedObject != null);

            InitializeData();
        }

        private void InitializeData()
        {
            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            Months.Add("Январь"); Months.Add("Февраль"); Months.Add("Март");
            Months.Add("Апрель"); Months.Add("Май"); Months.Add("Июнь");
            Months.Add("Июль"); Months.Add("Август"); Months.Add("Сентябрь");
            Months.Add("Октябрь"); Months.Add("Ноябрь"); Months.Add("Декабрь");

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;

            LoadObjects();
            LoadPaymentMethods();
            LoadPayments();
        }

        private void LoadObjects()
        {
            Objects.Clear();
            var list = _objectRepository.GetAll();
            foreach (var obj in list)
                Objects.Add(obj);
        }

        private void LoadPaymentMethods()
        {
            PaymentMethods.Clear();
            var list = _methodRepository.GetAll();
            foreach (var method in list)
                PaymentMethods.Add(new PaymentMethodDto
                {
                    Id = method.Id,
                    Name = method.Name
                });
        }

        private void LoadPayments()
        {
            Payments.Clear();
            var list = _paymentRepository.GetByPeriod(_selectedYear, _selectedMonth);
            foreach (var payment in list)
                Payments.Add(payment);

            OnPropertyChanged(nameof(TotalPayments));
        }

        private void LoadData()
        {
            LoadObjects();
            LoadPaymentMethods();
            LoadPayments();
        }

        private bool CanSave()
        {
            return SelectedObject != null &&
                   SelectedMethod != null &&
                   Amount > 0;
        }

        private void SavePayment()
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser == null) return;

            var dto = new PaymentRegistrationDto
            {
                ConsumptionObjectId = SelectedObject.Id,
                Amount = Amount,
                PaymentMethodId = SelectedMethod.Id,
                PeriodMonth = _selectedMonth,
                PeriodYear = _selectedYear,
                ReceivedByUserId = currentUser.Id,
                ReceiptNumber = string.IsNullOrWhiteSpace(ReceiptNumber)
                    ? GenerateReceiptNumber()
                    : ReceiptNumber
            };

            _paymentRepository.Add(dto);

            // Очистка формы
            SelectedObject = null;
            SelectedMethod = null;
            Amount = 0;
            ReceiptNumber = string.Empty;

            LoadPayments();
        }

        private string GenerateReceiptNumber()
        {
            return $"ПЛ-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        private void PrintReceipt()
        {
            if (SelectedObject == null) return;

            // Заглушка для печати
            System.Windows.MessageBox.Show(
                $"Печать квитанции для объекта: {SelectedObject.Address}\nСумма: {Amount}",
                "Печать",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }

    // DTO для метода оплаты
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

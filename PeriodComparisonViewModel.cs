using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FleetManager.ViewModels
{
    public class PeriodComparisonViewModel : INotifyPropertyChanged
    {
        private readonly StatisticsService _statisticsService;
        private bool _isLoading;
        private DateTime? _period1StartDate;
        private DateTime? _period1EndDate;
        private DateTime? _period2StartDate;
        private DateTime? _period2EndDate;
        private string _period1Predefined;
        private string _period2Predefined;
        private DateTime _comparisonDate;

        // KPIs
        private decimal _fuelCostDifference;
        private decimal _fuelCostPercentage;
        private decimal _maintenanceCostDifference;
        private decimal _maintenancePercentage;
        private decimal _mileageDifference;
        private decimal _mileagePercentage;
        private decimal _consumptionDifference;
        private decimal _consumptionPercentage;

        // Charts
        private IEnumerable<ISeries> _comparisonSeries;
        private IEnumerable<Axis> _comparisonXAxes;
        private IEnumerable<Axis> _comparisonYAxes;

        private ObservableCollection<ComparisonDetail> _comparisonDetails;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public PeriodComparisonViewModel(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            _comparisonDate = DateTime.Now;
            _period1Predefined = string.Empty;
            _period2Predefined = string.Empty;
            _comparisonSeries = Array.Empty<ISeries>();
            _comparisonXAxes = Array.Empty<Axis>();
            _comparisonYAxes = Array.Empty<Axis>();
            _comparisonDetails = new ObservableCollection<ComparisonDetail>();

            // Initialiser les périodes par défaut (mois actuel vs mois dernier)
            var now = DateTime.Now;
            _period1StartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            _period1EndDate = new DateTime(now.Year, now.Month, 1).AddDays(-1);
            _period2StartDate = new DateTime(now.Year, now.Month, 1);
            _period2EndDate = now;

            InitializePredefinedPeriods();
            InitializeCommands();
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DateTime? Period1StartDate
        {
            get => _period1StartDate;
            set => SetProperty(ref _period1StartDate, value);
        }

        public DateTime? Period1EndDate
        {
            get => _period1EndDate;
            set => SetProperty(ref _period1EndDate, value);
        }

        public DateTime? Period2StartDate
        {
            get => _period2StartDate;
            set => SetProperty(ref _period2StartDate, value);
        }

        public DateTime? Period2EndDate
        {
            get => _period2EndDate;
            set => SetProperty(ref _period2EndDate, value);
        }

        public string Period1Predefined
        {
            get => _period1Predefined;
            set
            {
                SetProperty(ref _period1Predefined, value);
                if (!string.IsNullOrEmpty(value))
                    ApplyPredefinedPeriod(value, true);
            }
        }

        public string Period2Predefined
        {
            get => _period2Predefined;
            set
            {
                SetProperty(ref _period2Predefined, value);
                if (!string.IsNullOrEmpty(value))
                    ApplyPredefinedPeriod(value, false);
            }
        }

        public List<string> PredefinedPeriods { get; private set; } = new();

        public decimal FuelCostDifference
        {
            get => _fuelCostDifference;
            set => SetProperty(ref _fuelCostDifference, value);
        }

        public decimal FuelCostPercentage
        {
            get => _fuelCostPercentage;
            set => SetProperty(ref _fuelCostPercentage, value);
        }

        public decimal MaintenanceCostDifference
        {
            get => _maintenanceCostDifference;
            set => SetProperty(ref _maintenanceCostDifference, value);
        }

        public decimal MaintenancePercentage
        {
            get => _maintenancePercentage;
            set => SetProperty(ref _maintenancePercentage, value);
        }

        public decimal MileageDifference
        {
            get => _mileageDifference;
            set => SetProperty(ref _mileageDifference, value);
        }

        public decimal MileagePercentage
        {
            get => _mileagePercentage;
            set => SetProperty(ref _mileagePercentage, value);
        }

        public decimal ConsumptionDifference
        {
            get => _consumptionDifference;
            set => SetProperty(ref _consumptionDifference, value);
        }

        public decimal ConsumptionPercentage
        {
            get => _consumptionPercentage;
            set => SetProperty(ref _consumptionPercentage, value);
        }

        public IEnumerable<ISeries> ComparisonSeries
        {
            get => _comparisonSeries;
            set => SetProperty(ref _comparisonSeries, value);
        }

        public IEnumerable<Axis> ComparisonXAxes
        {
            get => _comparisonXAxes;
            set => SetProperty(ref _comparisonXAxes, value);
        }

        public IEnumerable<Axis> ComparisonYAxes
        {
            get => _comparisonYAxes;
            set => SetProperty(ref _comparisonYAxes, value);
        }

        public ObservableCollection<ComparisonDetail> ComparisonDetails
        {
            get => _comparisonDetails;
            set => SetProperty(ref _comparisonDetails, value);
        }

        public DateTime ComparisonDate
        {
            get => _comparisonDate;
            set => SetProperty(ref _comparisonDate, value);
        }

        #endregion

        #region Commands

        public ICommand CompareCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        private void InitializeCommands()
        {
            CompareCommand = new RelayCommand(_ => _ = ComparePeriodsAsync());
            ExportCommand = new RelayCommand(_ => _ = ExportComparisonAsync());
            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        #endregion

        #region Methods

        private void InitializePredefinedPeriods()
        {
            PredefinedPeriods = new List<string>
            {
                "Mois actuel",
                "Mois dernier",
                "3 derniers mois",
                "6 derniers mois",
                "Année en cours",
                "Année dernière",
                "Trimestre actuel",
                "Trimestre dernier"
            };
        }

        private void ApplyPredefinedPeriod(string period, bool isPeriod1)
        {
            var now = DateTime.Now;
            DateTime start, end;

            switch (period)
            {
                case "Mois actuel":
                    start = new DateTime(now.Year, now.Month, 1);
                    end = now;
                    break;
                case "Mois dernier":
                    start = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                    end = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                    break;
                case "3 derniers mois":
                    start = now.AddMonths(-3);
                    end = now;
                    break;
                case "6 derniers mois":
                    start = now.AddMonths(-6);
                    end = now;
                    break;
                case "Année en cours":
                    start = new DateTime(now.Year, 1, 1);
                    end = now;
                    break;
                case "Année dernière":
                    start = new DateTime(now.Year - 1, 1, 1);
                    end = new DateTime(now.Year - 1, 12, 31);
                    break;
                case "Trimestre actuel":
                    var quarter = (now.Month - 1) / 3;
                    start = new DateTime(now.Year, quarter * 3 + 1, 1);
                    end = now;
                    break;
                case "Trimestre dernier":
                    var lastQuarter = (now.Month - 1) / 3 - 1;
                    if (lastQuarter < 0)
                    {
                        lastQuarter = 3;
                        start = new DateTime(now.Year - 1, lastQuarter * 3 + 1, 1);
                        end = new DateTime(now.Year - 1, 12, 31);
                    }
                    else
                    {
                        start = new DateTime(now.Year, lastQuarter * 3 + 1, 1);
                        end = new DateTime(now.Year, lastQuarter * 3 + 3, 1).AddMonths(1).AddDays(-1);
                    }
                    break;
                default:
                    return;
            }

            if (isPeriod1)
            {
                Period1StartDate = start;
                Period1EndDate = end;
            }
            else
            {
                Period2StartDate = start;
                Period2EndDate = end;
            }
        }

        private async Task ComparePeriodsAsync()
        {
            if (!Period1StartDate.HasValue || !Period1EndDate.HasValue ||
                !Period2StartDate.HasValue || !Period2EndDate.HasValue)
            {
                MessageBox.Show("Veuillez sélectionner les deux périodes à comparer.",
                    "Périodes manquantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                // Charger les statistiques pour les deux périodes
                var period1Stats = await LoadPeriodStatisticsAsync(Period1StartDate.Value, Period1EndDate.Value);
                var period2Stats = await LoadPeriodStatisticsAsync(Period2StartDate.Value, Period2EndDate.Value);

                // Calculer les différences
                CalculateDifferences(period1Stats, period2Stats);

                // Générer les graphiques
                GenerateComparisonCharts(period1Stats, period2Stats);

                // Créer le tableau détaillé
                CreateComparisonDetails(period1Stats, period2Stats);

                ComparisonDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la comparaison : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<PeriodStatistics> LoadPeriodStatisticsAsync(DateTime start, DateTime end)
        {
            // Simulation - À remplacer par vraies requêtes
            await Task.Delay(500);

            var random = new Random();
            return new PeriodStatistics
            {
                FuelCost = random.Next(5000, 15000),
                MaintenanceCost = random.Next(2000, 8000),
                TotalMileage = random.Next(10000, 50000),
                AverageConsumption = (decimal)(random.NextDouble() * 3 + 6),
                TotalLiters = random.Next(1000, 5000),
                VehicleCount = random.Next(10, 25)
            };
        }

        private void CalculateDifferences(PeriodStatistics period1, PeriodStatistics period2)
        {
            FuelCostDifference = period2.FuelCost - period1.FuelCost;
            FuelCostPercentage = period1.FuelCost > 0 ? (FuelCostDifference / period1.FuelCost) * 100 : 0;

            MaintenanceCostDifference = period2.MaintenanceCost - period1.MaintenanceCost;
            MaintenancePercentage = period1.MaintenanceCost > 0 ? (MaintenanceCostDifference / period1.MaintenanceCost) * 100 : 0;

            MileageDifference = period2.TotalMileage - period1.TotalMileage;
            MileagePercentage = period1.TotalMileage > 0 ? (MileageDifference / period1.TotalMileage) * 100 : 0;

            ConsumptionDifference = period2.AverageConsumption - period1.AverageConsumption;
            ConsumptionPercentage = period1.AverageConsumption > 0 ? (ConsumptionDifference / period1.AverageConsumption) * 100 : 0;
        }

        private void GenerateComparisonCharts(PeriodStatistics period1, PeriodStatistics period2)
        {
            ComparisonSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Période 1",
                    Values = new[] { period1.FuelCost, period1.MaintenanceCost, period1.TotalMileage / 100 },
                    Fill = new SolidColorPaint(new SKColor(33, 150, 243)),
                    Stroke = new SolidColorPaint(new SKColor(25, 118, 210), 1)
                },
                new ColumnSeries<decimal>
                {
                    Name = "Période 2",
                    Values = new[] { period2.FuelCost, period2.MaintenanceCost, period2.TotalMileage / 100 },
                    Fill = new SolidColorPaint(new SKColor(255, 152, 0)),
                    Stroke = new SolidColorPaint(new SKColor(245, 124, 0), 1)
                }
            };

            ComparisonXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[] { "Carburant (€)", "Maintenance (€)", "Kilométrage (x100)" },
                    LabelsRotation = 15
                }
            };

            ComparisonYAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Valeur",
                    NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
                }
            };
        }

        private void CreateComparisonDetails(PeriodStatistics period1, PeriodStatistics period2)
        {
            ComparisonDetails = new ObservableCollection<ComparisonDetail>
            {
                new ComparisonDetail
                {
                    Indicator = "Coût carburant",
                    Period1Value = $"{period1.FuelCost:C}",
                    Period2Value = $"{period2.FuelCost:C}",
                    Difference = $"{FuelCostDifference:C}",
                    PercentageChange = FuelCostPercentage,
                    Trend = FuelCostDifference >= 0 ? "📈" : "📉"
                },
                new ComparisonDetail
                {
                    Indicator = "Coût maintenance",
                    Period1Value = $"{period1.MaintenanceCost:C}",
                    Period2Value = $"{period2.MaintenanceCost:C}",
                    Difference = $"{MaintenanceCostDifference:C}",
                    PercentageChange = MaintenancePercentage,
                    Trend = MaintenanceCostDifference >= 0 ? "📈" : "📉"
                },
                new ComparisonDetail
                {
                    Indicator = "Kilométrage total",
                    Period1Value = $"{period1.TotalMileage:N0} km",
                    Period2Value = $"{period2.TotalMileage:N0} km",
                    Difference = $"{MileageDifference:N0} km",
                    PercentageChange = MileagePercentage,
                    Trend = MileageDifference >= 0 ? "📈" : "📉"
                },
                new ComparisonDetail
                {
                    Indicator = "Consommation moyenne",
                    Period1Value = $"{period1.AverageConsumption:F2} L/100km",
                    Period2Value = $"{period2.AverageConsumption:F2} L/100km",
                    Difference = $"{ConsumptionDifference:F2} L/100km",
                    PercentageChange = ConsumptionPercentage,
                    Trend = ConsumptionDifference >= 0 ? "📈" : "📉"
                },
                new ComparisonDetail
                {
                    Indicator = "Litres consommés",
                    Period1Value = $"{period1.TotalLiters:N0} L",
                    Period2Value = $"{period2.TotalLiters:N0} L",
                    Difference = $"{period2.TotalLiters - period1.TotalLiters:N0} L",
                    PercentageChange = period1.TotalLiters > 0 ? ((period2.TotalLiters - period1.TotalLiters) / period1.TotalLiters) * 100 : 0,
                    Trend = period2.TotalLiters >= period1.TotalLiters ? "📈" : "📉"
                }
            };
        }

        private async Task ExportComparisonAsync()
        {
            MessageBox.Show("Export de la comparaison - Fonctionnalité à implémenter",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseWindow()
        {
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        #endregion
    }

    // Classes auxiliaires
    public class PeriodStatistics
    {
        public decimal FuelCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal TotalMileage { get; set; }
        public decimal AverageConsumption { get; set; }
        public decimal TotalLiters { get; set; }
        public int VehicleCount { get; set; }
    }

    public class ComparisonDetail
    {
        public string Indicator { get; set; } = string.Empty;
        public string Period1Value { get; set; } = string.Empty;
        public string Period2Value { get; set; } = string.Empty;
        public string Difference { get; set; } = string.Empty;
        public decimal PercentageChange { get; set; }
        public string Trend { get; set; } = string.Empty;
    }
}
